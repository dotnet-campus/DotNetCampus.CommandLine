using System.Text;
using DotNetCampus.CommandLine.CodeAnalysis;
using DotNetCampus.CommandLine.Generators.Builders;
using DotNetCampus.CommandLine.Generators.ModelProviding;
using DotNetCampus.CommandLine.Generators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        if (ReportDiagnostics(context, model))
        {
            return;
        }

        var code = GenerateCommandObjectCreatorCode(model);
        context.AddSource($"CommandLine.Models/{model.CommandObjectType.ToDisplayString()}.cs", code);

        // if (model.UseFullStackParser)
        // {
        //     var originalCode = EmbeddedSourceFiles.Enumerate(null)
        //         .First(x => x.FileName == "CommandLineParser.cs")
        //         .Content;
        //     var parserCode = GenerateParserCode(originalCode, model);
        //     context.AddSource($"CommandLine.Models/{model.Namespace}.{model.CommandObjectType.Name}.parser.cs", parserCode);
        // }
    }

    private static bool ReportDiagnostics(SourceProductionContext context, CommandObjectGeneratingModel model)
    {
        var fileName = model.CommandObjectType.ToDisplayString();
        if (fileName.Contains('<'))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.DCL301_GenericCommandObjectTypeNotSupported,
                model.CommandObjectType.DeclaringSyntaxReferences.FirstOrDefault()?
                    .GetSyntax().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault()?
                    .Identifier.GetLocation(),
                fileName));
            return true;
        }

        if (model.OptionProperties.FindFirstDuplicateName() is ({ } name, { } location))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.DCL204_DuplicateOptionNames,
                location,
                name));
            return true;
        }

        return false;
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
        _ => "global::DotNetCampus.Cli.Compiler.ErrorArgument",
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
        parser.Parse().WithFallback(commandLine);
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
                .AddRawStatement("// 1. 先匹配 kebab-case 命名法（原样字符串）")
                .AddBracketScope("if (namingPolicy.SupportsOrdinal())", s => s
                    .AddRawStatement("// 1.1 先快速原字符匹配一遍（能应对规范命令行大小写，并优化 DotNet / GNU 风格的性能）。")
                    .AddBracketScope("switch (longOption)", c => c
                        .AddRawStatements(optionProperties.Select(x => GenerateLongOptionCaseCode(x, x.GetOrdinalLongNames()))))
                    .AddLineSeparator()
                    .AddRawStatement("// 1.2 再按指定大小写匹配一遍（能应对不规范命令行大小写）。")
                    .AddDefaultStringComparisonIfNeeded(optionProperties)
                    .AddRawStatements(optionProperties.Select(x => GenerateLongOptionEqualsCode(x, x.GetOrdinalLongNames()))))
                .AddLineSeparator()
                .AddRawStatement("// 2. 再匹配其他命名法（能应对所有不规范命令行大小写，并支持所有风格）。")
                .AddBracketScope("if (namingPolicy.SupportsPascalCase())", s => s
                    .AddDefaultStringComparisonIfNeeded(optionProperties)
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
        var matchAllProperty = positionalArgumentProperties.FirstOrDefault(x => x.Index is 0 && x.Length is int.MaxValue);
        return builder
            .Condition(positionalArgumentProperties.Count is 0, b => b
                .AddRawStatement("// 没有位置参数，无需匹配。")
                .AddRawStatement("return global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch.NotMatch;"))
            .Condition(matchAllProperty is not null, b => b
                .AddRawStatement($"// 属性 {matchAllProperty!.PropertyName} 覆盖了所有位置参数，直接匹配。")
                .AddRawStatement($"""
return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{matchAllProperty.PropertyName}", {matchAllProperty.PropertyIndex}, global::DotNetCampus.Cli.Compiler.PositionalArgumentValueType.Normal);
"""))
            .Otherwise(b => b
                .AddRawStatements(positionalArgumentProperties.Select(x => GenerateMatchPositionalArgumentCode(x, x.Index, x.Length)))
                .AddLineSeparator()
                .AddRawStatement("return global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch.NotMatch;"))
            .EndCondition();
    }

    private string GenerateMatchPositionalArgumentCode(PositionalArgumentPropertyGeneratingModel model, int index, int length)
    {
        return length switch
        {
            <= 0 => "// 属性 {model.PropertyName} 的范围不包含任何位置参数，无法匹配。",
            1 => $$"""
        if (argumentIndex is {{index}})
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{{model.PropertyName}}", {{model.PropertyIndex}}, global::DotNetCampus.Cli.Compiler.PositionalArgumentValueType.Normal);
        }
        """,
            _ when (index + length) is <= 0 or int.MaxValue => $$"""
        if (argumentIndex >= {{index}})
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{{model.PropertyName}}", {{model.PropertyIndex}}, global::DotNetCampus.Cli.Compiler.PositionalArgumentValueType.Normal);
        }
        """,
            _ => $$"""
        if (argumentIndex is >= {{index}} and < {{index + length}})
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("{{model.PropertyName}}", {{model.PropertyIndex}}, global::DotNetCampus.Cli.Compiler.PositionalArgumentValueType.Normal);
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

        // | required | init | nullable | list | 行为        | 解释                              |
        // | -------- | ---- | -------- | ---- | ----------- | --------------------------------- |
        // | 1        | _    | _        | _    | 抛异常      | 要求必须传入，没有传就抛异常      |
        // | 0        | 1    | 1        | _    | null        | 可空，没有传就赋值 null           |
        // | 0        | 1    | 0        | 1    | 空集合      | 集合永不为 null，没传就赋值空集合 |
        // | 0        | 1    | 0        | 0    | 默认值/空值 | 不可空，没有传就赋值默认值        |
        // | 0        | 0    | _        | _    | 保留初值    | 不要求必须或立即赋值的，保留初值  |
        //
        // [默认值/空值] 如果是值类型，则会赋值其默认值；如果是引用类型，目前只有一种情况，就是字符串，会赋值为空字符串 `""`。

        var toTarget = model.Type.GetGeneratedNotAbstractTypeName();
        var kind = model.Type.AsCommandValueKind();
        var isString = kind is CommandValueKind.String;
        var isList = kind is CommandValueKind.List or CommandValueKind.Dictionary;
        var supportCollectionExpression = model.Type.SupportCollectionExpression(false);
        var fallback = (model.IsRequired, model.IsInitOnly, model.IsNullable, isList) switch
        {
            (true, _, _, _) => model switch
            {
                OptionalArgumentPropertyGeneratingModel option =>
                    $"throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required option '{option.GetOrdinalLongNames()[0]}'. Command line: {{commandLine}}\", \"{option.PropertyName}\")",
                PositionalArgumentPropertyGeneratingModel positionalArgument =>
                    $"throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required positional argument at index {positionalArgument.Index}. Command line: {{commandLine}}\", \"{positionalArgument.PropertyName}\")",
                _ => "",
            },
            (_, true, true, _) => "null",
            (_, true, false, true) => supportCollectionExpression
                ? "[]"
                : $"new {GetArgumentPropertyTypeName(model)}().To{toTarget}(true)",
            (_, true, false, false) => isString
                ? "\"\""
                : $"default({model.Type.ToDisplayString()})!",
            _ => "/* 非 init 属性，在下面单独赋值 */",
        };

        return !forDefault
            // 正常传入了命令行参数时的通用赋值。
            ? $"{model.PropertyName} = {model.PropertyName}.To{toTarget}(){(fallback is "" ? "" : $" ?? {fallback}")},"
            // 未传命令行参数时，直接赋回退值。
            : $"{model.PropertyName} = {fallback},";
    }

    private string GenerateSetProperty(PropertyGeneratingModel model, int modelIndex)
    {
        var toTarget = model.Type.GetGeneratedNotAbstractTypeName();
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
    public {{enumType.ToUsingString()}}? To{{enumType.GetGeneratedNotAbstractTypeName()}}() => Value;
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
