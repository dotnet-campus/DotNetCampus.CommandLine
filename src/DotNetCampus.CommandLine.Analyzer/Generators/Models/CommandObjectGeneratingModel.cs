using DotNetCampus.Cli.Utils;
using DotNetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

internal record CommandObjectGeneratingModel
{
    private static readonly SymbolDisplayFormat SimpleContainingTypeFormat = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    /// <summary>
    /// 命令行对象类型所在的命名空间。如果类型位于全局命名空间（例如顶级语句所在的项目中未声明命名空间的类型），则为 <see langword="null"/>。
    /// </summary>
    public required string? Namespace { get; init; }

    public required INamedTypeSymbol CommandObjectType { get; init; }

    public required bool IsPublic { get; init; }

    public required string? CommandNames { get; init; }

    public required string? Description { get; init; }

    public required bool UseFullStackParser { get; init; }

    public required bool IsHandler { get; init; }

    public required IReadOnlyList<RawArgumentPropertyGeneratingModel> RawArgumentsProperties { get; init; }

    public required IReadOnlyList<OptionalArgumentPropertyGeneratingModel> OptionProperties { get; init; }

    public required IReadOnlyList<PositionalArgumentPropertyGeneratingModel> PositionalArgumentProperties { get; init; }

    public string GetBuilderTypeName()
    {
        return GetBuilderTypeName(CommandObjectType);
    }

    public static string GetBuilderTypeName(INamedTypeSymbol commandObjectType)
    {
        var name = commandObjectType.ToDisplayString(SimpleContainingTypeFormat).Replace('.', '_');
        return $"{name}Builder";
    }

    /// <summary>
    /// 获取创建器类型带 global:: 前缀的完整名称。支持命令行对象位于全局命名空间的情况。
    /// </summary>
    public string GetGlobalBuilderTypeName()
    {
        return GetGlobalBuilderTypeName(CommandObjectType);
    }

    /// <summary>
    /// 获取创建器类型带 global:: 前缀的完整名称。支持命令行对象位于全局命名空间的情况。
    /// </summary>
    public static string GetGlobalBuilderTypeName(INamedTypeSymbol commandObjectType)
    {
        var typeName = GetBuilderTypeName(commandObjectType);
        return commandObjectType.GetNamespaceOrNull() is { } @namespace
            ? $"global::{@namespace}.{typeName}"
            : $"global::{typeName}";
    }

    public int GetCommandLevel() => CommandNames switch
    {
        null => 0,
        { } names => names.Count(x => x == ' ') + 1,
    };

    public string? GetPascalCaseCommandNames()
    {
        if (CommandNames is not { } commandNames)
        {
            return null;
        }
        return string.Join(" ", commandNames.Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(NamingHelper.MakePascalCase));
    }

    public IEnumerable<PositionalArgumentPropertyGeneratingModel> EnumeratePositionalArgumentExcludingSameNameOptions()
    {
        var optionNames = new HashSet<string>(OptionProperties.Select(x => x.PropertyName));
        foreach (var positionalArgumentProperty in PositionalArgumentProperties)
        {
            if (!optionNames.Contains(positionalArgumentProperty.PropertyName))
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
            if (option.Type.GetSymbolInfoAsCommandProperty().AsEnumSymbol() is { } enumTypeSymbol)
            {
                enums.Add(enumTypeSymbol);
            }
        }
        foreach (var value in PositionalArgumentProperties)
        {
            if (value.Type.GetSymbolInfoAsCommandProperty().AsEnumSymbol() is { } enumTypeSymbol)
            {
                enums.Add(enumTypeSymbol);
            }
        }
        return enums;
    }
}
