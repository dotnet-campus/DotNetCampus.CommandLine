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
        var ac = context.AnalyzerConfigOptionsProvider;

        // CommandLine.As
        var ca = context.SelectCommandLineAsProvider();

        // CommandLine.AddHandler
        var c0 = context.SelectAddHandlers("CommandLine");
        var c1 = context.SelectAddHandlers("CommandLine", "global::System.Action<T>");
        var c2 = context.SelectAddHandlers("CommandLine", "global::System.Func<T, int>");
        var c3 = context.SelectAddHandlers("CommandLine", "global::System.Func<T, global::System.Threading.Tasks.Task>");
        var c4 = context.SelectAddHandlers("CommandLine", "global::System.Func<T, global::System.Threading.Tasks.Task<int>>");

        // ICommandRunnerBuilder.AddHandler
        var s0 = context.SelectAddHandlers("ICommandRunnerBuilder");
        var s1 = context.SelectAddHandlers("ICommandRunnerBuilder", "global::System.Action<T>");
        var s2 = context.SelectAddHandlers("ICommandRunnerBuilder", "global::System.Func<T, int>");
        var s3 = context.SelectAddHandlers("ICommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task>");
        var s4 = context.SelectAddHandlers("ICommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task<int>>");

        // IAsyncCommandRunnerBuilder.AddHandler
        var a0 = context.SelectAddHandlers("IAsyncCommandRunnerBuilder");
        var a1 = context.SelectAddHandlers("IAsyncCommandRunnerBuilder", "global::System.Action<T>");
        var a2 = context.SelectAddHandlers("IAsyncCommandRunnerBuilder", "global::System.Func<T, int>");
        var a3 = context.SelectAddHandlers("IAsyncCommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task>");
        var a4 = context.SelectAddHandlers("IAsyncCommandRunnerBuilder", "global::System.Func<T, global::System.Threading.Tasks.Task<int>>");

        // StatedCommandRunnerBuilder<TState>.AddHandler
        var t0 = context.SelectAddHandlers("StatedCommandRunnerBuilder");
        var t1 = context.SelectAddHandlers("StatedCommandRunnerLinkedBuilder");

        // CommandLine.As
        context.RegisterSourceOutput(ca.Collect().Combine(ac), CommandLineAs);

        // CommandLine.AddHandler
        context.RegisterSourceOutput(c0.Collect().Combine(ac), (c, args) => CommandLineAddHandler(c, args,
            "CommandLine", "AddHandler()"));
        context.RegisterSourceOutput(c1.Collect().Combine(ac), (c, args) => CommandLineAddHandlerAction(c, args,
            "CommandLine", "AddHandler(Action{T})", "System.Action<T>", "ICommandRunnerBuilder"));
        context.RegisterSourceOutput(c2.Collect().Combine(ac), (c, args) => CommandLineAddHandlerAction(c, args,
            "CommandLine", "AddHandler(Func{T,int})", "System.Func<T, int>", "ICommandRunnerBuilder"));
        context.RegisterSourceOutput(c3.Collect().Combine(ac), (c, args) => CommandLineAddHandlerAction(c, args,
            "CommandLine", "AddHandler(Func{T,Task})", "System.Func<T, System.Threading.Tasks.Task>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(c4.Collect().Combine(ac), (c, args) => CommandLineAddHandlerAction(c, args,
            "CommandLine", "AddHandler(Func{T,Task{int}})", "System.Func<T, System.Threading.Tasks.Task<int>>", "IAsyncCommandRunnerBuilder"));

        // ICommandRunnerBuilder.AddHandler
        context.RegisterSourceOutput(s0.Collect().Combine(ac), (c, args) => CommandRunnerAddHandler(c, args,
            "ICommandRunnerBuilder", "AddHandler()"));
        context.RegisterSourceOutput(s1.Collect().Combine(ac), (c, args) => CommandRunnerAddHandlerAction(c, args,
            "ICommandRunnerBuilder", "AddHandler(Action{T})", "System.Action<T>", "ICommandRunnerBuilder"));
        context.RegisterSourceOutput(s2.Collect().Combine(ac), (c, args) => CommandRunnerAddHandlerAction(c, args,
            "ICommandRunnerBuilder", "AddHandler(Func{T,int})", "System.Func<T, int>", "ICommandRunnerBuilder"));
        context.RegisterSourceOutput(s3.Collect().Combine(ac), (c, args) => CommandRunnerAddHandlerAction(c, args,
            "ICommandRunnerBuilder", "AddHandler(Func{T,Task})", "System.Func<T, System.Threading.Tasks.Task>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(s4.Collect().Combine(ac), (c, args) => CommandRunnerAddHandlerAction(c, args,
            "ICommandRunnerBuilder", "AddHandler(Func{T,Task{int}})", "System.Func<T, System.Threading.Tasks.Task<int>>", "IAsyncCommandRunnerBuilder"));

        // IAsyncCommandRunnerBuilder.AddHandler
        context.RegisterSourceOutput(a0.Collect().Combine(ac), (c, args) => CommandRunnerAddHandler(c, args,
            "IAsyncCommandRunnerBuilder", "AddHandler()"));
        context.RegisterSourceOutput(a1.Collect().Combine(ac), (c, args) => CommandRunnerAddHandlerAction(c, args,
            "IAsyncCommandRunnerBuilder", "AddHandler(Action{T})", "System.Action<T>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(a2.Collect().Combine(ac), (c, args) => CommandRunnerAddHandlerAction(c, args,
            "IAsyncCommandRunnerBuilder", "AddHandler(Func{T,int})", "System.Func<T, int>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(a3.Collect().Combine(ac), (c, args) => CommandRunnerAddHandlerAction(c, args,
            "IAsyncCommandRunnerBuilder", "AddHandler(Func{T,Task})", "System.Func<T, System.Threading.Tasks.Task>", "IAsyncCommandRunnerBuilder"));
        context.RegisterSourceOutput(a4.Collect().Combine(ac), (c, args) => CommandRunnerAddHandlerAction(c, args,
            "IAsyncCommandRunnerBuilder", "AddHandler(Func{T,Task{int}})", "System.Func<T, System.Threading.Tasks.Task<int>>", "IAsyncCommandRunnerBuilder"));

        // StatedCommandRunnerBuilder<TState>.AddHandler
        context.RegisterSourceOutput(t0.Collect().Combine(ac), (c, args) => StatedCommandRunnerAddHandler(c, args,
            "StatedCommandRunnerBuilder", "AddHandler()"));
        context.RegisterSourceOutput(t1.Collect().Combine(ac), (c, args) => StatedCommandRunnerAddHandler(c, args,
            "StatedCommandRunnerLinkedBuilder", "AddHandler()"));
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

    #region CommandLine.As

    /// <summary>
    /// CommandLine.AddHandler
    /// </summary>
    private void CommandLineAddHandler(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string thisName, string methodSignature)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, (t, x) =>
            GenerateCommandLineAddHandlerCode(t, x, thisName));
        context.AddSource($"CommandLine.Interceptors/{thisName}.{methodSignature}.g.cs", code);
    }

    /// <summary>
    /// CommandLine.AddHandler(Action)
    /// </summary>
    private void CommandLineAddHandlerAction(SourceProductionContext context,
        (ImmutableArray<InterceptorGeneratingModel> Left, AnalyzerConfigOptionsProvider Right) args,
        string thisName, string methodSignature, string parameterTypeFullName, string returnName)
    {
        if (context.ToDictionary(args) is not { } modelGroups || modelGroups.Count is 0)
        {
            return;
        }

        var code = GenerateCode(modelGroups, (t, x) =>
            GenerateCommandLineAddHandlerActionCode(t, x, thisName, parameterTypeFullName, returnName));
        context.AddSource($"CommandLine.Interceptors/{thisName}.{methodSignature}.g.cs", code);
    }

    private void GenerateCommandLineAddHandlerCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models,
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

    private void GenerateCommandLineAddHandlerActionCode(TypeDeclarationSourceTextBuilder builder, IReadOnlyList<InterceptorGeneratingModel> models,
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

    #endregion

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
