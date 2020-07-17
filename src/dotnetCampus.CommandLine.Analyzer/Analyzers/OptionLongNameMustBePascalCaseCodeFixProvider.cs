using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using dotnetCampus.CommandLine.Properties;
using System.Text;
using dotnetCampus.Cli.Utils;

namespace dotnetCampus.CommandLine.Analyzers
{
    /// <summary>
    /// [Option("LongName")]
    /// The LongName must be PascalCase. If not, this codefix will fix it.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionLongNameMustBePascalCaseCodeFixProvider)), Shared]
    public class OptionLongNameMustBePascalCaseCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.OptionLongNameMustBePascalCase);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            ExpressionSyntax? syntax = root.FindNode(diagnosticSpan) switch
            {
                AttributeArgumentSyntax attributeArgumentSyntax => attributeArgumentSyntax.Expression,
                ExpressionSyntax expressionSyntax => expressionSyntax,
                _ => null,
            };

            if (syntax != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.OptionLongNameMustBePascalCaseFix,
                        createChangedSolution: c => MakePascalCaseAsync(context.Document, syntax, c),
                        equivalenceKey: Resources.OptionLongNameMustBePascalCaseFix),
                    diagnostic);
            }
        }

        private async Task<Solution> MakePascalCaseAsync(Document document, ExpressionSyntax expressionSyntax, CancellationToken cancellationToken)
        {
            var expression = expressionSyntax.ToString();
            var oldName = expression.Substring(1, expression.Length - 2);
            var newName = NamingHelper.MakePascalCase(oldName);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return document.Project.Solution;
            }

            var newRoot = root.ReplaceNode(expressionSyntax,
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(newName)));
            return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newRoot);
        }
    }
}
