using System.Collections.Immutable;
using DotNetCampus.CommandLine.Generators.ModelProviding;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators;

[Generator(LanguageNames.CSharp)]
public class VerbCreatorGenerator : IIncrementalGenerator
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
        var code = GenerateVerbCreatorCode(model);
        context.AddSource($"CommandLine.Models/{model.Namespace}.{model.OptionsType.Name}.cs", code);
    }

    private void Execute(SourceProductionContext context,
        (ImmutableArray<AssemblyCommandsGeneratingModel> Left, ImmutableArray<CommandOptionsGeneratingModel> Right) generatingModels)
    {
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

    private string GenerateVerbCreatorCode(CommandOptionsGeneratingModel model)
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
        var methodName = property.IsValueType ? "GetOptionValue" : "GetOption";
        var generic = property.Type.ToNotNullGlobalDisplayString();
        var caseSensitive = property.CaseSensitive switch
        {
            true => ", true",
            false => ", false",
            null => "",
        };

        string ArgumentsCreator(string name) =>
            $"{(property.ShortName is { } shortName ? $"'{shortName}', " : "")}{(name.Contains(' ') ? name : $"\"{name}\"")}{caseSensitive}";

        var exception = property.IsRequired
            ? $"throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required option '{property.GetDisplayCommandOption()}'. Command line: {{commandLine}}\", \"{property.PropertyName}\")"
            : property.IsValueType
                ? "default"
                : "null!";
        var caseSensitiveNotDeterminedNames = property.GetNormalizedLongNames();
        var runtimeName = caseSensitiveNotDeterminedNames.Length is 2
            ? $"caseSensitive ? \"{caseSensitiveNotDeterminedNames[0]}\" : \"{caseSensitiveNotDeterminedNames[1]}\""
            : caseSensitiveNotDeterminedNames[0];
        if (property.IsRequired || property.IsInitOnly)
        {
            if (property.Aliases.Length is 0)
            {
                return $"""
            {property.PropertyName} = commandLine.{methodName}<{generic}>({ArgumentsCreator(runtimeName)}) ?? {exception},
""";
            }
            else
            {
                return $"""
            {property.PropertyName} = commandLine.{methodName}<{generic}>({ArgumentsCreator(runtimeName)})
{string.Join("\n", property.Aliases.Select(x => $"            ?? commandLine.{methodName}<{generic}>({ArgumentsCreator(x)})"))}
                ?? {exception},
""";
            }
        }
        else
        {
            if (property.Aliases.Length is 0)
            {
                return $$"""
        if (commandLine.{{methodName}}<{{generic}}>({{ArgumentsCreator(runtimeName)}}) is { } o{{modelIndex}})
        {
            result.{{property.PropertyName}} = o{{modelIndex}};
        }
""";
            }
            else
            {
                return $$"""
        if (commandLine.{{methodName}}<{{generic}}>({{ArgumentsCreator(runtimeName)}})
{{string.Join("\n", property.Aliases.Select(x => $"            ?? commandLine.{methodName}<{generic}>({ArgumentsCreator(x)})"))}}
            is { } o{{modelIndex}})
        {
            result.{{property.PropertyName}} = o{{modelIndex}};
        }
""";
            }
        }
    }

    private string GenerateValuePropertyAssignment(CommandOptionsGeneratingModel model, ValuePropertyGeneratingModel property, int modelIndex)
    {
        var methodName = property.IsValueType ? "GetPositionalArgumentValue" : "GetPositionalArgument";
        var generic = property.Type.ToNotNullGlobalDisplayString();
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
            {property.PropertyName} = commandLine.{methodName}<{generic}>({(indexLengthCode is not null ? $"{indexLengthCode}, {verbText}" : verbText)}) ?? {exception},
""";
        }
        else
        {
            return $$"""
        if (commandLine.{{methodName}}<{{generic}}>({{(indexLengthCode is not null ? $"{indexLengthCode}, {verbText}" : verbText)}}) is { } p{{modelIndex}})
        {
            result.{{property.PropertyName}} = p{{modelIndex}};
        }
""";
        }
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
