using System.Collections.Immutable;
using System.Composition;
using DotNetCampus.CommandLine.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.CommandLine.CodeFixes.ConvertOptionProperty;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionPropertyTypeToStringCodeFix)), Shared]
public class OptionPropertyTypeToStringCodeFix : ConvertOptionPropertyTypeCodeFix
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    [
        Diagnostics.SupportedOptionPropertyType,
        Diagnostics.NotSupportedOptionPropertyType,
    ];

    protected sealed override string CodeActionTitle => Localizations.DCL201_202_Fix_OptionTypeToString;

    protected sealed override CompilationUnitSyntax CreateTypeSyntaxNode(
        TypeSyntax oldTypeSyntax, CompilationUnitSyntax syntaxRoot, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return syntaxRoot.ReplaceNode(
            oldTypeSyntax,
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(SyntaxKind.StringKeyword)));
    }
}
