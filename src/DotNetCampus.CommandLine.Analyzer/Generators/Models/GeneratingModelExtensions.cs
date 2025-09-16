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

    /// <summary>
    /// 假定 <paramref name="symbol"/> 是一个命令行对象中一个枚举属性的属性类型，
    /// 现在我们要为这个枚举生成一个用来赋值命令行值的辅助类型，
    /// 此方法返回这个辅助类型的名称。
    /// </summary>
    /// <param name="symbol">命令行对象中一个枚举属性的属性类型。</param>
    /// <returns>辅助类型的名称。</returns>
    public static string GetGeneratedEnumArgumentTypeName(this ITypeSymbol symbol)
    {
        string typeName;

        if (symbol is { IsValueType: true, OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } typeSymbol)
        {
            typeName = typeSymbol is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } namedType
                // 获取 Nullable<T> 中的 T。
                ? namedType.TypeArguments[0].ToDisplayString()
                // 处理直接带有可空标记的类型 (int? 这种形式)。
                : typeSymbol.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString();
        }
        else
        {
            typeName = symbol.ToDisplayString();
        }

        return $"__GeneratedEnumArgument__{typeName.Replace('.', '_')}__";
    }

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
    public static string ToCommandValueNonAbstractName(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.Kind is SymbolKind.ArrayType)
        {
            return "Array";
        }

        var originalDefinitionString = typeSymbol.OriginalDefinition.ToString();
        if (originalDefinitionString.Equals("System.Nullable<T>", StringComparison.Ordinal))
        {
            // Nullable<T> 类型
            var genericType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
            return ToCommandValueNonAbstractName(genericType);
        }

        // 取出类型的 .NET 类名称，不含泛型。如 bool 返回 Boolean，Dictionary<string, string> 返回 Dictionary。
        return typeSymbol.ToDisplayString(ToTargetTypeFormat) switch
        {
            "IList" or "ICollection" or "IEnumerable" or "IReadOnlyList" or "IReadOnlyCollection" or "ISet"
                or "IImmutableSet" or "IImmutableList" => "List",
            "IDictionary" or "IReadOnlyDictionary" => "Dictionary",
            var name => name,
        };
    }

    /// <summary>
    /// 将类型符号映射为命令行值的种类。
    /// </summary>
    /// <param name="typeSymbol">类型符号。</param>
    /// <returns>命令行值的种类。</returns>
    public static CommandValueKind AsCommandValueKind(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.SpecialType is SpecialType.System_Boolean)
        {
            return CommandValueKind.Boolean;
        }

        if (typeSymbol.SpecialType is SpecialType.System_Byte or
            SpecialType.System_SByte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Decimal)
        {
            return CommandValueKind.Number;
        }

        if (typeSymbol.TypeKind is TypeKind.Enum)
        {
            return CommandValueKind.Enum;
        }

        if (typeSymbol.SpecialType is SpecialType.System_String)
        {
            return CommandValueKind.String;
        }

        if (typeSymbol.Kind is SymbolKind.ArrayType)
        {
            return CommandValueKind.List;
        }

        var originalDefinitionString = typeSymbol.OriginalDefinition.ToString();
        if (originalDefinitionString.Equals("System.Nullable<T>", StringComparison.Ordinal))
        {
            // Nullable<T> 类型
            var genericType = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
            return AsCommandValueKind(genericType);
        }

        return typeSymbol.ToDisplayString(ToTargetTypeFormat) switch
        {
            "IList" or "ICollection" or "IEnumerable" or "IReadOnlyList" or "IReadOnlyCollection" or "ISet"
                or "IImmutableSet" or "IImmutableList" => CommandValueKind.List,
            "ImmutableArray" or "List" or "ImmutableHashSet" or "Collection" or "HashSet" => CommandValueKind.List,
            "IDictionary" or "IReadOnlyDictionary" => CommandValueKind.Dictionary,
            "ImmutableDictionary" or "Dictionary" or "KeyValuePair" => CommandValueKind.Dictionary,
            _ => CommandValueKind.Unknown,
        };
    }
}
