using System.Collections.Immutable;
using System.Composition;
using dotnetCampus.CommandLine.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace dotnetCampus.CommandLine.Analyzers.ConvertOptionProperty;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionPropertyTypeToDictionaryCodeFix)), Shared]
public class OptionPropertyTypeToDictionaryCodeFix : ConvertOptionPropertyTypeCodeFix
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    [
        Diagnostics.SupportedOptionPropertyType,
        Diagnostics.NotSupportedOptionPropertyType,
    ];

    protected sealed override string CodeActionTitle => Localizations.DCL201_202_Fix_OptionTypeToDictionary;

    protected sealed override CompilationUnitSyntax CreateTypeSyntaxNode(
        TypeSyntax oldTypeSyntax, CompilationUnitSyntax syntaxRoot, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return syntaxRoot.ReplaceNode(
            oldTypeSyntax,
            SyntaxFactory.ParseName("global::System.Collections.Generic.IReadOnlyDictionary<string, string>")
                .WithAdditionalAnnotations(Simplifier.Annotation));
    }
}
