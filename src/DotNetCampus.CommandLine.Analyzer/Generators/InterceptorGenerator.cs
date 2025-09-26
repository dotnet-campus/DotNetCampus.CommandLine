#pragma warning disable RSEXPERIMENTAL002
using System.Collections.Immutable;
using DotNetCampus.Cli.Utils;
using DotNetCampus.CommandLine.Generators.Builders;
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
        var asProvider = context.SelectCommandLineAsProvider();
        var addHandlerProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder");
        var addHandlerWithObjectStateProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "object?");
        var addHandlerWithTStateProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "TState?");
        var addHandlerCoreActionProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "global::System.Action<T>");
        var addHandlerAsyncActionProvider = context.SelectCommandBuilderAddHandlerProvider("IAsyncCommandRunnerBuilder", "global::System.Action<T>");
        var addHandlerCoreFuncIntProvider = context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "global::System.Func<T, int>");
        var addHandlerAsyncFuncIntProvider = context.SelectCommandBuilderAddHandlerProvider("IAsyncCommandRunnerBuilder", "global::System.Func<T, int>");
        var addHandlerCoreFuncTaskProvider =
            context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task>");
        var addHandlerCoreFuncTaskIntProvider =
            context.SelectCommandBuilderAddHandlerProvider("ICoreCommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task<int>>");

        context.RegisterSourceOutput(asProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandLineAs);
        context.RegisterSourceOutput(addHandlerProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandRunnerAddHandler);
        context.RegisterSourceOutput(addHandlerWithObjectStateProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandRunnerAddHandlerObjectState);
        context.RegisterSourceOutput(addHandlerWithTStateProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandRunnerAddHandlerObjectState);

        // ICommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Action<T> handler)
        context.RegisterSourceOutput(addHandlerCoreActionProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICoreCommandRunnerBuilder.AddHandler(Action{T})", "ICoreCommandRunnerBuilder", "System.Action<T>",
                "ICommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Action<T> handler)
        context.RegisterSourceOutput(addHandlerAsyncActionProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "IAsyncCommandRunnerBuilder.AddHandler(Action{T})", "IAsyncCommandRunnerBuilder", "System.Action<T>",
                "IAsyncCommandRunnerBuilder"));

        // ICommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, int> handler)
        context.RegisterSourceOutput(addHandlerCoreFuncIntProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICoreCommandRunnerBuilder.AddHandler(Func{T,int})", "ICoreCommandRunnerBuilder", "System.Func<T, int>",
                "ICommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder AddHandler<T>(this IAsyncCommandRunnerBuilder builder, Func<T, int> handler)
        context.RegisterSourceOutput(addHandlerAsyncFuncIntProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "IAsyncCommandRunnerBuilder.AddHandler(Func{T,int})", "IAsyncCommandRunnerBuilder", "System.Func<T, int>",
                "IAsyncCommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, Task> handler)
        context.RegisterSourceOutput(addHandlerCoreFuncTaskProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICoreCommandRunnerBuilder.AddHandler(Func{T,Task})", "ICoreCommandRunnerBuilder",
                "System.Func<T, System.Threading.Tasks.Task>",
                "IAsyncCommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder AddHandler<T>(this ICoreCommandRunnerBuilder builder, Func<T, Task<int>> handler)
        context.RegisterSourceOutput(addHandlerCoreFuncTaskIntProvider.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICoreCommandRunnerBuilder.AddHandler(Func{T,Task{int}})", "ICoreCommandRunnerBuilder",
                "System.Func<T, System.Threading.Tasks.Task<int>>",
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
    private void CommandRunnerAddHandler(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, GenerateCommandBuilderAddHandlerCode);
        context.AddSource("CommandLine.Interceptors/ICoreCommandRunnerBuilder.AddHandler().g.cs", code);
    }

    /// <summary>
    /// CommandRunner.AddHandler
    /// </summary>
    private void CommandRunnerAddHandlerObjectState(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, GenerateCommandBuilderAddStatedHandlerCode);
        context.AddSource("CommandLine.Interceptors/ICoreCommandRunnerBuilder.AddHandler(TState).g.cs", code);
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

        var code = GenerateCode(modelGroups, (t, x) =>
            GenerateCommandBuilderAddHandlerActionCode(t, x, parameterThisName, parameterTypeFullName, returnName));
        context.AddSource($"CommandLine.Interceptors/{fileName}.g.cs", code);
    }

    private string GenerateCode(Dictionary<ISymbol, ImmutableArray<InterceptorGeneratingModel>> models,
        Action<TypeDeclarationSourceTextBuilder, IReadOnlyList<InterceptorGeneratingModel>> methodCreator)
    {
        var builder = new SourceTextBuilder()
            .AddNamespaceDeclaration("DotNetCampus.Cli.Compiler", n => n
                .AddTypeDeclaration("file static class Interceptors", t =>
                {
                    foreach (var pair in models)
                    {
                        methodCreator(t, pair.Value);
                    }
                }))
            .AddNamespaceDeclaration("System.Runtime.CompilerServices", n => n
                .AddTypeDeclaration("file sealed class InterceptsLocationAttribute : global::System.Attribute", t => t
                    .AddAttribute("""[global::System.Diagnostics.Conditional("FOR_SOURCE_GENERATION_ONLY")]""")
                    .AddAttribute("[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]")
                    .AddRawText("""
                        public InterceptsLocationAttribute(int version, string data)
                        {
                            _ = version;
                            _ = data;
                        }                                
                        """)));
        return builder.ToString();
    }

    private string GenerateInterceptsLocationCode(InterceptorGeneratingModel model) => $"""
        [global::System.Runtime.CompilerServices.InterceptsLocation({model.InterceptableLocation.Version}, /* {model.InvocationInfo} */ "{model.InterceptableLocation.Data}")]
        """;

    private void GenerateCommandLineAsCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models)
    {
        var model = models[0];
        builder.AddMethodDeclaration(
            $"public static T CommandLine_As_{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}<T>(this global::DotNetCampus.Cli.CommandLine commandLine)",
            m => m
                .WithSummaryComment($$"""<see cref="global::DotNetCampus.Cli.CommandLine.As{{{model.CommandObjectType.Name}}}()"/> 方法的拦截器。拦截以提高性能。""")
                .AddAttributes(models.Select(GenerateInterceptsLocationCode))
                .AddTypeConstraints(model.UseFullStackParser ? "where T : struct" : "where T : notnull")
                .AddRawStatements($"""
                    // 请确保 {model.CommandObjectType.Name} 类型中至少有一个属性标记了 [Option] 或 [Value] 特性；
                    // 否则下面的 {model.GetBuilderTypeName()} 类型将不存在，导致编译不通过。
                    var instance = new global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}(commandLine).Build();
                    {(
                        model.UseFullStackParser
                            ? $"""
                    #if NET8_0_OR_GREATER
                    return global::System.Runtime.CompilerServices.Unsafe.BitCast<{model.CommandObjectType.ToGlobalDisplayString()}, T>(instance);
                    #else
                    return global::System.Runtime.CompilerServices.Unsafe.As<{model.CommandObjectType.ToGlobalDisplayString()}, T>(ref instance);
                    #endif
                    """
                            : "return (T)(object)instance;"
                    )}
                    """));
    }

    private void GenerateCommandBuilderAddHandlerCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models)
    {
        var model = models[0];
        builder.AddMethodDeclaration($"""
            public static global::DotNetCampus.Cli.IAsyncCommandRunnerBuilder CommandBuilder_AddHandler_{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}<T>(this global::DotNetCampus.Cli.ICoreCommandRunnerBuilder builder)
            """, m => m
            .WithSummaryComment(
                $$"""<see cref="global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler{{{model.CommandObjectType.Name}}}(global::DotNetCampus.Cli.ICoreCommandRunnerBuilder)"/> 方法的拦截器。拦截以提高性能。""")
            .AddAttributes(models.Select(GenerateInterceptsLocationCode))
            .AddTypeConstraints("where T : class, global::DotNetCampus.Cli.ICommandHandler")
            .AddRawStatements($"""
                // 请确保 {model.CommandObjectType.Name} 类型中至少有一个属性标记了 [Option] 或 [Value] 特性；
                // 否则下面的 {model.GetBuilderTypeName()} 类型将不存在，导致编译不通过。
                return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CommandNameGroup, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CreateInstance);
                """));
    }

    private void GenerateCommandBuilderAddStatedHandlerCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models)
    {
        var model = models[0];
        builder.AddMethodDeclaration($"""
            public static global::DotNetCampus.Cli.IAsyncCommandRunnerBuilder CommandBuilder_AddHandler_{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}<T>(this global::DotNetCampus.Cli.ICoreCommandRunnerBuilder builder)
            """, m => m
            .WithSummaryComment(
                $$"""<see cref="global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler{{{model.CommandObjectType.Name}}}(global::DotNetCampus.Cli.ICoreCommandRunnerBuilder)"/> 方法的拦截器。拦截以提高性能。""")
            .AddAttributes(models.Select(GenerateInterceptsLocationCode))
            .AddTypeConstraints("where T : class, global::DotNetCampus.Cli.ICommandHandler<TState>")
            .AddRawStatements($"""
                // 请确保 {model.CommandObjectType.Name} 类型中至少有一个属性标记了 [Option] 或 [Value] 特性；
                // 否则下面的 {model.GetBuilderTypeName()} 类型将不存在，导致编译不通过。
                return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CommandNameGroup, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CreateInstance);
                """));
    }

    private void GenerateCommandBuilderAddHandlerActionCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models,
        string parameterThisName, string parameterTypeFullName, string returnName)
    {
        var model = models[0];
        builder.AddMethodDeclaration($"""
            public static global::DotNetCampus.Cli.{returnName} CommandBuilder_AddHandler_{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}<T>(this global::DotNetCampus.Cli.{parameterThisName} builder, global::{parameterTypeFullName} handler)
            """, m => m
            .WithSummaryComment(
                $$"""<see cref="global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler{T}(global::DotNetCampus.Cli.ICoreCommandRunnerBuilder,global::{{parameterTypeFullName.Replace('<', '{').Replace('>', '}')}})"/> 方法的拦截器。拦截以提高性能。""")
            .AddAttributes(models.Select(GenerateInterceptsLocationCode))
            .AddTypeConstraints("where T : class")
            .AddRawStatements($"""
                // 请确保 {model.CommandObjectType.Name} 类型中至少有一个属性标记了 [Option] 或 [Value] 特性；
                // 否则下面的 {model.GetBuilderTypeName()} 类型将不存在，导致编译不通过。
                return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, handler, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CommandNameGroup, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CreateInstance);
                """));
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
