using System.Collections.Immutable;
using System.Composition;
using DotNetCampus.CommandLine.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.CommandLine.CodeFixes.ConvertOptionProperty;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionPropertyTypeToBooleanCodeFix)), Shared]
public class OptionPropertyTypeToBooleanCodeFix : ConvertOptionPropertyTypeCodeFix
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    [
        Diagnostics.SupportedOptionPropertyType,
        Diagnostics.NotSupportedOptionPropertyType,
    ];

    protected sealed override string CodeActionTitle => Localizations.DCL201_202_Fix_OptionTypeToBoolean;

    protected sealed override CompilationUnitSyntax CreateTypeSyntaxNode(
        TypeSyntax oldTypeSyntax, CompilationUnitSyntax syntaxRoot, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return syntaxRoot.ReplaceNode(
            oldTypeSyntax,
            SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(SyntaxKind.BoolKeyword)));
    }
}
