using System.Collections.Immutable;
using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Utils;
using dotnetCampus.CommandLine.Generators.ModelProviding;
using dotnetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace dotnetCampus.CommandLine.Generators;

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
        context.AddSource($"{model.GetVerbCreatorTypeName()}.g.cs", code);
    }

    private void Execute(SourceProductionContext context,
        (ImmutableArray<AssemblyCommandsGeneratingModel> Left, ImmutableArray<CommandOptionsGeneratingModel> Right) generatingModels)
    {
        var (assemblyCommandsGeneratingModels, commandOptionsGeneratingModels) = generatingModels;
        commandOptionsGeneratingModels = [..commandOptionsGeneratingModels.OrderBy(x => x.GetVerbCreatorTypeName())];

        var moduleInitializerCode = GenerateModuleInitializerCode(commandOptionsGeneratingModels);
        context.AddSource("_ModuleInitializer.g.cs", moduleInitializerCode);

        foreach (var assemblyCommandsGeneratingModel in assemblyCommandsGeneratingModels)
        {
            var code = GenerateAssemblyCommandHandlerCode(assemblyCommandsGeneratingModel, commandOptionsGeneratingModels);
            context.AddSource($"{assemblyCommandsGeneratingModel.AssemblyCommandHandlerType.Name}.g.cs", code);
        }
    }

    private string GenerateVerbCreatorCode(CommandOptionsGeneratingModel model)
    {
        return $$"""
#nullable enable
namespace {{model.Namespace}};

/// <summary>
/// 辅助 <see cref="{{model.OptionsType.ToGlobalDisplayString()}}"/> 生成命令行选项、谓词或处理函数的创建。
/// </summary>
internal sealed class {{model.GetVerbCreatorTypeName()}} : global::dotnetCampus.Cli.Compiler.IVerbCreator<{{model.OptionsType.ToGlobalDisplayString()}}>
{
    public {{model.OptionsType.ToGlobalDisplayString()}} CreateInstance(global::dotnetCampus.Cli.CommandLine commandLine) => new()
    {
{{string.Join("\n", model.OptionProperties.Select(GenerateOptionPropertyAssignment))}}
{{string.Join("\n", model.ValueProperties.Select(GenerateValuePropertyAssignment))}}
    };
}

""";
    }

    private string GenerateOptionPropertyAssignment(OptionPropertyGeneratingModel property)
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

        return $"""
        {property.PropertyName} = commandLine.GetOption<{property.Type.ToGlobalDisplayString()}>("{property.LongName}") ?? {(property.IsRequired ? $"throw new global::dotnetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required option '{property.GetCommandOption()}'. Command line: {{commandLine}}\", \"{property.PropertyName}\")" : "default")},
""";
    }

    private string GenerateValuePropertyAssignment(ValuePropertyGeneratingModel property)
    {
        return $"""
        {property.PropertyName} = commandLine.GetValue<{property.Type.ToGlobalDisplayString()}>({(property.Index is { } index ? $"{index}, {property.Length ?? 1}" : "")}) ?? {(property.IsRequired ? $"throw new global::dotnetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($\"The command line arguments doesn't contain a required positional argument at {property.Index ?? 0}. Command line: {{commandLine}}\", \"{property.PropertyName}\")" : "default")},
""";
    }

    private string GenerateModuleInitializerCode(ImmutableArray<CommandOptionsGeneratingModel> models)
    {
        return $$"""
#nullable enable
namespace dotnetCampus.Cli;

/// <summary>
/// 为本程序集中的所有命令行选项、谓词或处理函数编译时信息初始化。
/// </summary>
internal static class CommandLineModuleInitializer
{
    [global::System.Runtime.CompilerServices.ModuleInitializerAttribute]
    internal static void Initialize()
    {
{{string.Join("\n", models.Select(GenerateCommandRunnerRegisterCode))}}
    }
}

""";
    }

    private string GenerateCommandRunnerRegisterCode(CommandOptionsGeneratingModel model)
    {
        return $"""
        global::dotnetCampus.Cli.CommandRunner.Register<{model.OptionsType.ToGlobalDisplayString()}>(
            {(model.VerbName is { } vn ? $"\"{vn}\"" : "null")},
            cl => new global::{model.Namespace}.{model.GetVerbCreatorTypeName()}().CreateInstance(cl));
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
partial class {{model.AssemblyCommandHandlerType.Name}} : global::dotnetCampus.Cli.Compiler.ICommandHandlerCollection
{
    public global::dotnetCampus.Cli.ICommandHandler? TryMatch(string? verb, global::dotnetCampus.Cli.CommandLine cl) => verb switch
    {
{{string.Join("\n", models.Select(GenerateAssemblyCommandHandlerMatchCode))}}
        _ => null,
    };
}

""";
    }

    private string GenerateAssemblyCommandHandlerMatchCode(CommandOptionsGeneratingModel model)
    {
        return $"""
        {(model.VerbName is { } vn ? $"\"{vn}\"" : "null")} => new global::{model.Namespace}.{model.GetVerbCreatorTypeName()}().CreateInstance(cl),
""";
    }
}
