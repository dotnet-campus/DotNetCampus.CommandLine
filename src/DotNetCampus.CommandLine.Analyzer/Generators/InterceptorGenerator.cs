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
        var sync = context.SelectAddHandlers("ICommandRunnerBuilder");
        var async = context.SelectAddHandlers("IAsyncCommandRunnerBuilder");
        var statedSync = context.SelectAddHandlers("StatedCommandRunnerBuilder");
        var statedAsync = context.SelectAddHandlers("StatedCommandRunnerLinkedBuilder");
        var syncVoid = context.SelectAddHandlers("ICommandRunnerBuilder", "global::System.Action<T>");
        var syncInt32 = context.SelectAddHandlers("ICommandRunnerBuilder", "global::System.Func<T, int>");
        var syncTask = context.SelectAddHandlers("ICommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task>");
        var syncTaskInt32 = context.SelectAddHandlers("ICommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task<int>>");
        var asyncVoid = context.SelectAddHandlers("IAsyncCommandRunnerBuilder", "global::System.Action<T>");
        var asyncInt32 = context.SelectAddHandlers("IAsyncCommandRunnerBuilder", "global::System.Func<T, int>");
        var asyncTask = context.SelectAddHandlers("IAsyncCommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task>");
        var asyncTaskInt32 = context.SelectAddHandlers("IAsyncCommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task<int>>");

        var obsolete = context.SelectAddHandlers("CommandLine");
        var obsoleteVoid = context.SelectAddHandlers("CommandLine", "global::System.Action<T>");
        var obsoleteInt32 = context.SelectAddHandlers("CommandLine", "global::System.Func<T, int>");
        var obsoleteTask = context.SelectAddHandlers("CommandLine", "global::System.Func<T, global::System.Threading.Tasks.Task>");
        var obsoleteTaskInt32 = context.SelectAddHandlers("CommandLine", "global::System.Func<T, global::System.Threading.Tasks.Task<int>>");

        // CommandLine.As<T>
        context.RegisterSourceOutput(asProvider.Collect().Combine(analyzerConfigOptionsProvider), CommandLineAs);

        // CommandRunner.AddHandler<T>()
        context.RegisterSourceOutput(sync.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandler(c, args, "ICommandRunnerBuilder", "AddHandler()"));
        context.RegisterSourceOutput(async.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandler(c, args, "IAsyncCommandRunnerBuilder", "AddHandler()"));

        // StatedCommandRunner.AddHandler<T>()
        context.RegisterSourceOutput(statedSync.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            StatedCommandRunnerAddHandler(c, args, "StatedCommandRunnerBuilder", "AddHandler()"));
        context.RegisterSourceOutput(statedAsync.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            StatedCommandRunnerAddHandler(c, args, "StatedCommandRunnerLinkedBuilder", "AddHandler()"));

        // CommandRunner.AddHandler<T>(Action/Func/...)
        context.RegisterSourceOutput(syncVoid.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICommandRunnerBuilder", "AddHandler(Action{T})",
                "System.Action<T>", "ICommandRunnerBuilder"));
        context.RegisterSourceOutput(syncInt32.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICommandRunnerBuilder", "AddHandler(Func{T,int})",
                "System.Func<T, int>", "ICommandRunnerBuilder"));
        context.RegisterSourceOutput(syncTask.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICommandRunnerBuilder", "AddHandler(Func{T,Task})",
                "System.Func<T, System.Threading.Tasks.Task>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(syncTaskInt32.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "ICommandRunnerBuilder", "AddHandler(Func{T,Task{int}})",
                "System.Func<T, System.Threading.Tasks.Task<int>>", "IAsyncCommandRunnerBuilder"));

        // AsyncCommandRunner.AddHandler<T>(Action/Func/...)
        context.RegisterSourceOutput(asyncVoid.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "IAsyncCommandRunnerBuilder", "AddHandler(Action{T})",
                "System.Action<T>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(asyncInt32.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "IAsyncCommandRunnerBuilder", "AddHandler(Func{T,int})",
                "System.Func<T, int>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(asyncTask.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "IAsyncCommandRunnerBuilder", "AddHandler(Func{T,Task})",
                "System.Func<T, System.Threading.Tasks.Task>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(asyncTaskInt32.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            CommandRunnerAddHandlerAction(c, args, "IAsyncCommandRunnerBuilder", "AddHandler(Func{T,Task{int}})",
                "System.Func<T, System.Threading.Tasks.Task<int>>", "IAsyncCommandRunnerBuilder"));

#pragma warning disable CS0618
        // Obsolete APIs for migration
        context.RegisterSourceOutput(obsolete.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            ObsoleteAddHandler(c, args, "CommandLine", "AddHandler()"));
        context.RegisterSourceOutput(obsoleteVoid.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            ObsoleteAddHandlerAction(c, args, "CommandLine", "AddHandler(Action{T})",
                "System.Action<T>", "ICommandRunnerBuilder"));
        context.RegisterSourceOutput(obsoleteInt32.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            ObsoleteAddHandlerAction(c, args, "CommandLine", "AddHandler(Func{T,int})",
                "System.Func<T, int>", "ICommandRunnerBuilder"));
        context.RegisterSourceOutput(obsoleteTask.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            ObsoleteAddHandlerAction(c, args, "CommandLine", "AddHandler(Func{T,Task})",
                "System.Func<T, System.Threading.Tasks.Task>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(obsoleteTaskInt32.Collect().Combine(analyzerConfigOptionsProvider), (c, args) =>
            ObsoleteAddHandlerAction(c, args, "CommandLine", "AddHandler(Func{T,Task{int}})",
                "System.Func<T, System.Threading.Tasks.Task<int>>", "IAsyncCommandRunnerBuilder"));
#pragma warning restore CS0618
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
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string thisName, string methodSignature)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, (t, x) =>
            GenerateCommandBuilderAddHandlerCode(t, x, thisName));
        context.AddSource($"CommandLine.Interceptors/{thisName}.{methodSignature}.g.cs", code);
    }

    /// <summary>
    /// StatedCommandRunnerBuilder{TState}.AddHandler
    /// </summary>
    private void StatedCommandRunnerAddHandler(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string thisName, string methodSignature)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, (t, x) =>
            GenerateStatedCommandBuilderAddHandlerCode(t, x, thisName));
        context.AddSource($"CommandLine.Interceptors/{thisName}.{methodSignature}.g.cs", code);
    }

    /// <summary>
    /// CommandRunner.AddHandler(Action)
    /// </summary>
    private void CommandRunnerAddHandlerAction(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string thisName, string methodSignature, string parameterTypeFullName, string returnName)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, (t, x) =>
            GenerateCommandBuilderAddHandlerActionCode(t, x, thisName, parameterTypeFullName, returnName));
        context.AddSource($"CommandLine.Interceptors/{thisName}.{methodSignature}.g.cs", code);
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
                .AddRawStatement(GenerateComment(model))
                .AddRawStatements($$"""""
                    var context = new global::DotNetCampus.Cli.Compiler.CommandRunningContext { CommandLine = commandLine };
                    var instance = new global::{{model.CommandObjectType.ContainingNamespace}}.{{model.GetBuilderTypeName()}}().Build(context);
                    {{(
                        model.UseFullStackParser
                            ? $"""
                    #if NET8_0_OR_GREATER
                    return global::System.Runtime.CompilerServices.Unsafe.BitCast<{model.CommandObjectType.ToGlobalDisplayString()}, T>(instance);
                    #else
                    return global::System.Runtime.CompilerServices.Unsafe.As<{model.CommandObjectType.ToGlobalDisplayString()}, T>(ref instance);
                    #endif
                    """
                            : "return (T)(object)instance;"
                    )}}
                    """""));
    }

    private void GenerateCommandBuilderAddHandlerCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models,
        string parameterThisName)
    {
        var model = models[0];
        builder.AddMethodDeclaration($"""
            public static global::DotNetCampus.Cli.IAsyncCommandRunnerBuilder CommandBuilder_AddHandler_{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}<T>(this global::DotNetCampus.Cli.{parameterThisName} builder)
            """, m => m
            .WithSummaryComment(
                $$"""<see cref="global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler{{{model.CommandObjectType.Name}}}(global::DotNetCampus.Cli.ICommandRunnerBuilder)"/> 方法的拦截器。拦截以提高性能。""")
            .AddAttributes(models.Select(GenerateInterceptsLocationCode))
            .AddTypeConstraints("where T : class, global::DotNetCampus.Cli.ICommandHandler")
            .AddRawStatement(GenerateComment(model))
            .AddRawStatements($"""
                return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CommandNameGroup, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CreateInstance);
                """));
    }

    private void GenerateStatedCommandBuilderAddHandlerCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models,
        string parameterThisName)
    {
        var model = models[0];
        builder.AddMethodDeclaration($"""
            public static global::DotNetCampus.Cli.StatedCommandRunnerLinkedBuilder<TState> CommandBuilder_AddHandler_{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}<TState, T>(this in global::DotNetCampus.Cli.{parameterThisName}<TState> builder)
            """, m => m
            .WithSummaryComment(
                $$"""<see cref="global::DotNetCampus.Cli.StatedCommandRunnerBuilder{TState}.AddHandler{{{model.CommandObjectType.Name}}}()"/> 方法的拦截器。拦截以提高性能。""")
            .AddAttributes(models.Select(GenerateInterceptsLocationCode))
            .AddTypeConstraints("where T : class, global::DotNetCampus.Cli.ICommandHandler<TState>")
            .AddRawStatement(GenerateComment(model))
            .AddRawStatements($"""
                return builder.AddHandler<T>(global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CommandNameGroup, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CreateInstance);
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
                $$"""<see cref="global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler{T}(global::DotNetCampus.Cli.ICommandRunnerBuilder,global::{{parameterTypeFullName.Replace('<', '{').Replace('>', '}')}})"/> 方法的拦截器。拦截以提高性能。""")
            .AddAttributes(models.Select(GenerateInterceptsLocationCode))
            .AddTypeConstraints("where T : class")
            .AddRawStatement(GenerateComment(model))
            .AddRawStatements($"""
                return global::DotNetCampus.Cli.CommandRunnerBuilderExtensions.AddHandler<T>(builder, handler, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CommandNameGroup, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CreateInstance);
                """));
    }

    private string GenerateComment(InterceptorGeneratingModel model) => $"""
        // 请确保 {model.CommandObjectType.Name} 类型中至少有一个属性标记了 [Option] 或 [Value] 特性；
        // 否则下面的 {model.GetBuilderTypeName()} 类型将不存在，导致编译不通过。
        """;

    /// <summary>
    /// CommandLine.AddHandler
    /// </summary>
    [Obsolete("仅用于从旧版本迁移到新版本时使用")]
    private void ObsoleteAddHandler(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string thisName, string methodSignature)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, (t, x) =>
            GenerateObsoleteAddHandlerCode(t, x, thisName));
        context.AddSource($"CommandLine.Interceptors/{thisName}.{methodSignature}.g.cs", code);
    }

    /// <summary>
    /// CommandLine.AddHandler(Action)
    /// </summary>
    [Obsolete("仅用于从旧版本迁移到新版本时使用")]
    private void ObsoleteAddHandlerAction(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string thisName, string methodSignature, string parameterTypeFullName, string returnName)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, (t, x) =>
            GenerateObsoleteAddHandlerActionCode(t, x, thisName, parameterTypeFullName, returnName));
        context.AddSource($"CommandLine.Interceptors/{thisName}.{methodSignature}.g.cs", code);
    }

    [Obsolete("仅用于从旧版本迁移到新版本时使用")]
    private void GenerateObsoleteAddHandlerCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models,
        string parameterThisName)
    {
        var model = models[0];
        builder.AddMethodDeclaration($"""
            public static global::DotNetCampus.Cli.IAsyncCommandRunnerBuilder CommandLine_AddHandler_{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}<T>(this global::DotNetCampus.Cli.{parameterThisName} commandLine)
            """, m => m
            .AddAttributes(models.Select(GenerateInterceptsLocationCode))
            .AddTypeConstraints("where T : class, global::DotNetCampus.Cli.ICommandHandler")
            .AddRawStatement(GenerateComment(model))
            .AddRawStatements($"""
                return commandLine.ToRunner().AddHandler<T>(global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CommandNameGroup, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CreateInstance);
                """));
    }

    [Obsolete("仅用于从旧版本迁移到新版本时使用")]
    private void GenerateObsoleteAddHandlerActionCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models,
        string parameterThisName, string parameterTypeFullName, string returnName)
    {
        var model = models[0];
        builder.AddMethodDeclaration($"""
            public static global::DotNetCampus.Cli.{returnName} CommandLine_AddHandler_{NamingHelper.MakePascalCase(model.CommandObjectType.ToDisplayString())}<T>(this global::DotNetCampus.Cli.{parameterThisName} commandLine, global::{parameterTypeFullName} handler)
            """, m => m
            .AddAttributes(models.Select(GenerateInterceptsLocationCode))
            .AddTypeConstraints("where T : class")
            .AddRawStatement(GenerateComment(model))
            .AddRawStatements($"""
                return commandLine.ToRunner().AddHandler<T>(handler, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CommandNameGroup, global::{model.CommandObjectType.ContainingNamespace}.{model.GetBuilderTypeName()}.CreateInstance);
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
