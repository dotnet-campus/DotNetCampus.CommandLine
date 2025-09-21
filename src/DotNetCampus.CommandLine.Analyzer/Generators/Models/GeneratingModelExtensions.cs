using DotNetCampus.CommandLine.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

/// <summary>
/// 为源生成器使用的数据模型提供扩展方法。
/// </summary>
internal static class GeneratingModelExtensions
{
    public static string ToCommandValueTypeName(this CommandValueKind type) => type switch
    {
        CommandValueKind.Boolean => "global::DotNetCampus.Cli.Compiler.OptionValueType.Boolean",
        CommandValueKind.List => "global::DotNetCampus.Cli.Compiler.OptionValueType.List",
        CommandValueKind.Dictionary => "global::DotNetCampus.Cli.Compiler.OptionValueType.Dictionary",
        _ => "global::DotNetCampus.Cli.Compiler.OptionValueType.Normal",
    };

    /// <summary>
    /// 视 <paramref name="typeSymbol"/> 为命令行属性的类型，按命令行属性的要求获取其所需的类型信息。<br/>
    /// 这个过程会丢掉类型的可空性信息。
    /// </summary>
    /// <param name="typeSymbol">类型符号。</param>
    /// <returns>类型信息。</returns>
    public static CommandPropertyTypeInfo GetSymbolInfoAsCommandProperty(this ITypeSymbol typeSymbol)
    {
        return new CommandPropertyTypeInfo(typeSymbol);
    }

    /// <summary>
    /// 获取类型的非抽象名称。<br/>
    /// 对于命令行解析中所支持的各种接口，会被映射为其常见的具体类型名称。
    /// </summary>
    /// <param name="typeSymbol">类型符号。</param>
    /// <returns>非抽象名称。</returns>
    public static string GetGeneratedNotAbstractTypeName(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetSymbolInfoAsCommandProperty().GetGeneratedNotAbstractTypeName();
    }

    /// <summary>
    /// 假定 <paramref name="symbol"/> 是一个命令行对象中一个枚举属性的属性类型，
    /// 现在我们要为这个枚举生成一个用来赋值命令行值的辅助类型，
    /// 此方法返回这个辅助类型的名称。
    /// </summary>
    /// <param name="symbol">命令行对象中一个枚举属性的属性类型。</param>
    /// <returns>辅助类型的名称。</returns>
    public static string GetGeneratedEnumArgumentTypeName(this ITypeSymbol symbol)
    {
        return symbol.GetSymbolInfoAsCommandProperty().AsEnumSymbol() is { } enumTypeSymbol
            ? $"__GeneratedEnumArgument__{enumTypeSymbol.ToDisplayString().Replace('.', '_')}__"
            : symbol.ToDisplayString();
    }

    /// <summary>
    /// 将类型符号映射为命令行值的种类。
    /// </summary>
    /// <param name="typeSymbol">类型符号。</param>
    /// <returns>命令行值的种类。</returns>
    public static CommandValueKind AsCommandValueKind(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetSymbolInfoAsCommandProperty().Kind;
    }

    /// <summary>
    /// 判断类型是否确定支持集合表达式（Collection Expression）。
    /// </summary>
    /// <param name="typeSymbol">要检查的类型符号。</param>
    /// <param name="supportImmutableCollections">当前框架是否支持不可变集合类型（如 <c>ImmutableList</c> 和 <c>ImmutableHashSet</c>）。</param>
    /// <returns>如果类型确定支持集合表达式，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public static bool SupportCollectionExpression(this ITypeSymbol typeSymbol, bool supportImmutableCollections)
    {
        var info = typeSymbol.GetSymbolInfoAsCommandProperty();
        if (info.Kind is not CommandValueKind.List)
        {
            return false;
        }

        // 不可变集合在 .NET 8 及以上版本中支持集合表达式。
        // 其他类型均直接支持集合表达式。
        var simpleName = info.GetSimpleDeclarationName();
        return !simpleName.Contains("Immutable") || supportImmutableCollections;
    }
}
