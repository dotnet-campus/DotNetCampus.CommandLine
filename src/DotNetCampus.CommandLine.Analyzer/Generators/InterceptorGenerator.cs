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
        var commandRunnerAddHandlerActionTProvider = context.SelectCommandBuilderAddHandlerProvider("System.Action<T>");
        var commandRunnerAddHandlerFuncTTaskProvider = context.SelectCommandBuilderAddHandlerProvider("System.Func<T, System.Threading.Tasks.Task>");
        var commandRunnerAddHandlerFuncTIntProvider = context.SelectCommandBuilderAddHandlerProvider("System.Func<T, int>");
        var commandRunnerAddHandlerFuncTTaskIntProvider = context.SelectCommandBuilderAddHandlerProvider("System.Func<T, System.Threading.Tasks.Task<int>>");

        context.RegisterSourceOutput(commandLineAsProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandLineAs);
        context.RegisterSourceOutput(commandRunnerAddHandlerProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandRunnerAddHandler);
        context.RegisterSourceOutput(commandRunnerAddHandlerActionTProvider.Collect().Combine(analyzerConfigOptionsProvider),
            (c, args) => CommandRunnerAddHandlerAction(c, args, "1", "System.Action<T>"));
        // context.RegisterSourceOutput(commandRunnerAddHandlerFuncTTaskProvider.Collect().Combine(analyzerConfigOptionsProvider),
        //     (c, args) => CommandRunnerAddHandlerAction(c, args, "2", "System.Func<T, System.Threading.Tasks.Task>"));
        // context.RegisterSourceOutput(commandRunnerAddHandlerFuncTIntProvider.Collect().Combine(analyzerConfigOptionsProvider),
        //     (c, args) => CommandRunnerAddHandlerAction(c, args, "3", "System.Func<T, int>"));
        // context.RegisterSourceOutput(commandRunnerAddHandlerFuncTTaskIntProvider.Collect().Combine(analyzerConfigOptionsProvider),
        //     (c, args) => CommandRunnerAddHandlerAction(c, args, "4", "System.Func<T, System.Threading.Tasks.Task<int>>"));
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
        context.AddSource("CommandLine.Interceptors/CommandBuilder.AddHandler.0.g.cs", code);
    }

    /// <summary>
    /// CommandRunner.AddHandler(Action)
    /// </summary>
    private void CommandRunnerAddHandlerAction(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string fileName, string parameterTypeFullName)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, x => GenerateCommandBuilderAddHandlerActionCode(parameterTypeFullName, x));
        context.AddSource($"CommandLine.Interceptors/CommandBuilder.AddHandler.{fileName}.g.cs", code);
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
        [global::System.Runtime.CompilerServices.InterceptsLocation({model.InterceptableLocation.Version}, "{model.InterceptableLocation.Data}")]
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
            return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, null, global::{{model.CommandObjectType.ContainingNamespace}}.{{model.GetBuilderTypeName()}}.CreateInstance);
        }
""";
    }

    private string GenerateCommandBuilderAddHandlerActionCode(string parameterTypeFullName, ImmutableArray<InterceptorGeneratingModel> models)
    {
        var model = models[0];
        return $$"""
        /// <summary>
        /// <see cref="global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler{T}(global::DotNetCampus.Cli.ICoreCommandRunnerBuilder,global::{{parameterTypeFullName.Replace('<', '{').Replace('>', '}')}})"/> 方法的拦截器。拦截以提高性能。
        /// </summary>
{{string.Join("\n", models.Select(GenerateInterceptsLocationCode))}}
        public static global::DotNetCampus.Cli.ICommandRunnerBuilder CommandBuilder_AddHandler_{{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}}<T>(this global::DotNetCampus.Cli.ICoreCommandRunnerBuilder builder,
            global::{{parameterTypeFullName}} handler)
            where T : class
        {
            return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, null, global::{{model.CommandObjectType.ContainingNamespace}}.{{model.GetBuilderTypeName()}}.CreateInstance, handler);
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
