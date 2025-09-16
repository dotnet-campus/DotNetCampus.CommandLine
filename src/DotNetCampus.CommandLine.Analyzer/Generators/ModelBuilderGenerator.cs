using DotNetCampus.CommandLine.Generators.Builders;
using DotNetCampus.CommandLine.Generators.ModelProviding;
using DotNetCampus.CommandLine.Generators.Models;
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
                .WithSummaryComment($"""辅助 <see cref="{model.CommandObjectType.ToUsingString()}"/> 生成命令行选项、子命令或处理函数的创建。""")
                .AddMethodDeclaration(
                    $"public static {model.CommandObjectType.ToUsingString()} CreateInstance(global::DotNetCampus.Cli.CommandLine commandLine)",
                    m => m
                        .AddRawStatements($"return new {model.Namespace}.{model.GetBuilderTypeName()}(commandLine).Build();"))
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

    private string GenerateOptionPropertyCode(PropertyGeneratingModel model) => model.Type.AsCommandValueKind() switch
    {
        CommandValueKind.Boolean => $"private global::DotNetCampus.Cli.Compiler.BooleanArgument {model.PropertyName} = new();",
        CommandValueKind.Number => $"private global::DotNetCampus.Cli.Compiler.NumberArgument {model.PropertyName} = new();",
        CommandValueKind.Enum => $"private {model.Type.GetGeneratedEnumArgumentTypeName()} {model.PropertyName} = new();",
        CommandValueKind.String => $"private global::DotNetCampus.Cli.Compiler.StringArgument {model.PropertyName} = new();",
        CommandValueKind.List => $"private global::DotNetCampus.Cli.Compiler.StringListArgument {model.PropertyName} = new();",
        CommandValueKind.Dictionary => $"private global::DotNetCampus.Cli.Compiler.StringDictionaryArgument {model.PropertyName} = new();",
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
        parser.Parse().ThrowIfError();
        return BuildCore(commandLine);
    }
    """;

    private string GenerateMatchLongOptionCode(CommandObjectGeneratingModel model)
    {
        var optionProperties = model.OptionProperties;
        return optionProperties.Count is 0
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
        {{string.Join("\n", optionProperties.Select(x => GenerateOptionMatchCode(x, x.GetPascalCaseLongNames())))}}
        }
        """;

        static string GenerateOptionMatchCode(OptionalArgumentPropertyGeneratingModel model, IReadOnlyList<string> names)
        {
            var comparison = model.CaseSensitive switch
            {
                true => "global::System.StringComparison.Ordinal",
                false => "global::System.StringComparison.OrdinalIgnoreCase",
                null => "defaultComparison",
            };
            return string.Join("\n", names.Select(name => $$"""
            if (longOption.Equals("{{name}}".AsSpan(), {{comparison}}))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof({{model.PropertyName}}), {{model.PropertyIndex}}, {{model.Type.AsCommandValueKind().ToCommandValueTypeName()}});
            }
        """));
        }
    }

    private string GenerateMatchShortOptionCode(CommandObjectGeneratingModel model)
    {
        var optionProperties = model.OptionProperties;
        return optionProperties.Count is 0
            ? "// 没有短名称选项，无需匹配。"
            : $$"""
        var defaultComparison = defaultCaseSensitive ? global::System.StringComparison.Ordinal : global::System.StringComparison.OrdinalIgnoreCase;

        {{string.Join("\n", optionProperties.Select(x => GenerateOptionMatchCode(x, x.GetShortNames())))}}
        """;

        static string GenerateOptionMatchCode(OptionalArgumentPropertyGeneratingModel model, IReadOnlyList<string> names)
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
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof({{model.PropertyName}}), {{model.PropertyIndex}}, {{model.Type.AsCommandValueKind().ToCommandValueTypeName()}});
            }
            """));
        }
    }

    private string GenerateMatchPositionalArgumentsCode(CommandObjectGeneratingModel model)
    {
        var positionalArgumentProperties = model.PositionalArgumentProperties;
        return positionalArgumentProperties.Count is 0
            ? "// 没有位置参数，无需匹配。"
            : $$"""
        {{string.Join("\n", positionalArgumentProperties.Select(x => GenerateMatchPositionalArgumentCode(x, x.Index, x.Length)))}}
        """;
    }

    private string GenerateMatchPositionalArgumentCode(PositionalArgumentPropertyGeneratingModel model, int index, int length)
    {
        return length switch
        {
            1 => $$"""
        if (argumentIndex is {{index}})
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{{model.PropertyName}}", {{model.PropertyIndex}}, global::DotNetCampus.Cli.PositionalArgumentValueType.Normal);
        }
        """,
            _ when index + length <= 0 => $$"""
        if (argumentIndex >= {{index}})
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{{model.PropertyName}}", {{model.PropertyIndex}}, global::DotNetCampus.Cli.PositionalArgumentValueType.Normal);
        }
        """,
            _ => $$"""
        if (argumentIndex is >= {{index}} and < {{index + length}})
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{{model.PropertyName}}", {{model.PropertyIndex}}, global::DotNetCampus.Cli.PositionalArgumentValueType.Normal);
        }
        """,
        };
    }

    private string GenerateAssignPropertyValueCode(PropertyGeneratingModel model)
    {
        var assign = model.Type.AsCommandValueKind() switch
        {
            CommandValueKind.Boolean => $"{model.PropertyName} = {model.PropertyName}.Assign(value);",
            CommandValueKind.List => $"{model.PropertyName} = {model.PropertyName}.Append(value);",
            CommandValueKind.Dictionary => $"{model.PropertyName} = {model.PropertyName}.Append(key, value);",
            _ => $"{model.PropertyName} = {model.PropertyName}.Assign(value);",
        };
        var propertyIndex = model switch
        {
            OptionalArgumentPropertyGeneratingModel optionPropertyGeneratingModel => optionPropertyGeneratingModel.PropertyIndex,
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
        var initRawArgumentsProperties = model.RawArgumentsProperties.Where(x => x.IsRequiredOrInit).ToList();
        var initOptionProperties = model.OptionProperties.Where(x => x.IsRequiredOrInit).ToList();
        var initPositionalArgumentProperties = model.PositionalArgumentProperties.Where(x => x.IsRequiredOrInit).ToList();
        var setRawArgumentsProperties = model.RawArgumentsProperties.Where(x => !x.IsRequiredOrInit).ToList();
        var setOptionProperties = model.OptionProperties.Where(x => !x.IsRequiredOrInit).ToList();
        var setPositionalArgumentProperties = model.PositionalArgumentProperties.Where(x => !x.IsRequiredOrInit).ToList();
        return $$"""
    var result = new {{model.CommandObjectType.ToUsingString()}}
    {
        // 1. [RawArguments]
    {{(
        initRawArgumentsProperties.Count is 0
            ? "    // There is no [RawArguments] property to be initialized."
            : string.Join("\n", initRawArgumentsProperties.Select(GenerateRawArgumentProperty))
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
            : string.Join("\n", setRawArgumentsProperties.Select(GenerateRawArgumentProperty))
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
        // 对于不同的属性种类，如果命令行中没有赋值，则行为不同。

        // required: 属性要求必须由命令行传入
        // init: 属性要求必须赋值（没有传入则使用该类型默认值）
        // nullable: 属性允许为 null
        // list: 属性是一个集合
        // cli: 实际命令行参数是否传入

        // | required | init | list | nullable | 行为       | 解释                              |
        // | -------- | ---- | ---- | -------- | ---------- | --------------------------------- |
        // | 1        | _    | _    | _        | 抛异常     | 要求必须传入，没有传就抛异常      |
        // | 0        | 1    | 1    | _        | 空集合     | 集合永不为 null，没传就赋值空集合 |
        // | 0        | 1    | 0    | 1        | null       | 可空，没有传就赋值 null           |
        // | 0        | 1    | 0    | 0        | 默认值     | 不可空，没有传就赋值默认值        |
        // | 0        | 0    | _    | _        | 保留初值   | 不要求必须或立即赋值的，保留初值  |

        var toTarget = model.Type.ToCommandValueNonAbstractName();
        var isList = model.Type.AsCommandValueKind() is CommandValueKind.List or CommandValueKind.Dictionary;
        var fallback = (model.IsRequired, model.IsInitOnly, isList, model.IsNullable) switch
        {
            (true, _, _, _) => model switch
            {
                OptionalArgumentPropertyGeneratingModel option =>
                    $" ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required option '{option.GetOrdinalLongNames()[0]}'. Command line: {{commandLine}}\", \"{option.PropertyName}\")",
                PositionalArgumentPropertyGeneratingModel positionalArgument =>
                    $" ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required positional argument at index {positionalArgument.Index}. Command line: {{commandLine}}\", \"{positionalArgument.PropertyName}\")",
                _ => "",
            },
            (_, true, true, _) => "",
            (_, true, false, true) => " ?? null",
            (_, true, false, false) => $" ?? default({model.Type.ToDisplayString()})",
            _ => "/* 非 init 属性，在下面单独赋值 */",
        };
        return $"    {model.PropertyName} = {model.PropertyName}.To{toTarget}(){fallback},";
    }

    private string GenerateSetProperty(PropertyGeneratingModel model, int modelIndex)
    {
        var toTarget = model.Type.ToCommandValueNonAbstractName();
        var variablePrefix = model switch
        {
            RawArgumentPropertyGeneratingModel => "a",
            OptionalArgumentPropertyGeneratingModel => "o",
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

    private string GenerateRawArgumentProperty(RawArgumentPropertyGeneratingModel model)
    {
        var assignment = $"{model.PropertyName} = (commandLine.CommandLineArguments as {model.Type.ToDisplayString()}) ?? [..commandLine.CommandLineArguments]";
        return model.IsRequiredOrInit
            ? $"    {assignment},"
            : $"result.{assignment};";
    }

    private string GenerateEnumDeclarationCode(ITypeSymbol enumType)
    {
        var enumNames = enumType.GetMembers().OfType<IFieldSymbol>().Select(x => x.Name);
        return $$"""
/// <summary>
/// Provides parsing and assignment for the enum type <see cref="{{enumType.ToGlobalDisplayString()}}"/>.
/// </summary>
private readonly record struct {{enumType.GetGeneratedEnumArgumentTypeName()}}
{
    /// <summary>
    /// Indicates whether to ignore exceptions when parsing fails.
    /// </summary>
    public bool IgnoreExceptions { get; init; }

    /// <summary>
    /// Stores the parsed enum value.
    /// </summary>
    private {{enumType.ToUsingString()}}? Value { get; init; }

    /// <summary>
    /// Assigns a value when a command line input is parsed.
    /// </summary>
    /// <param name="value">The parsed string value.</param>
    public {{enumType.GetGeneratedEnumArgumentTypeName()}} Assign(ReadOnlySpan<char> value)
    {
        Span<char> lowerValue = stackalloc char[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            lowerValue[i] = char.ToLowerInvariant(value[i]);
        }
        {{enumType.ToUsingString()}}? newValue = lowerValue switch
        {
    {{string.Join("\n    ", enumNames.Select(x => $"        \"{x.ToLowerInvariant()}\" => {enumType.ToUsingString()}.{x},"))}}
            _ when IgnoreExceptions => null,
            _ => throw new global::DotNetCampus.Cli.Exceptions.CommandLineParseValueException($"Cannot convert '{value.ToString()}' to enum type '{{enumType.ToDisplayString()}}'."),
        };
        return this with { Value = newValue };
    }

    /// <summary>
    /// Converts the parsed value to the enum type.
    /// </summary>
    public {{enumType.ToUsingString()}}? To{{enumType.ToCommandValueNonAbstractName()}}() => Value;
}
""";
    }
}
