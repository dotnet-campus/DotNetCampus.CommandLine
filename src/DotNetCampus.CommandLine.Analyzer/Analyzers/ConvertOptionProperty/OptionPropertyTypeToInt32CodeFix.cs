using System.Collections.Immutable;
using System.Composition;
using DotNetCampus.CommandLine.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.CommandLine.Analyzers.ConvertOptionProperty;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionPropertyTypeToInt32CodeFix)), Shared]
public class OptionPropertyTypeToInt32CodeFix : ConvertOptionPropertyTypeCodeFix
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    [
        Diagnostics.SupportedOptionPropertyType,
        Diagnostics.NotSupportedOptionPropertyType,
    ];

    protected sealed override string CodeActionTitle => Localizations.DCL201_202_Fix_OptionTypeToInt32;

    protected sealed override CompilationUnitSyntax CreateTypeSyntaxNode(
        TypeSyntax oldTypeSyntax, CompilationUnitSyntax syntaxRoot, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return syntaxRoot.ReplaceNode(
            oldTypeSyntax,
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(SyntaxKind.IntKeyword)));
    }
}
