using System.Collections.Immutable;
using System.Composition;
using DotNetCampus.Cli.Temp40.Utils;
using DotNetCampus.CommandLine.Temp40.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.CommandLine.Temp40.CodeFixes;

/// <summary>
/// [Option("LongName")]
/// The LongName must be kebab-case. If not, this codefix will fix it.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionLongNameMustBeKebabCaseCodeFixProvider)), Shared]
public class OptionLongNameMustBeKebabCaseCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    [
        Diagnostics.OptionLongNameMustBeKebabCase,
        Diagnostics.OptionLongNameCanBeKebabCase,
    ];

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
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
        // 判断此 syntax 是属于 CommandAttribute 还是 OptionAttribute。
        var attributeSyntax = syntax?.FirstAncestorOrSelf<AttributeSyntax>();
        var name = attributeSyntax?.Name.ToString() ?? "";
        var hasSeparator = name.EndsWith("Command") || name.EndsWith("CommandAttribute") || name.EndsWith("Verb") || name.EndsWith("VerbAttribute");

        if (syntax != null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Localizations.DCL101_Fix1,
                    createChangedSolution: c => MakeKebabCaseAsync(context.Document, syntax, hasSeparator, c),
                    equivalenceKey: Localizations.DCL101_Fix1),
                diagnostic);
        }
    }

    private async Task<Solution> MakeKebabCaseAsync(Document document, ExpressionSyntax expressionSyntax,
        bool hasSeparator, CancellationToken cancellationToken)
    {
        var expression = expressionSyntax.ToString();
        // 去掉引号。
        var oldName = expression.Substring(1, expression.Length - 2);
        var newName = MakeKebabCase(oldName, hasSeparator);

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

    private string MakeKebabCase(string oldName, bool hasSeparator)
    {
        if (hasSeparator)
        {
            return string.Join(" ", oldName.Split([' '], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => NamingHelper.MakeKebabCase(x)));
        }
        else
        {
            return NamingHelper.MakeKebabCase(oldName);
        }
    }
}
