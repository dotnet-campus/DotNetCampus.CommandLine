using DotNetCampus.Cli.Utils;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

internal record CommandObjectGeneratingModel
{
    public required string Namespace { get; init; }

    public required INamedTypeSymbol CommandObjectType { get; init; }

    public required bool IsPublic { get; init; }

    public required string? CommandNames { get; init; }

    public required bool UseFullStackParser { get; init; }

    public required bool IsHandler { get; init; }

    public required IReadOnlyList<RawArgumentPropertyGeneratingModel> RawArgumentsProperties { get; init; }

    public required IReadOnlyList<OptionalArgumentPropertyGeneratingModel> OptionProperties { get; init; }

    public required IReadOnlyList<PositionalArgumentPropertyGeneratingModel> PositionalArgumentProperties { get; init; }

    public string GetBuilderTypeName() => GetBuilderTypeName(CommandObjectType);

    public static string GetBuilderTypeName(INamedTypeSymbol commandObjectType)
    {
        return $"{commandObjectType.Name}Builder";
    }

    public int GetCommandLevel() => CommandNames switch
    {
        null => 0,
        { } names => names.Count(x => x == ' ') + 1,
    };

    public IEnumerable<PositionalArgumentPropertyGeneratingModel> EnumeratePositionalArgumentPropertiesExcludingSameNameOptions()
    {
        var optionNames = OptionProperties.Select(x => x.PropertyName).ToList();
        foreach (var positionalArgumentProperty in PositionalArgumentProperties)
        {
            if (!optionNames.Contains(positionalArgumentProperty.PropertyName, StringComparer.Ordinal))
            {
                yield return positionalArgumentProperty;
            }
        }
    }

    public string? GetKebabCaseCommandNames()
    {
        if (CommandNames is not { } commandNames)
        {
            return null;
        }
        return string.Join(" ", commandNames.Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => NamingHelper.MakeKebabCase(x, false, false)));
    }

    public IEnumerable<ITypeSymbol> EnumerateEnumPropertyTypes()
    {
        var enums = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var option in OptionProperties)
        {
            if (option.Type.TypeKind is TypeKind.Enum)
            {
                enums.Add(option.Type);
            }
        }
        foreach (var value in PositionalArgumentProperties)
        {
            if (value.Type.TypeKind is TypeKind.Enum)
            {
                enums.Add(value.Type);
            }
        }
        return enums;
    }
}
