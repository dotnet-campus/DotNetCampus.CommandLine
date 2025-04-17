#pragma warning disable RSEXPERIMENTAL002
using System.Collections.Immutable;
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

        context.RegisterSourceOutput(commandLineAsProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandLineAs);
        context.RegisterSourceOutput(commandRunnerAddHandlerProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandRunnerAddHandler);
    }

    private void CommandLineAs(SourceProductionContext context, (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args)
    {
        if (context.ToDictionary(args) is not { } modelGroups)
        {
            return;
        }

        var code = GenerateCode(modelGroups, GenerateCommandLineAsCode);
        context.AddSource("CommandLine.Interceptors/CommandLine.As.g.cs", code);
    }

    private void CommandRunnerAddHandler(SourceProductionContext context, (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args)
    {
        if (context.ToDictionary(args) is not { } modelGroups)
        {
            return;
        }

        var code = GenerateCode(modelGroups, GenerateCommandBuilderAddHandlerCode);
        context.AddSource("CommandLine.Interceptors/CommandBuilder.AddHandler.g.cs", code);
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
        public static T CommandLineAs{{model.CommandObjectType.Name}}<T>(this global::DotNetCampus.Cli.CommandLine commandLine) where T : {{model.CommandObjectType.ToGlobalDisplayString()}}
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
        public static global::DotNetCampus.Cli.IAsyncCommandRunnerBuilder CommandBuilderAddHandler{{model.CommandObjectType.Name}}<T>(this global::DotNetCampus.Cli.ICoreCommandRunnerBuilder builder) where T : {{model.CommandObjectType.ToGlobalDisplayString()}}
        {
            return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, null,
                global::{{model.CommandObjectType.ContainingNamespace}}.{{model.GetBuilderTypeName()}}.CreateInstance);
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
