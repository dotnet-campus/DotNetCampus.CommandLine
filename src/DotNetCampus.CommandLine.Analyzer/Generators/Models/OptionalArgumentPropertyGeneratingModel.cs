using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Utils;
using DotNetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

internal sealed record OptionalArgumentPropertyGeneratingModel : PropertyGeneratingModel
{
    private OptionalArgumentPropertyGeneratingModel(IPropertySymbol propertySymbol) : base(propertySymbol)
    {
    }

    public required IReadOnlyList<string> ShortNames { get; init; }

    public required IReadOnlyList<string> LongNames { get; init; }

    public required bool? CaseSensitive { get; init; }

    public int PropertyIndex { get; set; } = -1;

    /// <summary>
    /// 返回开发者定义的长选项名称列表，按定义顺序返回。<br/>
    /// 如果没有定义，则返回 kebab-case 风格的属性名作为默认名称；
    /// 如果有定义，无论定义了什么，都视其为 kebab-case 风格的名称。
    /// </summary>
    public IReadOnlyList<string> GetOrdinalLongNames()
    {
        List<string> list = [];
        if (LongNames.Count is 0)
        {
            list.Add(NamingHelper.MakeKebabCase(PropertyName));
        }
        else
        {
            foreach (var longName in LongNames)
            {
                if (!string.IsNullOrEmpty(longName) && !list.Contains(longName, StringComparer.Ordinal))
                {
                    list.Add(longName);
                }
            }
        }
        return list;
    }

    public IReadOnlyList<string> GetPascalCaseLongNames()
    {
        List<string> list = [];
        if (LongNames.Count is 0)
        {
            list.Add(PropertyName);
        }
        else
        {
            foreach (var longName in LongNames)
            {
                if (!string.IsNullOrEmpty(longName))
                {
                    var pascalCase = NamingHelper.MakePascalCase(longName);
                    if (!list.Contains(pascalCase, StringComparer.Ordinal))
                    {
                        list.Add(pascalCase);
                    }
                }
            }
        }
        return list;
    }

    public IReadOnlyList<string> GetShortNames()
    {
        List<string> list = [];
        foreach (var shortName in ShortNames)
        {
            if (!string.IsNullOrEmpty(shortName) && !list.Contains(shortName, StringComparer.Ordinal))
            {
                list.Add(shortName);
            }
        }
        return list;
    }

    public static OptionalArgumentPropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
    {
        var optionAttribute = propertySymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<OptionAttribute>());
        if (optionAttribute is null)
        {
            return null;
        }

        List<string> shortNames = [];
        List<string> longNames = [];

        if (optionAttribute.ConstructorArguments.Length is 0)
        {
            // 没有构造函数参数时，不设置任何名称。
        }
        else if (optionAttribute.ConstructorArguments.Length is 1)
        {
            // 只有一个构造函数参数时，要么是短名称（一定是字符），要么是长名称（一定是字符串）。
            var arg = optionAttribute.ConstructorArguments[0];
            if (arg.Type?.SpecialType is SpecialType.System_Char)
            {
                var shortName = arg.Value?.ToString();
                if (!string.IsNullOrEmpty(shortName))
                {
                    shortNames.Add(shortName!);
                }
            }
            else if (arg.Type?.SpecialType is SpecialType.System_String)
            {
                var longName = arg.Value?.ToString();
                if (!string.IsNullOrEmpty(longName))
                {
                    longNames.Add(longName!);
                }
            }
        }
        else if (optionAttribute.ConstructorArguments.Length is 2)
        {
            // 有两个构造函数参数时，第一个参数是短名称（字符、字符串、字符串数组），第二个参数是长名称（字符串、字符串数组）。
            var shortArg = optionAttribute.ConstructorArguments[0];
            if (shortArg.Type?.SpecialType is SpecialType.System_Char)
            {
                var shortName = shortArg.Value?.ToString();
                if (!string.IsNullOrEmpty(shortName))
                {
                    shortNames.Add(shortName!);
                }
            }
            else if (shortArg.Type?.SpecialType is SpecialType.System_String)
            {
                var shortName = shortArg.Value?.ToString();
                if (!string.IsNullOrEmpty(shortName))
                {
                    shortNames.Add(shortName!);
                }
            }
            else if (shortArg.Kind is TypedConstantKind.Array)
            {
                foreach (var value in shortArg.Values)
                {
                    var shortName = value.Value?.ToString();
                    if (!string.IsNullOrEmpty(shortName) && !shortNames.Contains(shortName, StringComparer.Ordinal))
                    {
                        shortNames.Add(shortName!);
                    }
                }
            }
            var longArg = optionAttribute.ConstructorArguments[1];
            if (longArg.Type?.SpecialType is SpecialType.System_String)
            {
                var longName = longArg.Value?.ToString();
                if (!string.IsNullOrEmpty(longName))
                {
                    longNames.Add(longName!);
                }
            }
            else if (longArg.Kind is TypedConstantKind.Array)
            {
                foreach (var value in longArg.Values)
                {
                    var longName = value.Value?.ToString();
                    if (!string.IsNullOrEmpty(longName) && !longNames.Contains(longName, StringComparer.Ordinal))
                    {
                        longNames.Add(longName!);
                    }
                }
            }
        }

        var caseSensitive = optionAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(OptionAttribute.CaseSensitive)).Value.Value?.ToString();

        return new OptionalArgumentPropertyGeneratingModel(propertySymbol)
        {
            ShortNames = shortNames,
            LongNames = longNames,
            CaseSensitive = caseSensitive is not null && bool.TryParse(caseSensitive, out var result) ? result : null,
        };
    }
}
