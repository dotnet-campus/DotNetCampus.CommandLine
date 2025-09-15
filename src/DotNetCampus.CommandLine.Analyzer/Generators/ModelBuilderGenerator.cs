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
        // 对于不同的属性类型，生成不同的代码。
        // required: 属性要求必须赋值
        // nullable: 属性允许为 null
        // cli: 实际命令行参数是否传入

        // | required | nullable | cli | 行为       |
        // | -------- | -------- | --- | ---------- |
        // | 0        | 0        | 0   | 分析器警告 |
        // | 0        | 1        | 0   | 默认值     |
        // | 1        | _        | 0   | 抛异常     |
        // | _        | _        | 1   | 赋值       |

        var modifier = model.IsPublic ? "public" : "internal";
        var builder = new SourceTextBuilder(model.Namespace)
            .Using("System")
            .Using("DotNetCampus.Cli.Compiler")
            .AddTypeDeclaration($"{modifier} sealed class {model.GetBuilderTypeName()}(global::DotNetCampus.Cli.CommandLine commandLine)", t => t
                .WithDocumentationComment($"""
/// <summary>
/// 辅助 <see cref="{model.CommandObjectType.ToGlobalDisplayString()}"/> 生成命令行选项、子命令或处理函数的创建。
/// </summary>
""")
                .AddRawMembers(model.OptionProperties.Select(x => GenerateOptionPropertyCode(model.Namespace, x)))
                .AddRawText(GenerateBuildCode(model))
                .AddRawText(GenerateMatchLongOptionCode(model))
                .AddRawText(GenerateMatchShortOptionCode(model))
                .AddRawText(GenerateMatchPositionalArgumentsCode(model))
                .AddRawText(GenerateAssignPropertyValueCode(model))
                .AddRawText(GenerateBuildCoreCode(model))
            );
        return builder.ToString();
    }

    private string GenerateOptionPropertyCode(string @namespace, OptionPropertyGeneratingModel model) => model.Type.AsCommandPropertyType() switch
    {
        CommandPropertyType.Boolean => $"private global::DotNetCampus.Cli.Compiler.BooleanArgument {model.PropertyName} {{ get; }} = new();",
        CommandPropertyType.Number => $"private global::DotNetCampus.Cli.Compiler.NumberArgument {model.PropertyName} {{ get; }} = new();",
        CommandPropertyType.Enum => $"private {@namespace}.{model.Type.GetGeneratedEnumArgumentTypeName()} {model.PropertyName} {{ get; }} = new();",
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
        return BuildCore();
    }
    """;

    private string GenerateMatchLongOptionCode(CommandObjectGeneratingModel model)
    {
        var optionProperties = model.OptionProperties;
        return $$"""
    private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchLongOption(ReadOnlySpan<char> longOption, bool defaultCaseSensitive, CommandNamingPolicy namingPolicy)
    {
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

        return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;
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
        return $$"""
    private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchShortOption(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive)
    {
        var defaultComparison = defaultCaseSensitive ? global::System.StringComparison.Ordinal : global::System.StringComparison.OrdinalIgnoreCase;
    
    {{string.Join("\n", optionProperties.Select(x => GenerateOptionMatchCode(x, x.GetShortNames())))}}

        return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;
    }
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
        return $$"""
    private global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch MatchPositionalArguments(ReadOnlySpan<char> value, int argumentIndex)
    {
    }
    """;
    }

    private string GenerateAssignPropertyValueCode(CommandObjectGeneratingModel model)
    {
        return $$"""
    private void AssignPropertyValue(string propertyName, int propertyIndex, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
    }
    """;
    }

    private string GenerateBuildCoreCode(CommandObjectGeneratingModel model)
    {
        return $$"""
    private {{model.CommandObjectType.ToUsingString()}} BuildCore()
    {
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
