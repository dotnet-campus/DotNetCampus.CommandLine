using System.Reflection;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.CommandLine;

internal static class GeneratorInfo
{
    public static string RootNamespace { get; } = typeof(GeneratorInfo).Namespace!;

    public static string ToolName { get; } = typeof(GeneratorInfo).Assembly
        .GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? typeof(GeneratorInfo).Namespace!;

    public static string ToolVersion { get; } = typeof(GeneratorInfo).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

    private static readonly SymbolDisplayFormat GlobalDisplayFormat = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    private static readonly SymbolDisplayFormat GlobalTypeOfDisplayFormat = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.None,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static string ToGlobalDisplayString(this ISymbol symbol)
    {
        return symbol.ToDisplayString(GlobalDisplayFormat);
    }

    public static string ToGlobalTypeOfDisplayString(this INamedTypeSymbol symbol)
    {
        var name = symbol.ToDisplayString(GlobalTypeOfDisplayFormat);
        return symbol.IsGenericType ? $"{name}<{new string(',', symbol.TypeArguments.Length - 1)}>" : name;
    }
}
