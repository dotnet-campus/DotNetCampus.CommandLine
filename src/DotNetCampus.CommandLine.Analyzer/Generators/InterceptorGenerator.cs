using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
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
            if (node is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name: GenericNameSyntax
                        {
                            Identifier: { Text: "As" },
                        },
                    },
                })
            {
                return true;
            }
            return false;
        }, (c, ct) =>
        {
            return c.Node;
        });

        context.RegisterSourceOutput(asProvider.Collect(), Execute);
    }

    private void Execute(SourceProductionContext arg1, ImmutableArray<SyntaxNode> arg2)
    {
    }
}
