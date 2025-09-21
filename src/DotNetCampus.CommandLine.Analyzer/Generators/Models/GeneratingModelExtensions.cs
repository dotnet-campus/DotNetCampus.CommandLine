using DotNetCampus.CommandLine.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

/// <summary>
/// 为源生成器使用的数据模型提供扩展方法。
/// </summary>
internal static class GeneratingModelExtensions
{
    private static readonly SymbolDisplayFormat ToTargetTypeFormat = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        genericsOptions: SymbolDisplayGenericsOptions.None,
        kindOptions: SymbolDisplayKindOptions.None
    );

    public static string ToCommandValueTypeName(this CommandValueKind type) => type switch
    {
        CommandValueKind.Boolean => "global::DotNetCampus.Cli.OptionValueType.Boolean",
        CommandValueKind.List => "global::DotNetCampus.Cli.OptionValueType.List",
        CommandValueKind.Dictionary => "global::DotNetCampus.Cli.OptionValueType.Dictionary",
        _ => "global::DotNetCampus.Cli.OptionValueType.Normal",
    };

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
        if (typeSymbol.Kind is SymbolKind.ArrayType)
        {
            return true;
        }

        var originalDefinitionString = typeSymbol.OriginalDefinition.ToString();
        if (originalDefinitionString.Equals("System.Nullable<T>", StringComparison.Ordinal))
        {
            // Nullable<T> 类型
            var genericType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
            return SupportCollectionExpression(genericType, supportImmutableCollections);
        }

        return typeSymbol.ToDisplayString(ToTargetTypeFormat) switch
        {
            "IList" or "ICollection" or "IEnumerable" or "IReadOnlyList" or "IReadOnlyCollection" or "ISet"
                or "IImmutableSet" or "IImmutableList" => true,
            "List" or "Collection" or "HashSet" => true,
            // 不可变集合在 .NET 8 及以上版本中支持集合表达式。
            "ImmutableArray" or "ImmutableHashSet" => supportImmutableCollections,
            _ => false,
        };
    }
}
