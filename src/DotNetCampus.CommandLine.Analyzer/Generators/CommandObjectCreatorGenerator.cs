using System.Collections.Immutable;
using DotNetCampus.CommandLine.Generators.ModelProviding;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators;

[Generator(LanguageNames.CSharp)]
public class CommandObjectCreatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandOptionsProvider = context.SelectCommandOptions();
        var assemblyCommandsProvider = context.SelectAssemblyCommands();

        context.RegisterSourceOutput(
            commandOptionsProvider,
            Execute);

        context.RegisterSourceOutput(
            assemblyCommandsProvider.Collect().Combine(commandOptionsProvider.Collect()),
            Execute);
    }

    private void Execute(SourceProductionContext context, CommandOptionsGeneratingModel model)
    {
        var code = GenerateCommandObjectCreatorCode(model);
        context.AddSource($"CommandLine.Models/{model.Namespace}.{model.OptionsType.Name}.cs", code);
    }

    private void Execute(SourceProductionContext context,
        (ImmutableArray<AssemblyCommandsGeneratingModel> Left, ImmutableArray<CommandOptionsGeneratingModel> Right) generatingModels)
    {
        return;

        var (assemblyCommandsGeneratingModels, commandOptionsGeneratingModels) = generatingModels;
        commandOptionsGeneratingModels = [..commandOptionsGeneratingModels.OrderBy(x => x.GetBuilderTypeName())];

        var moduleInitializerCode = GenerateModuleInitializerCode(commandOptionsGeneratingModels);
        context.AddSource("CommandLine.Metadata/_ModuleInitializer.g.cs", moduleInitializerCode);

        foreach (var assemblyCommandsGeneratingModel in assemblyCommandsGeneratingModels)
        {
            var code = GenerateAssemblyCommandHandlerCode(assemblyCommandsGeneratingModel, commandOptionsGeneratingModels);
            context.AddSource(
                $"CommandLine.Metadata/{assemblyCommandsGeneratingModel.Namespace}.{assemblyCommandsGeneratingModel.AssemblyCommandHandlerType.Name}.g.cs",
                code);
        }
    }

    private string GenerateCommandObjectCreatorCode(CommandOptionsGeneratingModel model)
    {
        // | required | nullable | cli | 行为       |
        // | -------- | -------- | --- | ---------- |
        // | 0        | 0        | 0   | 分析器警告 |
        // | 1        | 0        | 0   | 抛异常     |
        // | 0        | 1        | 0   | 默认值     |
        // | 1        | 1        | 0   | 抛异常     |
        // | 0        | 0        | 1   | 赋值       |
        // | 1        | 0        | 1   | 赋值       |
        // | 0        | 1        | 1   | 赋值       |
        // | 1        | 1        | 1   | 赋值       |

        var initOptionProperties = model.OptionProperties.Where(x => x.IsRequired || x.IsInitOnly).ToImmutableArray();
        var initValueProperties = model.ValueProperties.Where(x => x.IsRequired || x.IsInitOnly).ToImmutableArray();
        var setOptionProperties = model.OptionProperties.Where(x => !x.IsRequired && !x.IsInitOnly).ToImmutableArray();
        var setValueProperties = model.ValueProperties.Where(x => !x.IsRequired && !x.IsInitOnly).ToImmutableArray();
        return $$"""
#nullable enable
namespace {{model.Namespace}};

/// <summary>
/// 辅助 <see cref="{{model.OptionsType.ToGlobalDisplayString()}}"/> 生成命令行选项、谓词或处理函数的创建。
/// </summary>
internal sealed class {{model.GetBuilderTypeName()}}
{
    public static object CreateInstance(global::DotNetCampus.Cli.CommandLine commandLine)
    {
        var caseSensitive = commandLine.DefaultCaseSensitive;
        var result = new {{model.OptionsType.ToGlobalDisplayString()}}
        {
{{(initOptionProperties.Length is 0 ? "            // There is no option to be initialized." : string.Join("\n", initOptionProperties.Select(GenerateOptionPropertyAssignment)))}}
{{(initValueProperties.Length is 0 ? "            // There is no positional argument to be initialized." : string.Join("\n", initValueProperties.Select((x, i) => GenerateValuePropertyAssignment(model, x, i))))}}
        };
{{(setOptionProperties.Length is 0 ? "        // There is no option to be assigned." : string.Join("\n", setOptionProperties.Select(GenerateOptionPropertyAssignment)))}}
{{(setValueProperties.Length is 0 ? "        // There is no positional argument to be assigned." : string.Join("\n", setValueProperties.Select((x, i) => GenerateValuePropertyAssignment(model, x, i))))}}
        return result;
    }
}

""";
    }

    private string GenerateOptionPropertyAssignment(OptionPropertyGeneratingModel property, int modelIndex)
    {
        var isInitProperty = property.IsRequired || property.IsInitOnly;
        var toMethod = GetCommandLinePropertyValueToMethodName(property.Type) is { } tm ? $"?.{tm}()" : "";
        var caseSensitive = property.CaseSensitive switch
        {
            true => ", true",
            false => ", false",
            null => "",
        };
        var exception = property.IsRequired
            ? $"throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required option '{property.GetDisplayCommandOption()}'. Command line: {{commandLine}}\", \"{property.PropertyName}\")"
            : property.IsValueType
                ? "default"
                : "null!";

        var getters = property.GenerateAllNames(
            shortOption => $"""commandLine.GetShortOption("{shortOption}"{caseSensitive})""",
            longOption => $"""commandLine.GetOption("{longOption}"{caseSensitive})""",
            (caseSensitiveLongOption, ignoreCaseLongName) => $"""commandLine.GetOption(caseSensitive ? "{caseSensitiveLongOption}" : "{ignoreCaseLongName}"{caseSensitive})""",
            aliasOption => $"""commandLine.GetOption("{aliasOption}")"""
        );

        return (isInitProperty, getters) switch
        {
            // [Option("OptionName")]
            // public required string PropertyName { get; init; }
            (true, { Count: 1 }) => $"""
            {property.PropertyName} = {getters[0]}{toMethod} ?? {exception},
""",
            // [Option('o', "OptionName")]
            // public required string PropertyName { get; init; }
            (true, _) => $"""
            {property.PropertyName} = ({string.Join("\n                ?? ", getters)}){toMethod}
                ?? {exception},
""",
            // [Option("OptionName")]
            // public string PropertyName { get; set; }
            (false, { Count: 1 }) => $$"""
        if ({{getters[0]}}{{toMethod}} is { } o{{modelIndex}})
        {
            result.{{property.PropertyName}} = o{{modelIndex}};
        }
""",
            // [Option('o', "OptionName")]
            // public string PropertyName { get; set; }
            (false, _) => $$"""
        if (({{string.Join("\n            ?? ", getters)}}){{toMethod}} is { } o{{modelIndex}})
        {
            result.{{property.PropertyName}} = o{{modelIndex}};
        }
""",
        };
    }

    private string GenerateValuePropertyAssignment(CommandOptionsGeneratingModel model, ValuePropertyGeneratingModel property, int modelIndex)
    {
        var toMethod = GetCommandLinePropertyValueToMethodName(property.Type) is { } tm ? $"?.{tm}()" : "";
        var indexLengthCode = (property.Index, property.Length) switch
        {
            (null, null) => null,
            (null, { } length) => $"0, {length}",
            ({ } index, null) => $"{index}, 1",
            ({ } index, { } length) => $"{index}, {length}",
        };
        var verbText = model.VerbName is { } verbName ? $"\"{verbName}\"" : "null";
        var exception = property.IsRequired
            ? $"throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required positional argument at {property.Index ?? 0}. Command line: {{commandLine}}\", \"{property.PropertyName}\")"
            : property.IsValueType
                ? "default"
                : "null!";
        if (property.IsRequired || property.IsInitOnly)
        {
            return $"""
            {property.PropertyName} = commandLine.GetPositionalArgument({(indexLengthCode is not null ? $"{indexLengthCode}, {verbText}" : verbText)}){toMethod} ?? {exception},
""";
        }
        else
        {
            return $$"""
        if (commandLine.GetPositionalArgument({{(indexLengthCode is not null ? $"{indexLengthCode}, {verbText}" : verbText)}}){{toMethod}} is { } p{{modelIndex}})
        {
            result.{{property.PropertyName}} = p{{modelIndex}};
        }
""";
        }
    }

    /// <summary>
    /// 获取一个方法名，调用该方法可使“命令行属性值”转换为“目标类型”。
    /// </summary>
    /// <param name="targetType">目标类型。</param>
    /// <returns>方法名。</returns>
    private string? GetCommandLinePropertyValueToMethodName(ITypeSymbol targetType)
    {
        // 特殊处理接口，因为接口不支持隐式转换，所以要调用专门的转换方法。
        if (targetType.TypeKind is TypeKind.Interface)
        {
            return targetType.Name switch
            {
                "IEnumerable" or "IReadOnlyList" => "AsReadOnlyList",
                "IList" or "ICollection" => "ToList",
                "IReadOnlyDictionary" or "IDictionary" => "ToDictionary",
                // 专门生成不存在的方法名和全名注释，编译不通过，同时还能辅助报告错误原因。
                _ => $"To{targetType.Name}/* {targetType.ToDisplayString()} */",
            };
        }

        // 特殊处理枚举和可空枚举，因为枚举类型不可穷举，所以要调用专门的转换方法。
        if (targetType.ToDisplayString().EndsWith("?") && targetType.TypeKind is TypeKind.Struct)
        {
            // 拿到可空类型内部的类型，如 int? -> int。
            targetType = ((INamedTypeSymbol)targetType).TypeArguments[0];
        }
        if (targetType.TypeKind is TypeKind.Enum)
        {
            return $"ToEnum<{targetType.ToNotNullGlobalDisplayString()}>";
        }

        // 其他类型使用隐式转换。
        return null;
    }

    private string GenerateModuleInitializerCode(ImmutableArray<CommandOptionsGeneratingModel> models)
    {
        return $$"""
#nullable enable
namespace DotNetCampus.Cli;

/// <summary>
/// 为本程序集中的所有命令行选项、谓词或处理函数编译时信息初始化。
/// </summary>
internal static class CommandLineModuleInitializer
{
    [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
    internal static void Initialize()
    {
{{string.Join("\n\n", models.Select(GenerateCommandRunnerRegisterCode))}}
    }
}

""";
    }

    private string GenerateCommandRunnerRegisterCode(CommandOptionsGeneratingModel model)
    {
        var verbCode = model.VerbName is { } vn ? $"\"{vn}\"" : "null";
        return $$"""
        // {{model.OptionsType.Name}} { VerbName = {{verbCode}} }
        global::DotNetCampus.Cli.CommandRunner.Register<{{model.OptionsType.ToGlobalDisplayString()}}>(
            {{verbCode}},
            global::{{model.Namespace}}.{{model.GetBuilderTypeName()}}.CreateInstance);
""";
    }

    private string GenerateAssemblyCommandHandlerCode(AssemblyCommandsGeneratingModel model, ImmutableArray<CommandOptionsGeneratingModel> models)
    {
        return $$"""
#nullable enable
namespace {{model.Namespace}};

/// <summary>
/// 提供一种辅助自动搜集并执行本程序集中所有命令行处理器的方式。
/// </summary>
partial class {{model.AssemblyCommandHandlerType.Name}} : global::DotNetCampus.Cli.Compiler.ICommandHandlerCollection
{
    public global::DotNetCampus.Cli.ICommandHandler? TryMatch(string? verb, global::DotNetCampus.Cli.CommandLine cl) => verb switch
    {
{{string.Join("\n", models.GroupBy(x => x.VerbName).Select(GenerateAssemblyCommandHandlerMatchCode))}}
        _ => null,
    };
}

""";
    }

    private string GenerateAssemblyCommandHandlerMatchCode(IGrouping<string?, CommandOptionsGeneratingModel> group)
    {
        var models = group.ToList();
        if (models.Count is 1)
        {
            var model = models[0];
            if (model.IsHandler)
            {
                return $"""
        {(group.Key is { } vn ? $"\"{vn}\"" : "null")} => (global::DotNetCampus.Cli.ICommandHandler)global::{model.Namespace}.{model.GetBuilderTypeName()}.CreateInstance(cl),
""";
            }
            else
            {
                return $"""
        // 类型 {model.OptionsType.Name} 没有继承 ICommandHandler 接口，因此无法统一调度执行，只能由开发者单独调用。
""";
            }
        }
        else
        {
            var verbCode = group.Key is { } vn ? $"\"{vn}\"" : "null";
            return $"""
        {verbCode} => throw new global::DotNetCampus.Cli.Exceptions.CommandVerbAmbiguityException($"Multiple command handlers match the same verb name '{group.Key ?? "null"}': {string.Join(", ", models.Select(x => x.OptionsType.Name))}.", {verbCode}),
""";
        }
    }
}
