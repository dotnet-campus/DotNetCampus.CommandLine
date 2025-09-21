using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.CodeAnalysis;

/// <summary>
/// 命令行属性的类型信息。
/// </summary>
internal record CommandPropertyTypeInfo
{
    public CommandPropertyTypeInfo(ITypeSymbol typeSymbol)
    {
        TypeSymbol = typeSymbol.GetNotNullTypeSymbol();
        Kind = GetSymbolInfoAsCommandProperty(typeSymbol);
    }

    /// <summary>
    /// 获取类型的简单名称，仅包含名称本身，不包含命名空间、泛型参数、可空标记等信息。
    /// </summary>
    /// <returns></returns>
    public string GetSimpleName() => TypeSymbol.ToDisplayString(SimpleNameFormat);

    public ITypeSymbol TypeSymbol { get; }

    public CommandValueKind Kind { get; }

    /// <summary>
    /// 获取当前类型是否可以从数组或列表赋值。
    /// </summary>
    /// <returns>如果可以从数组或列表赋值，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public bool IsAssignableFromArrayOrList()
    {
        if (Kind is not CommandValueKind.List)
        {
            return false;
        }

        if (TypeSymbol.Kind is SymbolKind.ArrayType)
        {
            return true;
        }

        var simpleName = GetSimpleName();
        return AllowedArrayOrListTypeNames.Contains(simpleName);
    }

    /// <summary>
    /// 如果当前类型是枚举类型，则返回其枚举类型符号，否则返回 <see langword="null"/>。
    /// </summary>
    /// <returns>枚举类型符号，或 <see langword="null"/>。</returns>
    public ITypeSymbol? AsEnumSymbol()
    {
        return Kind is not CommandValueKind.Enum ? null : TypeSymbol;
    }

    /// <summary>
    /// 获取类型的非抽象名称。<br/>
    /// 对于命令行解析中所支持的各种接口，会被映射为其常见的具体类型名称。
    /// </summary>
    /// <returns>非抽象名称。</returns>
    public string GetGeneratedNotAbstractTypeName()
    {
        if (TypeSymbol.Kind is SymbolKind.ArrayType)
        {
            return "Array";
        }

        return Kind switch
        {
            CommandValueKind.Boolean => TypeSymbol.ToDisplayString(SimpleDeclarationNameFormat),
            CommandValueKind.Number => TypeSymbol.ToDisplayString(SimpleDeclarationNameFormat),
            CommandValueKind.Enum => "Enum",
            CommandValueKind.String => TypeSymbol.ToDisplayString(SimpleDeclarationNameFormat),
            CommandValueKind.List => AllowedListTypeNames.TryGetValue(GetSimpleName(), out var list)
                ? list
                : "List",
            CommandValueKind.Dictionary => AllowedDictionaryTypeNames.TryGetValue(GetSimpleName(), out var dict)
                ? dict
                : "Dictionary",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// 允许的单泛型类型名称。
    /// </summary>
    /// <summary>
    /// 允许的单泛型类型名称。
    /// </summary>
    private static readonly Dictionary<string, string> AllowedListTypeNames = new Dictionary<string, string>
    {
        ["Collection"] = "Collection",
        ["HashSet"] = "HashSet",
        ["ICollection"] = "List",
        ["IEnumerable"] = "List",
        ["IImmutableList"] = "ImmutableList",
        ["IImmutableSet"] = "ImmutableHashSet",
        ["IList"] = "List",
        ["ImmutableArray"] = "ImmutableArray",
        ["ImmutableHashSet"] = "ImmutableHashSet",
        ["ImmutableList"] = "ImmutableList",
        ["ImmutableSortedSet"] = "ImmutableSortedSet",
        ["IReadOnlyCollection"] = "List",
        ["IReadOnlyList"] = "List",
        ["ISet"] = "HashSet",
        ["List"] = "List",
        ["ReadOnlyCollection"] = "ReadOnlyCollection",
        ["SortedSet"] = "SortedSet",
    };

    /// <summary>
    /// 允许的双泛型类型名称。
    /// </summary>
    private static readonly Dictionary<string, string> AllowedDictionaryTypeNames = new Dictionary<string, string>
    {
        ["Dictionary"] = "Dictionary",
        ["IDictionary"] = "Dictionary",
        ["ImmutableDictionary"] = "ImmutableDictionary",
        ["ImmutableSortedDictionary"] = "ImmutableSortedDictionary",
        ["IReadOnlyDictionary"] = "Dictionary",
        ["KeyValuePair"] = "KeyValuePair",
        ["SortedDictionary"] = "SortedDictionary",
    };

    /// <summary>
    /// 允许的 RawArguments 泛型类型名称。
    /// </summary>
    private static readonly HashSet<string> AllowedArrayOrListTypeNames =
    [
        "IList", "IReadOnlyList", "ICollection", "IReadOnlyCollection", "IEnumerable",
    ];

    /// <summary>
    /// 用于将类型符号转换为仅包含名称的字符串形式。会去掉可空标记、命名空间、泛型参数等信息。
    /// </summary>
    private static readonly SymbolDisplayFormat SimpleNameFormat = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        genericsOptions: SymbolDisplayGenericsOptions.None,
        kindOptions: SymbolDisplayKindOptions.None,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    private static readonly SymbolDisplayFormat SimpleDeclarationNameFormat = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        genericsOptions: SymbolDisplayGenericsOptions.None,
        kindOptions: SymbolDisplayKindOptions.None
    );

    /// <summary>
    /// 视 <paramref name="typeSymbol"/> 为命令行属性的类型，按命令行属性的要求获取其所需的类型信息。<br/>
    /// 这个过程会丢掉类型的可空性信息。
    /// </summary>
    /// <param name="typeSymbol">类型符号。</param>
    /// <returns>类型信息。</returns>
    private static CommandValueKind GetSymbolInfoAsCommandProperty(ITypeSymbol typeSymbol)
    {
        var notNullTypeSymbol = typeSymbol.GetNotNullTypeSymbol();

        switch (notNullTypeSymbol.SpecialType)
        {
            case SpecialType.System_Boolean:
                return CommandValueKind.Boolean;
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Decimal:
            case SpecialType.System_Double:
            case SpecialType.System_Single:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            // 不应支持这种不能跨进程传递的类型。
            // case SpecialType.System_IntPtr:
            // case SpecialType.System_UIntPtr:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
                return CommandValueKind.Number;
            case SpecialType.System_Char:
            case SpecialType.System_String:
                return CommandValueKind.String;
            case SpecialType.System_Array:
            case SpecialType.System_Collections_IEnumerable:
            case SpecialType.System_Collections_Generic_IEnumerable_T:
            case SpecialType.System_Collections_Generic_IList_T:
            case SpecialType.System_Collections_Generic_ICollection_T:
            case SpecialType.System_Collections_IEnumerator:
            case SpecialType.System_Collections_Generic_IEnumerator_T:
            case SpecialType.System_Collections_Generic_IReadOnlyList_T:
            case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                return CommandValueKind.List;
            case SpecialType.None:
                // 其他类型，进行后续分析。
                break;
            default:
                return CommandValueKind.Unknown;
        }

        if (notNullTypeSymbol.TypeKind is TypeKind.Enum)
        {
            return CommandValueKind.Enum;
        }

        // List
        if (typeSymbol is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_String })
        {
            return CommandValueKind.List;
        }

        // List
        if (notNullTypeSymbol is INamedTypeSymbol
            {
                TypeArguments: [{ SpecialType: SpecialType.System_String }],
                OriginalDefinition.Name: { } oneGenericName,
            } && AllowedListTypeNames.ContainsKey(oneGenericName))
        {
            return CommandValueKind.List;
        }

        // Dictionary
        if (notNullTypeSymbol is INamedTypeSymbol
            {
                TypeArguments: [{ SpecialType: SpecialType.System_String }, { SpecialType: SpecialType.System_String }],
                OriginalDefinition.Name: { } twoGenericName,
            } && AllowedDictionaryTypeNames.ContainsKey(twoGenericName))
        {
            return CommandValueKind.Dictionary;
        }

        return CommandValueKind.Unknown;
    }
}

public static class CommandPropertyTypeInfoExtensions
{
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
}
