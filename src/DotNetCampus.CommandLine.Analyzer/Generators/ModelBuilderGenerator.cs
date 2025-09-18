using System.Text;
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

        // if (model.UseFullStackParser)
        // {
        //     var originalCode = EmbeddedSourceFiles.Enumerate(null)
        //         .First(x => x.FileName == "CommandLineParser.cs")
        //         .Content;
        //     var parserCode = GenerateParserCode(originalCode, model);
        //     context.AddSource($"CommandLine.Models/{model.Namespace}.{model.CommandObjectType.Name}.parser.cs", parserCode);
        // }
    }

    private string GenerateCommandObjectCreatorCode(CommandObjectGeneratingModel model)
    {
        var builder = new SourceTextBuilder(model.Namespace)
            .Using("System")
            .Using("DotNetCampus.Cli.Compiler")
            .AddTypeDeclaration(GenerateBuilderTypeDeclarationLine(model), t => t
                .WithSummaryComment($"""辅助 <see cref="{model.CommandObjectType.ToUsingString()}"/> 生成命令行选项、子命令或处理函数的创建。""")
                .AddMethodDeclaration(
                    $"public static {model.CommandObjectType.ToUsingString()} CreateInstance(global::DotNetCampus.Cli.CommandLine commandLine)",
                    m => m
                        .AddRawStatements($"return new {model.Namespace}.{model.GetBuilderTypeName()}(commandLine).Build();"))
                .AddRawMembers(model.OptionProperties.Select(GenerateArgumentPropertyCode))
                .AddRawMembers(model.EnumeratePositionalArgumentExcludingSameNameOptions().Select(GenerateArgumentPropertyCode))
                .AddRawText(GenerateBuildCode(model))
                .AddMethodDeclaration(
                    "private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchLongOption(ReadOnlySpan<char> longOption, bool defaultCaseSensitive, global::DotNetCampus.Cli.CommandNamingPolicy namingPolicy)",
                    m => GenerateMatchLongOptionCode(m, model))
                .AddMethodDeclaration(
                    "private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchShortOption(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive)",
                    m => GenerateMatchShortOptionCode(m, model))
                .AddMethodDeclaration(
                    "private global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch MatchPositionalArguments(ReadOnlySpan<char> value, int argumentIndex)",
                    m => GenerateMatchPositionalArgumentsCode(m, model))
                .AddMethodDeclaration(
                    "private void AssignPropertyValue(string propertyName, int propertyIndex, ReadOnlySpan<char> key, ReadOnlySpan<char> value)",
                    m => m
                        .Condition(model.OptionProperties.Count > 0 || model.PositionalArgumentProperties.Count > 0, b => b
                            .AddBracketScope("switch (propertyIndex)", l => l
                                .AddRawStatements(model.OptionProperties.Select(GenerateAssignPropertyValueCode))
                                .AddRawStatements(model.EnumeratePositionalArgumentExcludingSameNameOptions().Select(GenerateAssignPropertyValueCode))))
                        .Otherwise(b => b.AddRawStatement("// 没有可赋值的属性。"))
                        .EndCondition())
                .AddMethodDeclaration(
                    $"private {model.CommandObjectType.ToUsingString()} BuildCore(global::DotNetCampus.Cli.CommandLine commandLine)",
                    m => GenerateBuildCoreCode(m, model))
                .AddMethodDeclaration(
                    $"private {model.CommandObjectType.ToUsingString()} BuildDefault()",
                    m => GenerateBuildDefaultCode(m, model))
                .AddRawMembers(model.EnumerateEnumPropertyTypes().Select(GenerateEnumDeclarationCode))
            );
        return builder.ToString();
    }

    private static string GenerateBuilderTypeDeclarationLine(CommandObjectGeneratingModel model)
    {
        var modifier = model.IsPublic ? "public" : "internal";
        var type = model.UseFullStackParser ? "partial struct" : "sealed class";
        return $"{modifier} {type} {model.GetBuilderTypeName()}(global::DotNetCampus.Cli.CommandLine commandLine)";
    }

    private string GenerateArgumentPropertyCode(PropertyGeneratingModel model) =>
        $"private {GetArgumentPropertyTypeName(model)} {model.PropertyName} = new();";

    private string GetArgumentPropertyTypeName(PropertyGeneratingModel model) => model.Type.AsCommandValueKind() switch
    {
        CommandValueKind.Boolean => "global::DotNetCampus.Cli.Compiler.BooleanArgument",
        CommandValueKind.Number => "global::DotNetCampus.Cli.Compiler.NumberArgument",
        CommandValueKind.Enum => model.Type.GetGeneratedEnumArgumentTypeName(),
        CommandValueKind.String => "global::DotNetCampus.Cli.Compiler.StringArgument",
        CommandValueKind.List => "global::DotNetCampus.Cli.Compiler.StringListArgument",
        CommandValueKind.Dictionary => "global::DotNetCampus.Cli.Compiler.StringDictionaryArgument",
        _ => $"// 不支持解析类型为 {model.Type.ToDisplayString()} 的属性 {model.PropertyName}。",
    };

    private static string GenerateBuildCode(CommandObjectGeneratingModel model) => $$"""
    public {{model.CommandObjectType.ToUsingString()}} Build()
    {
        if (commandLine.RawArguments.Count is 0)
        {
            return BuildDefault();
        }
    
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

    private MethodDeclarationSourceTextBuilder GenerateMatchLongOptionCode(MethodDeclarationSourceTextBuilder builder, CommandObjectGeneratingModel model)
    {
        var optionProperties = model.OptionProperties;
        return builder
            .Condition(optionProperties.Count is 0, b => b
                .AddRawStatement("// 没有长名称选项，无需匹配。"))
            .Otherwise(b => b
                .AddRawStatement("// 1. 先快速原字符匹配一遍（能应对规范命令行大小写，并优化 DotNet / GNU 风格的性能）。")
                .AddBracketScope("switch (longOption)", s => s
                    .AddRawStatements(optionProperties.Select(x => GenerateLongOptionCaseCode(x, x.GetOrdinalLongNames()))))
                .AddLineSeparator()
                .AddDefaultStringComparisonIfNeeded(optionProperties)
                .AddLineSeparator()
                .AddRawStatement("// 2. 再按指定大小写指定命名法匹配一遍（能应对不规范命令行大小写）。")
                .AddBracketScope("if (namingPolicy.SupportsOrdinal())", s => s
                    .AddRawStatements(optionProperties.Select(x => GenerateLongOptionEqualsCode(x, x.GetOrdinalLongNames()))))
                .AddLineSeparator()
                .AddRawStatement("// 3. 最后根据其他命名法匹配一遍（能应对所有不规范命令行大小写，并支持所有风格）。")
                .AddBracketScope("if (namingPolicy.SupportsPascalCase())", s => s
                    .AddRawStatements(optionProperties.Select(x => GenerateLongOptionEqualsCode(x, x.GetPascalCaseLongNames()))))
                .AddLineSeparator())
            .EndCondition()
            .AddRawStatement("return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;");

        static string GenerateLongOptionCaseCode(OptionalArgumentPropertyGeneratingModel model, IReadOnlyList<string> names)
        {
            return string.Join("\n", names.Select(name => $"""
            case "{name}":
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof({model.PropertyName}), {model.PropertyIndex}, {model.Type.AsCommandValueKind().ToCommandValueTypeName()});
            """));
        }

        static string GenerateLongOptionEqualsCode(OptionalArgumentPropertyGeneratingModel model, IReadOnlyList<string> names)
        {
            return string.Join("\n", names.Select(name => $$"""
            if (longOption.Equals("{{name}}".AsSpan(), {{model.GetStringComparisonExpression()}}))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof({{model.PropertyName}}), {{model.PropertyIndex}}, {{model.Type.AsCommandValueKind().ToCommandValueTypeName()}});
            }
            """));
        }
    }

    private MethodDeclarationSourceTextBuilder GenerateMatchShortOptionCode(MethodDeclarationSourceTextBuilder builder, CommandObjectGeneratingModel model)
    {
        var optionProperties = model.OptionProperties;
        var hasShortName = optionProperties.SelectMany(x => x.GetShortNames()).Any();
        return builder
            .Condition(!hasShortName, b => b
                .AddRawStatement("// 没有短名称选项，无需匹配。"))
            .Otherwise(b => b
                .AddRawStatement("// 1. 先快速原字符匹配一遍（能应对规范命令行大小写，并优化 DotNet / GNU 风格的性能）。")
                .AddBracketScope("switch (shortOption)", s => s
                    .AddRawStatements(optionProperties.Select(x => GenerateOptionCaseCode(x, x.GetShortNames()))))
                .AddLineSeparator()
                .AddDefaultStringComparisonIfNeeded(optionProperties)
                .AddLineSeparator()
                .AddRawStatement("// 2. 再按指定大小写指定命名法匹配一遍（能应对不规范命令行大小写）。")
                .AddRawStatements(optionProperties.Select(x => GenerateOptionEqualsCode(x, x.GetShortNames())))
                .AddLineSeparator())
            .EndCondition()
            .AddRawStatement("return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;");

        static string GenerateOptionCaseCode(OptionalArgumentPropertyGeneratingModel model, IReadOnlyList<string> names)
        {
            if (names.Count == 0)
            {
                return $"""
            // 属性 {model.PropertyName} 没有短名称，无需匹配。
            """;
            }
            return string.Join("\n", names.Select(name => $"""
            case "{name}":
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof({model.PropertyName}), {model.PropertyIndex}, {model.Type.AsCommandValueKind().ToCommandValueTypeName()});
            """));
        }

        static string GenerateOptionEqualsCode(OptionalArgumentPropertyGeneratingModel model, IReadOnlyList<string> names)
        {
            if (names.Count == 0)
            {
                return $"""
            // 属性 {model.PropertyName} 没有短名称，无需匹配。
            """;
            }
            return string.Join("\n", names.Select(name => $$"""
            if (shortOption.Equals("{{name}}".AsSpan(), {{model.GetStringComparisonExpression()}}))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof({{model.PropertyName}}), {{model.PropertyIndex}}, {{model.Type.AsCommandValueKind().ToCommandValueTypeName()}});
            }
            """));
        }
    }

    private MethodDeclarationSourceTextBuilder GenerateMatchPositionalArgumentsCode(MethodDeclarationSourceTextBuilder builder,
        CommandObjectGeneratingModel model)
    {
        var positionalArgumentProperties = model.PositionalArgumentProperties;
        return builder
            .Condition(positionalArgumentProperties.Count is 0, b => b
                .AddRawStatement("// 没有位置参数，无需匹配。"))
            .Otherwise(b => b
                .AddRawStatements(positionalArgumentProperties.Select(x => GenerateMatchPositionalArgumentCode(x, x.Index, x.Length)))
                .AddLineSeparator())
            .EndCondition()
            .AddRawStatement("return global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch.NotMatch;");
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

    private MethodDeclarationSourceTextBuilder GenerateBuildCoreCode(MethodDeclarationSourceTextBuilder builder, CommandObjectGeneratingModel model)
    {
        var initRawArgumentsProperties = model.RawArgumentsProperties.Where(x => x.IsRequiredOrInit).ToList();
        var initOptionProperties = model.OptionProperties.Where(x => x.IsRequiredOrInit).ToList();
        var initPositionalArgumentProperties = model.PositionalArgumentProperties.Where(x => x.IsRequiredOrInit).ToList();
        var setRawArgumentsProperties = model.RawArgumentsProperties.Where(x => !x.IsRequiredOrInit).ToList();
        var setOptionProperties = model.OptionProperties.Where(x => !x.IsRequiredOrInit).ToList();
        var setPositionalArgumentProperties = model.PositionalArgumentProperties.Where(x => !x.IsRequiredOrInit).ToList();

        return builder
            .AddBracketScope($"var result = new {model.CommandObjectType.ToUsingString()}", "{", "};", c => c

                // 1. [RawArguments]
                .Condition(initRawArgumentsProperties.Count is 0, b => b
                    .AddRawStatement("// 1. There is no [RawArguments] property to be initialized."))
                .Otherwise(b => b
                    .AddRawStatement("// 1. [RawArguments]")
                    .AddRawStatements(initRawArgumentsProperties.Select(x => GenerateRawArgumentProperty(x, false))))
                .EndCondition()

                // 2. [Option]
                .AddLineSeparator()
                .Condition(initOptionProperties.Count is 0, b => b
                    .AddRawStatement("// 2. There is no [Option] property to be initialized."))
                .Otherwise(b => b
                    .AddRawStatement("// 2. [Option]")
                    .AddRawStatements(initOptionProperties.Select(x => GenerateInitProperty(x, false))))
                .EndCondition()

                // 3. [Value]
                .AddLineSeparator()
                .Condition(initPositionalArgumentProperties.Count is 0, b => b
                    .AddRawStatement("// 3. There is no [Value] property to be initialized."))
                .Otherwise(b => b
                    .AddRawStatement("// 3. [Value]")
                    .AddRawStatements(initPositionalArgumentProperties.Select(x => GenerateInitProperty(x, false))))
                .EndCondition())

            // 1. [RawArguments]
            .AddLineSeparator()
            .Condition(setRawArgumentsProperties.Count is 0, b => b
                .AddRawStatement("// 1. There is no [RawArguments] property to be assigned."))
            .Otherwise(b => b
                .AddRawStatement("// 1. [RawArguments]")
                .AddRawStatements(setRawArgumentsProperties.Select(x => GenerateRawArgumentProperty(x, false))))
            .EndCondition()

            // 2. [Option]
            .AddLineSeparator()
            .Condition(setOptionProperties.Count is 0, b => b
                .AddRawStatement("// 2. There is no [Option] property to be assigned."))
            .Otherwise(b => b
                .AddRawStatement("// 2. [Option]")
                .AddRawStatements(setOptionProperties.Select(GenerateSetProperty)))
            .EndCondition()

            // 3. [Value]
            .AddLineSeparator()
            .Condition(setPositionalArgumentProperties.Count is 0, b => b
                .AddRawStatement("// 3. There is no [Value] property to be assigned."))
            .Otherwise(b => b
                .AddRawStatement("// 3. [Value]")
                .AddRawStatements(setPositionalArgumentProperties.Select(GenerateSetProperty)))
            .EndCondition()
            .AddLineSeparator()

            // 返回。
            .AddRawStatement("return result;");
    }

    private MethodDeclarationSourceTextBuilder GenerateBuildDefaultCode(MethodDeclarationSourceTextBuilder builder, CommandObjectGeneratingModel model)
    {
        var initRawArgumentsProperties = model.RawArgumentsProperties.Where(x => x.IsRequiredOrInit).ToList();
        var initOptionProperties = model.OptionProperties.Where(x => x.IsRequiredOrInit).ToList();
        var initPositionalArgumentProperties = model.PositionalArgumentProperties.Where(x => x.IsRequiredOrInit).ToList();

        if (initOptionProperties.Any(x => x.IsRequired)
            || initPositionalArgumentProperties.Any(x => x.IsRequired))
        {
            // 存在必须赋值的属性，不能生成默认值创建代码。
            builder.AddRawStatement("""
throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($"The command line arguments doesn't contain any required option or positional argument. Command line: {commandLine}", null!);
""");
            return builder;
        }

        return builder
            .AddBracketScope($"var result = new {model.CommandObjectType.ToUsingString()}", "{", "};", c => c

                // 1. [RawArguments]
                .Condition(initRawArgumentsProperties.Count is 0, b => b.Ignore())
                .Otherwise(b => b
                    .AddRawStatements(initRawArgumentsProperties.Select(x => GenerateRawArgumentProperty(x, true))))
                .EndCondition()

                // 2. [Option]
                .AddLineSeparator()
                .Condition(initOptionProperties.Count is 0, b => b.Ignore())
                .Otherwise(b => b
                    .AddRawStatements(initOptionProperties.Select(x => GenerateInitProperty(x, true))))
                .EndCondition()

                // 3. [Value]
                .AddLineSeparator()
                .Condition(initPositionalArgumentProperties.Count is 0, b => b.Ignore())
                .Otherwise(b => b
                    .AddRawStatements(initPositionalArgumentProperties.Select(x => GenerateInitProperty(x, true))))
                .EndCondition())

            // 返回。
            .AddRawStatement("return result;");
    }

    private string GenerateInitProperty(PropertyGeneratingModel model, bool forDefault)
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
                    $"throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required option '{option.GetOrdinalLongNames()[0]}'. Command line: {{commandLine}}\", \"{option.PropertyName}\")",
                PositionalArgumentPropertyGeneratingModel positionalArgument =>
                    $"throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required positional argument at index {positionalArgument.Index}. Command line: {{commandLine}}\", \"{positionalArgument.PropertyName}\")",
                _ => "",
            },
            (_, true, true, _) => "",
            (_, true, false, true) => "null",
            (_, true, false, false) => $"default({model.Type.ToDisplayString()})!",
            _ => "/* 非 init 属性，在下面单独赋值 */",
        };

        if (!forDefault)
        {
            // 正常传入了命令行参数时的通用赋值。
            return $"{model.PropertyName} = {model.PropertyName}.To{toTarget}(){(fallback is "" ? "" : $" ?? {fallback}")},";
        }

        if (fallback is not "")
        {
            // 未传命令行参数时，给非集合类型赋值。
            return $"{model.PropertyName} = {fallback},";
        }

        // 未传命令行参数时，给集合类型赋值为空集合。
        var supportCollectionExpression = model.Type.SupportCollectionExpression(true);
        var supportCollectionExpressionLegacy = model.Type.SupportCollectionExpression(false);
        return (supportCollectionExpression, supportCollectionExpressionLegacy) switch
        {
            (true, true) => $"    {model.PropertyName} = [],",
            (false, false) => $"    {model.PropertyName} = new {GetArgumentPropertyTypeName(model)}().To{toTarget}(),",
            _ => $"""
                #if NET8_0_OR_GREATER
                {model.PropertyName} = [],
                #else
                {model.PropertyName} = new {GetArgumentPropertyTypeName(model)}().To{toTarget}(),
                #endif
                """,
        };
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

    private string GenerateRawArgumentProperty(RawArgumentPropertyGeneratingModel model, bool forDefault)
    {
        if (forDefault)
        {
            return $"{model.PropertyName} = [],";
        }

        var assignment = $"{model.PropertyName} = (commandLine.CommandLineArguments as {model.Type.ToDisplayString()}) ?? [..commandLine.CommandLineArguments]";
        return model.IsRequiredOrInit
            ? $"{assignment},"
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

    private string GenerateParserCode(string originalCode, CommandObjectGeneratingModel model) => new StringBuilder()
        .AppendLine("#nullable enable")
        .AppendLine("using DotNetCampus.Cli;")
        .Append(originalCode)
        .Replace("namespace DotNetCampus.Cli.Utils.Parsers;", $"namespace {model.Namespace};")
        .Replace("public readonly ref struct CommandLineParser", $"partial struct {model.GetBuilderTypeName()}")
        .ToString();
}

file static class Extensions
{
    public static string GetStringComparisonExpression(this OptionalArgumentPropertyGeneratingModel model)
    {
        return model.CaseSensitive switch
        {
            true => "global::System.StringComparison.Ordinal",
            false => "global::System.StringComparison.OrdinalIgnoreCase",
            null => "defaultComparison",
        };
    }

    public static TBuilder AddDefaultStringComparisonIfNeeded<TBuilder>(this TBuilder builder,
        IReadOnlyList<OptionalArgumentPropertyGeneratingModel> optionProperties)
        where TBuilder : IAllowStatements
    {
        var needStringComparison = optionProperties.Any(x => x.CaseSensitive is null);
        if (needStringComparison)
        {
            builder.AddRawStatement(
                """
                var defaultComparison = defaultCaseSensitive
                    ? global::System.StringComparison.Ordinal
                    : global::System.StringComparison.OrdinalIgnoreCase;
                """);
        }
        return builder;
    }
}
