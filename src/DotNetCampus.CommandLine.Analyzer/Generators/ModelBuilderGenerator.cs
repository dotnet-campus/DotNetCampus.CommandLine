using DotNetCampus.CommandLine.Generators.Builders;
using DotNetCampus.CommandLine.Generators.ModelProviding;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators;

/// <summary>
/// 为命令行参数的模型对象生成创建器代码。
/// </summary>
[Generator(LanguageNames.CSharp)]
public class ModelBuilderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandOptionsProvider = context.SelectCommandObjects();
        context.RegisterSourceOutput(commandOptionsProvider, Execute);
    }

    private void Execute(SourceProductionContext context, CommandObjectGeneratingModel model)
    {
        var code = GenerateCommandObjectCreatorCode(model);
        context.AddSource($"CommandLine.Models/{model.Namespace}.{model.CommandObjectType.Name}.cs", code);
    }

    private string GenerateCommandObjectCreatorCode(CommandObjectGeneratingModel model)
    {
        var modifier = model.IsPublic ? "public" : "internal";
        var builder = new SourceTextBuilder(model.Namespace)
            .Using("System")
            .Using("DotNetCampus.Cli.Compiler")
            .AddTypeDeclaration($"{modifier} sealed class {model.GetBuilderTypeName()}(global::DotNetCampus.Cli.CommandLine commandLine)", t => t
                .WithSummaryComment($"""辅助 <see cref="{model.CommandObjectType.ToGlobalDisplayString()}"/> 生成命令行选项、子命令或处理函数的创建。""")
                .AddRawMembers(model.OptionProperties.Select(GenerateOptionPropertyCode))
                .AddRawMembers(model.EnumeratePositionalArgumentPropertiesExcludingSameNameOptions().Select(GenerateOptionPropertyCode))
                .AddRawText(GenerateBuildCode(model))
                .AddMethodDeclaration(
                    "private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchLongOption(ReadOnlySpan<char> longOption, bool defaultCaseSensitive, CommandNamingPolicy namingPolicy)",
                    m => m
                        .AddRawStatements(GenerateMatchLongOptionCode(model))
                        .AddRawStatements("return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;"))
                .AddMethodDeclaration(
                    "private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchShortOption(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive)",
                    m => m
                        .AddRawStatements(GenerateMatchShortOptionCode(model))
                        .AddRawStatements("return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;"))
                .AddMethodDeclaration(
                    "private global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch MatchPositionalArguments(ReadOnlySpan<char> value, int argumentIndex)",
                    m => m
                        .AddRawStatements(GenerateMatchPositionalArgumentsCode(model))
                        .AddRawStatements("return global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch.NotMatch;"))
                .AddMethodDeclaration(
                    "private void AssignPropertyValue(string propertyName, int propertyIndex, ReadOnlySpan<char> key, ReadOnlySpan<char> value)",
                    m => m
                        .BeginBracketScope("switch (propertyIndex)", l => l
                            .AddRawStatements(model.OptionProperties.Select(GenerateAssignPropertyValueCode))
                            .AddRawStatements(model.EnumeratePositionalArgumentPropertiesExcludingSameNameOptions().Select(GenerateAssignPropertyValueCode))))
                .AddMethodDeclaration(
                    $"private {model.CommandObjectType.ToUsingString()} BuildCore(global::DotNetCampus.Cli.CommandLine commandLine)",
                    m => m
                        .AddRawStatements(GenerateBuildCoreCode(model)))
                .AddRawMembers(model.EnumerateEnumPropertyTypes().Select(GenerateEnumDeclarationCode))
            );
        return builder.ToString();
    }

    private string GenerateOptionPropertyCode(PropertyGeneratingModel model) => model.Type.AsCommandPropertyType() switch
    {
        CommandPropertyType.Boolean => $"private global::DotNetCampus.Cli.Compiler.BooleanArgument {model.PropertyName} {{ get; }} = new();",
        CommandPropertyType.Number => $"private global::DotNetCampus.Cli.Compiler.NumberArgument {model.PropertyName} {{ get; }} = new();",
        CommandPropertyType.Enum => $"private {model.Type.GetGeneratedEnumArgumentTypeName()} {model.PropertyName} {{ get; }} = new();",
        CommandPropertyType.String => $"private global::DotNetCampus.Cli.Compiler.StringArgument {model.PropertyName} {{ get; }} = new();",
        CommandPropertyType.List => $"private global::DotNetCampus.Cli.Compiler.StringListArgument {model.PropertyName} {{ get; }} = new();",
        CommandPropertyType.Dictionary => $"private global::DotNetCampus.Cli.Compiler.StringDictionaryArgument {model.PropertyName} {{ get; }} = new();",
        _ => $"// 不支持解析类型为 {model.Type.ToDisplayString()} 的属性 {model.PropertyName}。",
    };

    private static string GenerateBuildCode(CommandObjectGeneratingModel model) => $$"""
    public {{model.CommandObjectType.ToUsingString()}} Build()
    {
        var parser = new global::DotNetCampus.Cli.Utils.Parsers.CommandLineParser(commandLine, "{{model.CommandObjectType.Name}}", {{model.GetCommandLevel()}})
        {
            MatchLongOption = MatchLongOption,
            MatchShortOption = MatchShortOption,
            MatchPositionalArguments = MatchPositionalArguments,
            AssignPropertyValue = AssignPropertyValue,
        };
        parser.Parse();
        return BuildCore(commandLine);
    }
    """;

    private string GenerateMatchLongOptionCode(CommandObjectGeneratingModel model)
    {
        var optionProperties = model.OptionProperties;
        return optionProperties.Length is 0
            ? "// 没有长名称选项，无需匹配。"
            : $$"""
        var defaultComparison = defaultCaseSensitive ? global::System.StringComparison.Ordinal : global::System.StringComparison.OrdinalIgnoreCase;

        // 先原样匹配一遍。
        if (namingPolicy.SupportsOrdinal())
        {
        {{string.Join("\n", optionProperties.Select(x => GenerateOptionMatchCode(x, x.GetOrdinalLongNames())))}}
        }

        // 再根据命名法匹配一遍（只匹配与上述名称不同的名称）。
        if (namingPolicy.SupportsPascalCase())
        {
        {{string.Join("\n", optionProperties.Select(x => GenerateOptionMatchCode(x, x.GetPascalCaseLongNames().Except(x.GetOrdinalLongNames()).ToList())))}}
        }
        """;

        static string GenerateOptionMatchCode(OptionPropertyGeneratingModel model, IReadOnlyList<string> names)
        {
            if (names.Count == 0)
            {
                return $"""
            // 属性 {model.PropertyName} 在此命名法下的所有名称均已在前面匹配过，无需重复匹配。
        """;
            }
            var comparison = model.CaseSensitive switch
            {
                true => "global::System.StringComparison.Ordinal",
                false => "global::System.StringComparison.OrdinalIgnoreCase",
                null => "defaultComparison",
            };
            return string.Join("\n", names.Select(name => $$"""
            if (longOption.Equals("{{name}}".AsSpan(), {{comparison}}))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch("{{model.PropertyName}}", {{model.PropertyIndex}}, {{model.Type.AsCommandPropertyType().ToOptionValueTypeName()}});
            }
        """));
        }
    }

    private string GenerateMatchShortOptionCode(CommandObjectGeneratingModel model)
    {
        var optionProperties = model.OptionProperties;
        return optionProperties.Length is 0
            ? "// 没有短名称选项，无需匹配。"
            : $$"""
        var defaultComparison = defaultCaseSensitive ? global::System.StringComparison.Ordinal : global::System.StringComparison.OrdinalIgnoreCase;

        {{string.Join("\n", optionProperties.Select(x => GenerateOptionMatchCode(x, x.GetShortNames())))}}
        """;

        static string GenerateOptionMatchCode(OptionPropertyGeneratingModel model, IReadOnlyList<string> names)
        {
            if (names.Count == 0)
            {
                return $"""
            // 属性 {model.PropertyName} 没有短名称，无需匹配。
            """;
            }
            var comparison = model.CaseSensitive switch
            {
                true => "global::System.StringComparison.Ordinal",
                false => "global::System.StringComparison.OrdinalIgnoreCase",
                null => "defaultComparison",
            };
            return string.Join("\n", names.Select(name => $$"""
            if (shortOption.Equals("{{name}}".AsSpan(), {{comparison}}))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch("{{model.PropertyName}}", {{model.PropertyIndex}}, {{model.Type.AsCommandPropertyType().ToOptionValueTypeName()}});
            }
            """));
        }
    }

    private string GenerateMatchPositionalArgumentsCode(CommandObjectGeneratingModel model)
    {
        var positionalArgumentProperties = model.PositionalArgumentProperties;
        return positionalArgumentProperties.Length is 0
            ? "// 没有位置参数，无需匹配。"
            : $$"""
        {{string.Join("\n", positionalArgumentProperties.Select(x => GenerateMatchPositionalArgumentCode(x, x.Index, x.Length)))}}
        """;
    }

    private string GenerateMatchPositionalArgumentCode(PositionalArgumentPropertyGeneratingModel positionalArgumentPropertyGeneratingModel, int index,
        int length)
    {
        return length == 1
            ? $$"""
        if (argumentIndex is {{index}})
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{{positionalArgumentPropertyGeneratingModel.PropertyName}}", {{positionalArgumentPropertyGeneratingModel.PropertyIndex}}, global::DotNetCampus.Cli.PositionalArgumentValueType.Normal);
        }
    """
            : $$"""
        if (argumentIndex is >= {{index}} and < {{index + length}})
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{{positionalArgumentPropertyGeneratingModel.PropertyName}}", {{positionalArgumentPropertyGeneratingModel.PropertyIndex}}, global::DotNetCampus.Cli.PositionalArgumentValueType.Normal);
        }
    """;
    }

    private string GenerateAssignPropertyValueCode(PropertyGeneratingModel model)
    {
        var assign = model.Type.AsCommandPropertyType() switch
        {
            CommandPropertyType.Boolean => $"{model.PropertyName}.Assign(value[0] == '1');",
            CommandPropertyType.List => $"{model.PropertyName}.Append(value);",
            CommandPropertyType.Dictionary => $"{model.PropertyName}.Append(key, value);",
            _ => $"{model.PropertyName}.Assign(value);",
        };
        var propertyIndex = model switch
        {
            OptionPropertyGeneratingModel optionPropertyGeneratingModel => optionPropertyGeneratingModel.PropertyIndex,
            PositionalArgumentPropertyGeneratingModel positionalArgumentPropertyGeneratingModel => positionalArgumentPropertyGeneratingModel.PropertyIndex,
            _ => -1,
        };
        return $"""
            case {propertyIndex}:
                {assign}
                break;
            """;
    }

    private string GenerateBuildCoreCode(CommandObjectGeneratingModel model)
    {
        // 对于不同的属性类型，生成不同的代码。
        // init: 属性要求必须立即赋值
        // nullable: 属性允许为 null
        // cli: 实际命令行参数是否传入

        // | init | nullable | cli | 行为       |
        // | ---- | -------- | --- | ---------- |
        // | 1    | 1        | 0   | 默认值     |
        // | 1    | 0        | 0   | 抛异常     |
        // | 0    | 1        | 0   | 保留初值   |
        // | 0    | 0        | 0   | 保留初值   |
        // | _    | _        | 1   | 赋值       |

        var initRawArgumentsProperties = model.RawArgumentsProperties.Where(x => x.IsRequired || x.IsInitOnly).ToList();
        var initOptionProperties = model.OptionProperties.Where(x => x.IsRequired || x.IsInitOnly).ToList();
        var initPositionalArgumentProperties = model.PositionalArgumentProperties.Where(x => x.IsRequired || x.IsInitOnly).ToList();
        var setRawArgumentsProperties = model.RawArgumentsProperties.Where(x => !x.IsRequired && !x.IsInitOnly).ToList();
        var setOptionProperties = model.OptionProperties.Where(x => !x.IsRequired && !x.IsInitOnly).ToList();
        var setPositionalArgumentProperties = model.PositionalArgumentProperties.Where(x => !x.IsRequired && !x.IsInitOnly).ToList();
        return $$"""
    var result = new {{model.CommandObjectType.ToUsingString()}}
    {
        // 1. [RawArguments]
    {{(
        initRawArgumentsProperties.Count is 0
            ? "    // There is no [RawArguments] property to be initialized."
            : string.Join("\n", initRawArgumentsProperties.Select(GenerateInitRawArgumentProperty))
    )}}

        // 2. [Option]
    {{(
        initOptionProperties.Count is 0
            ? "    // There is no [Option] property to be initialized."
            : string.Join("\n", initOptionProperties.Select(GenerateInitProperty))
    )}}

        // 3. [Value]
    {{(
        initPositionalArgumentProperties.Count is 0
            ? "    // There is no [Value] property to be initialized."
            : string.Join("\n", initPositionalArgumentProperties.Select(GenerateInitProperty))
    )}}
    };

    // 1. [RawArguments]
    {{(
        setRawArgumentsProperties.Count is 0
            ? "// There is no [RawArguments] property to be assigned."
            : string.Join("\n", setRawArgumentsProperties.Select(GenerateSetRawArgumentProperty))
    )}}

    // 2. [Option]
    {{(
        setOptionProperties.Count is 0
            ? "// There is no [RawArguments] property to be assigned."
            : string.Join("\n", setOptionProperties.Select(GenerateSetProperty))
    )}}

    // 3. [Value]
    {{(
        setPositionalArgumentProperties.Count is 0
            ? "// There is no [RawArguments] property to be assigned."
            : string.Join("\n", setPositionalArgumentProperties.Select(GenerateSetProperty))
    )}}

    return result;
    """;
    }

    private string GenerateInitProperty(PropertyGeneratingModel model)
    {
        var toTarget = model.Type.ToCommandTargetMethodName();
        var fallback = model.IsNullable
            ? " ?? null"
            : model switch
            {
                OptionPropertyGeneratingModel option =>
                    $" ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required option '{option.GetOrdinalLongNames()[0]}'. Command line: {{commandLine}}\", \"{option.PropertyName}\")",
                PositionalArgumentPropertyGeneratingModel positionalArgument =>
                    $" ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required positional argument at index {positionalArgument.Index}. Command line: {{commandLine}}\", \"{positionalArgument.PropertyName}\")",
                _ => "",
            };
        return $"    {model.PropertyName} = {model.PropertyName}.To{toTarget}(){fallback},";
    }

    private string GenerateSetProperty(PropertyGeneratingModel model, int modelIndex)
    {
        var toTarget = model.Type.ToCommandTargetMethodName();
        var variablePrefix = model switch
        {
            RawArgumentsPropertyGeneratingModel => "a",
            OptionPropertyGeneratingModel => "o",
            PositionalArgumentPropertyGeneratingModel => "v",
            _ => "",
        };
        return $$"""
        if ({{model.PropertyName}}.To{{toTarget}}() is { } {{variablePrefix}}{{modelIndex}})
        {
            result.{{model.PropertyName}} = {{variablePrefix}}{{modelIndex}};
        }
        """;
    }

    private string GenerateInitRawArgumentProperty(RawArgumentsPropertyGeneratingModel model)
    {
        return $"    {model.PropertyName} = commandLine.CommandLineArguments,";
    }

    private string GenerateSetRawArgumentProperty(RawArgumentsPropertyGeneratingModel model)
    {
        return $"result.{model.PropertyName} = commandLine.CommandLineArguments;";
    }

    private string GenerateEnumDeclarationCode(ITypeSymbol enumType)
    {
        var enumNames = enumType.GetMembers().OfType<IFieldSymbol>().Select(x => x.Name);
        return $$"""
/// <summary>
/// Provides parsing and assignment for the enum type <see cref="{{enumType.ToGlobalDisplayString()}}"/>.
/// </summary>
private struct {{enumType.GetGeneratedEnumArgumentTypeName()}}
{
    /// <summary>
    /// Indicates whether to ignore exceptions when parsing fails.
    /// </summary>
    public bool IgnoreExceptions { get; init; }

    /// <summary>
    /// Stores the parsed enum value.
    /// </summary>
    private {{enumType.ToUsingString()}}? _value;

    /// <summary>
    /// Assigns a value when a command line input is parsed.
    /// </summary>
    /// <param name="value">The parsed string value.</param>
    public void Assign(ReadOnlySpan<char> value)
    {
        Span<char> lowerValue = stackalloc char[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            lowerValue[i] = char.ToLowerInvariant(value[i]);
        }
        _value = lowerValue switch
        {
    {{string.Join("\n    ", enumNames.Select(x => $"        \"{x.ToLowerInvariant()}\" => {enumType.ToUsingString()}.{x},"))}}
            _ when IgnoreExceptions => null,
            _ => throw new global::System.ArgumentOutOfRangeException(nameof(value), value.ToString(), $"Cannot convert '{value.ToString()}' to enum type '{{enumType.ToDisplayString()}}'."),
        };
    }

    /// <summary>
    /// Converts the parsed value to the enum type.
    /// </summary>
    public {{enumType.ToUsingString()}}? To{{enumType.ToCommandTargetMethodName()}}() => _value;
}
""";
    }
}

file static class Extensions
{
    public static string GetGeneratedEnumArgumentTypeName(this ITypeSymbol type)
    {
        return $"__GeneratedEnumArgument__{type.ToDisplayString().Replace('.', '_')}__";
    }

    public static string ToOptionValueTypeName(this CommandPropertyType type) => type switch
    {
        CommandPropertyType.Boolean => "global::DotNetCampus.Cli.OptionValueType.Boolean",
        CommandPropertyType.List => "global::DotNetCampus.Cli.OptionValueType.List",
        CommandPropertyType.Dictionary => "global::DotNetCampus.Cli.OptionValueType.Dictionary",
        _ => "global::DotNetCampus.Cli.OptionValueType.Normal",
    };
}
