#pragma warning disable RSEXPERIMENTAL002
using System.Collections.Immutable;
using DotNetCampus.Cli.Utils;
using DotNetCampus.CommandLine.Generators.ModelProviding;
using DotNetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotNetCampus.CommandLine.Generators;

[Generator(LanguageNames.CSharp)]
public class InterceptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var analyzerConfigOptionsProvider = context.AnalyzerConfigOptionsProvider;
        var commandLineAsProvider = context.SelectCommandLineAsProvider();
        var commandRunnerAddHandlerProvider = context.SelectCommandBuilderAddHandlerProvider();
        var commandRunnerAddHandlerCoreActionProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "global::System.Action<T>");
        var commandRunnerAddHandlerAsyncActionProvider = context.SelectCommandBuilderAddHandlerProvider("IAsyncCommandRunnerBuilder", "global::System.Action<T>");
        var commandRunnerAddHandlerCoreFuncIntProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "global::System.Func<T, int>");
        var commandRunnerAddHandlerAsyncFuncIntProvider = context.SelectCommandBuilderAddHandlerProvider("IAsyncCommandRunnerBuilder", "global::System.Func<T, int>");
        var commandRunnerAddHandlerCoreFuncTaskProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task>");
        var commandRunnerAddHandlerCoreFuncTaskIntProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task<int>>");

        context.RegisterSourceOutput(commandLineAsProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandLineAs);
        context.RegisterSourceOutput(commandRunnerAddHandlerProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandRunnerAddHandler);

        // ICommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Action<T> handler)
        context.RegisterSourceOutput(commandRunnerAddHandlerCoreActionProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICoreCommandRunnerBuilder.AddHandler(Action{T})", "ICoreCommandRunnerBuilder", "System.Action<T>", "ICommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Action<T> handler)
        context.RegisterSourceOutput(commandRunnerAddHandlerAsyncActionProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "IAsyncCommandRunnerBuilder.AddHandler(Action{T})", "IAsyncCommandRunnerBuilder", "System.Action<T>", "IAsyncCommandRunnerBuilder"));

        // ICommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, int> handler)
        context.RegisterSourceOutput(commandRunnerAddHandlerCoreFuncIntProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICoreCommandRunnerBuilder.AddHandler(Func{T,int})", "ICoreCommandRunnerBuilder", "System.Func<T, int>", "ICommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, int> handler)
        context.RegisterSourceOutput(commandRunnerAddHandlerAsyncFuncIntProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "IAsyncCommandRunnerBuilder.AddHandler(Func{T,int})", "IAsyncCommandRunnerBuilder", "System.Func<T, int>", "IAsyncCommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, Task> handler)
        context.RegisterSourceOutput(commandRunnerAddHandlerCoreFuncTaskProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICoreCommandRunnerBuilder.AddHandler(Func{T,Task})", "ICoreCommandRunnerBuilder", "System.Func<T, System.Threading.Tasks.Task>",
                "IAsyncCommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, Task<int>> handler)
        context.RegisterSourceOutput(commandRunnerAddHandlerCoreFuncTaskIntProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICoreCommandRunnerBuilder.AddHandler(Func{T,Task{int}})", "ICoreCommandRunnerBuilder", "System.Func<T, System.Threading.Tasks.Task<int>>",
                "IAsyncCommandRunnerBuilder"));
    }

    /// <summary>
    /// CommandLine.As
    /// </summary>
    private void CommandLineAs(SourceProductionContext context, (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, GenerateCommandLineAsCode);
        context.AddSource("CommandLine.Interceptors/CommandLine.As.g.cs", code);
    }

    /// <summary>
    /// CommandRunner.AddHandler
    /// </summary>
    private void CommandRunnerAddHandler(SourceProductionContext context, (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, GenerateCommandBuilderAddHandlerCode);
        context.AddSource("CommandLine.Interceptors/CommandBuilder.AddHandler.g.cs", code);
    }

    /// <summary>
    /// CommandRunner.AddHandler(Action)
    /// </summary>
    private void CommandRunnerAddHandlerAction(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string fileName, string parameterThisName, string parameterTypeFullName, string returnName)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, x => GenerateCommandBuilderAddHandlerActionCode(x, parameterThisName, parameterTypeFullName, returnName));
        context.AddSource($"CommandLine.Interceptors/{fileName}.g.cs", code);
    }

    private string GenerateCode(Dictionary<ISymbol, ImmutableArray<InterceptorGeneratingModel>> models,
        Func<ImmutableArray<InterceptorGeneratingModel>, string> methodCreator)
    {
        return $$"""
#nullable enable

namespace {{GeneratorInfo.RootNamespace}}.Compiler
{
    file static class Interceptors
    {
{{string.Join("\n\n", models.Select(x => methodCreator(x.Value)))}}
    }
}

namespace System.Runtime.CompilerServices
{
    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute : global::System.Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
            _ = version;
            _ = data;
        }
    }
}

""";
    }

    private string GenerateInterceptsLocationCode(InterceptorGeneratingModel model)
    {
        return $"""
        [global::System.Runtime.CompilerServices.InterceptsLocation({model.InterceptableLocation.Version}, /* {model.InvocationInfo} */ "{model.InterceptableLocation.Data}")]
""";
    }

    private string GenerateCommandLineAsCode(ImmutableArray<InterceptorGeneratingModel> models)
    {
        var model = models[0];
        return $$"""
        /// <summary>
        /// <see cref="global::DotNetCampus.Cli.CommandLine.As{{{model.CommandObjectType.Name}}}()"/> 方法的拦截器。拦截以提高性能。
        /// </summary>
{{string.Join("\n", models.Select(GenerateInterceptsLocationCode))}}
        public static T CommandLine_As_{{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}}<T>(this global::DotNetCampus.Cli.CommandLine commandLine)
            where T : {{model.CommandObjectType.ToGlobalDisplayString()}}
        {
            return (T)global::{{model.CommandObjectType.ContainingNamespace}}.{{model.GetBuilderTypeName()}}.CreateInstance(commandLine);
        }
""";
    }

    private string GenerateCommandBuilderAddHandlerCode(ImmutableArray<InterceptorGeneratingModel> models)
    {
        var model = models[0];
        return $$"""
        /// <summary>
        /// <see cref="global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler{{{model.CommandObjectType.Name}}}(global::DotNetCampus.Cli.ICoreCommandRunnerBuilder)"/> 方法的拦截器。拦截以提高性能。
        /// </summary>
{{string.Join("\n", models.Select(GenerateInterceptsLocationCode))}}
        public static global::DotNetCampus.Cli.IAsyncCommandRunnerBuilder CommandBuilder_AddHandler_{{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}}<T>(this global::DotNetCampus.Cli.ICoreCommandRunnerBuilder builder)
            where T : {{model.CommandObjectType.ToGlobalDisplayString()}}, global::DotNetCampus.Cli.ICommandHandler
        {
            return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, {{(model.VerbName is { } verb ? $"\"{verb}\"" : "null")}}, global::{{model.CommandObjectType.ContainingNamespace}}.{{model.GetBuilderTypeName()}}.CreateInstance);
        }
""";
    }

    private string GenerateCommandBuilderAddHandlerActionCode(ImmutableArray<InterceptorGeneratingModel> models, string parameterThisName, string parameterTypeFullName, string returnName)
    {
        var model = models[0];
        return $$"""
        /// <summary>
        /// <see cref="global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler{T}(global::DotNetCampus.Cli.ICoreCommandRunnerBuilder,global::{{parameterTypeFullName.Replace('<', '{').Replace('>', '}')}})"/> 方法的拦截器。拦截以提高性能。
        /// </summary>
{{string.Join("\n", models.Select(GenerateInterceptsLocationCode))}}
        public static global::DotNetCampus.Cli.{{returnName}} CommandBuilder_AddHandler_{{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}}<T>(this global::DotNetCampus.Cli.{{parameterThisName}} builder,
            global::{{parameterTypeFullName}} handler)
            where T : class
        {
            return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, {{(model.VerbName is { } verb ? $"\"{verb}\"" : "null")}}, global::{{model.CommandObjectType.ContainingNamespace}}.{{model.GetBuilderTypeName()}}.CreateInstance, handler);
        }
""";
    }
}

file static class Extensions
{
    public static Dictionary<ISymbol, ImmutableArray<InterceptorGeneratingModel>>? ToDictionary(
        this SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args)
    {
        var (models, analyzerConfigOptions) = args;
        if (analyzerConfigOptions.GlobalOptions.TryGetValue<bool>("DotNetCampusCommandLineUseInterceptor", out var useInterceptor)
            && !useInterceptor)
        {
            return null;
        }

        var modelGroups = models
            .GroupBy(x => x.CommandObjectType, SymbolEqualityComparer.Default)
            .ToDictionary(
                x => x.Key!,
                x => x.ToImmutableArray(),
                SymbolEqualityComparer.Default);
        return modelGroups;
    }
}
