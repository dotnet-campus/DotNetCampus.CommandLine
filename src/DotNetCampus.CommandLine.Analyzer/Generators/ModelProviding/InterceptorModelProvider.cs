﻿#pragma warning disable RSEXPERIMENTAL002
using System.Text.RegularExpressions;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.CommandLine.Generators.ModelProviding;

internal static class InterceptorModelProvider
{
    public static IncrementalValuesProvider<InterceptorGeneratingModel> SelectCommandLineAsProvider(this IncrementalGeneratorInitializationContext context)
    {
        return SelectMethodInvocationProvider(context,
            "DotNetCampus.Cli.CommandLine", "As");
    }

    public static IncrementalValuesProvider<InterceptorGeneratingModel> SelectCommandBuilderAddHandlerProvider(
        this IncrementalGeneratorInitializationContext context)
    {
        return SelectMethodInvocationProvider(context, "DotNetCampus.Cli.CommandRunnerBuilderExtensions", "AddHandler");
    }

    public static IncrementalValuesProvider<InterceptorGeneratingModel> SelectCommandBuilderAddHandlerProvider(
        this IncrementalGeneratorInitializationContext context, string extensionMethodThisTypeName, string parameterTypeFullName)
    {
        return SelectMethodInvocationProvider(context,
            $"DotNetCampus.Cli.{extensionMethodThisTypeName}", "AddHandler",
            parameterTypeFullName.Replace(".", @"\.").Replace("<T>", @"<[\w_\.:\?]+>").Replace("<T,", @"<[\w_\.:\?]+,"));
    }

    public static IncrementalValuesProvider<InterceptorGeneratingModel> SelectMethodInvocationProvider(this IncrementalGeneratorInitializationContext context,
        string typeFullName, string methodName, params string[] parameterTypeFullNameRegexes)
    {
        return context.SyntaxProvider.CreateSyntaxProvider((node, ct) =>
            {
                // 检查 commandLine.As<T>() 方法调用。
                if (node is InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax
                        {
                            Name: GenericNameSyntax
                            {
                                TypeArgumentList.Arguments.Count: 1,
                            } syntax,
                        },
                    } invocationExpressionNode && syntax.Identifier.Text == methodName)
                {
                    // 再检查方法的参数列表是否是指定类型。
                    var expectedParameterCount = parameterTypeFullNameRegexes.Length;
                    var argumentList = invocationExpressionNode.ArgumentList.Arguments;
                    if (argumentList.Count != expectedParameterCount)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }, (c, ct) =>
            {
                var node = (InvocationExpressionSyntax)c.Node;
                // 确保此方法是 DotNetCampus.Cli.CommandLine.As<T>() 方法（类也要匹配）。
                var methodSymbol = ModelExtensions.GetSymbolInfo(c.SemanticModel, node, ct).Symbol as IMethodSymbol;
                if (methodSymbol is null)
                {
                    // 没有方法。
                    return null;
                }
                if (methodSymbol.ContainingType.ToDisplayString() != typeFullName
                    && methodSymbol.ReceiverType?.ToDisplayString() != typeFullName)
                {
                    // 方法所在的类型不匹配，且扩展方法的 this 参数类型不匹配。
                    return null;
                }
                if (methodSymbol.Parameters.Length != parameterTypeFullNameRegexes.Length)
                {
                    // 参数数量不匹配。
                    return null;
                }
                for (var i = 0; i < parameterTypeFullNameRegexes.Length; i++)
                {
                    var parameterSymbol = methodSymbol.Parameters[i];
                    if (!Regex.IsMatch(parameterSymbol.Type.ToGlobalDisplayString(), parameterTypeFullNameRegexes[i]))
                    {
                        // 参数类型不匹配。
                        return null;
                    }
                }

                // 获取 commandLine.As<T>() 中的 T。
                var genericTypeNode = ((GenericNameSyntax)((MemberAccessExpressionSyntax)node.Expression).Name).TypeArgumentList.Arguments[0];
                var symbol = ModelExtensions.GetSymbolInfo(c.SemanticModel, genericTypeNode, ct).Symbol as INamedTypeSymbol;
                var interceptableLocation = c.SemanticModel.GetInterceptableLocation(node, ct);
                if (interceptableLocation is null || symbol is null)
                {
                    return null;
                }
                // 获取 [Verb("xxx")] 特性中的 xxx。
                var verbName = symbol.GetAttributes()
                    .FirstOrDefault(attribute => attribute.AttributeClass?.IsAttributeOf<VerbAttribute>() is true)?
                    .ConstructorArguments.FirstOrDefault() is { Kind: TypedConstantKind.Primitive } verbArgument
                    ? verbArgument.Value?.ToString()
                    : null;
                // 获取调用代码所在的类和方法。
                var methodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                var classDeclaration = methodDeclaration?.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>();
                var invocationFileName = Path.GetFileName(node.SyntaxTree.FilePath);
                var invocationInfo = $"{classDeclaration?.Identifier.ToString()}.{methodDeclaration?.Identifier.ToString()} @{invocationFileName}";

                return new InterceptorGeneratingModel(interceptableLocation, symbol, verbName, invocationInfo);
            })
            .Where(model => model is not null)
            .Select((model, ct) => model!);
    }
}

internal record InterceptorGeneratingModel(
    InterceptableLocation InterceptableLocation,
    INamedTypeSymbol CommandObjectType,
    string? VerbName,
    string InvocationInfo
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
