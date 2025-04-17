#pragma warning disable RSEXPERIMENTAL002
using System.Collections.Immutable;
using DotNetCampus.CommandLine.Generators.ModelProviding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.CommandLine.Generators;

[Generator(LanguageNames.CSharp)]
public class InterceptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var asProvider = context.SyntaxProvider.CreateSyntaxProvider((node, ct) =>
            {
                // 检查 commandLine.As<T>() 方法调用。
                return node is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name: GenericNameSyntax
                        {
                            Identifier.Text: "As",
                            TypeArgumentList.Arguments.Count: 1,
                        },
                    },
                };
            }, (c, ct) =>
            {
                var node = (InvocationExpressionSyntax)c.Node;
                // 确保此方法是 DotNetCampus.Cli.CommandLine.As<T>() 方法（类也要匹配）。
                var methodSymbol = c.SemanticModel.GetSymbolInfo(node, ct).Symbol as IMethodSymbol;
                if (methodSymbol is null || methodSymbol.ContainingType.ToDisplayString() != "DotNetCampus.Cli.CommandLine")
                {
                    return null;
                }
                // 获取 commandLine.As<T>() 中的 T。
                var genericTypeNode = ((GenericNameSyntax)((MemberAccessExpressionSyntax)node.Expression).Name).TypeArgumentList.Arguments[0];
                var symbol = c.SemanticModel.GetSymbolInfo(genericTypeNode, ct).Symbol as INamedTypeSymbol;
                var interceptableLocation = c.SemanticModel.GetInterceptableLocation(node, ct);
                return interceptableLocation is not null && symbol is not null
                    ? new InterceptorGeneratingModel(interceptableLocation, symbol)
                    : null;
            })
            .Where(model => model is not null)
            .Select((model, ct) => model!);

        context.RegisterSourceOutput(asProvider.Collect(), Execute);
    }

    private void Execute(SourceProductionContext context, ImmutableArray<InterceptorGeneratingModel> models)
    {
        var modelGroups = models
            .GroupBy(x => x.CommandObjectType, SymbolEqualityComparer.Default)
            .ToDictionary(
                x => x.Key!,
                x => x.ToImmutableArray(),
                SymbolEqualityComparer.Default);
        var code = GenerateInterceptorCode(modelGroups);
        context.AddSource("CommandLine.Interceptors.g.cs", code);
    }

    private string GenerateInterceptorCode(Dictionary<ISymbol, ImmutableArray<InterceptorGeneratingModel>> models)
    {
        return $$"""
#nullable enable

namespace {{GeneratorInfo.RootNamespace}}
{
    file static class CommandObjectCreatorInterceptors
    {
{{string.Join("\n\n", models.Select(x=>GenerateInterceptorCode(x.Value)))}}
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

    private string GenerateInterceptorCode(ImmutableArray<InterceptorGeneratingModel> models)
    {
        var model = models[0];
        return $$"""
        /// <summary>
        /// <see cref="global::DotNetCampus.Cli.CommandLine.As{{{model.CommandObjectType.Name}}}()"/> 方法的拦截器。拦截以提高性能。
        /// </summary>
{{string.Join("\n", models.Select(GenerateInterceptorCode))}}
        public static T CommandLineAs{{model.CommandObjectType.Name}}<T>(this global::DotNetCampus.Cli.CommandLine commandLine) where T : {{model.CommandObjectType.ToGlobalDisplayString()}}
        {
            return (T)global::{{model.CommandObjectType.ContainingNamespace}}.{{model.GetBuilderTypeName()}}.CreateInstance(commandLine);
        }
""";
    }

    private string GenerateInterceptorCode(InterceptorGeneratingModel model)
    {
        return $"""
        [global::System.Runtime.CompilerServices.InterceptsLocation({model.InterceptableLocation.Version}, "{model.InterceptableLocation.Data}")]
""";
    }
}

internal record InterceptorGeneratingModel(
    InterceptableLocation InterceptableLocation,
    INamedTypeSymbol CommandObjectType
)
{
    public string GetBuilderTypeName() => CommandObjectGeneratingModel.GetBuilderTypeName(CommandObjectType);

    internal static IEqualityComparer<InterceptorGeneratingModel> CommandObjectTypeEqualityComparer { get; } =
        new PrivateTypeSymbolEqualityComparer();

    private sealed class PrivateTypeSymbolEqualityComparer : IEqualityComparer<InterceptorGeneratingModel>
    {
        public bool Equals(InterceptorGeneratingModel? x, InterceptorGeneratingModel? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return SymbolEqualityComparer.Default.Equals(x.CommandObjectType, y.CommandObjectType);
        }

        public int GetHashCode(InterceptorGeneratingModel obj)
        {
            return SymbolEqualityComparer.Default.GetHashCode(obj.CommandObjectType);
        }
    }
}
