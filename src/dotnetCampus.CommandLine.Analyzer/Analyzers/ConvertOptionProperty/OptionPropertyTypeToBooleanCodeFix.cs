﻿using System.Collections.Immutable;
using System.Composition;
using System.Threading;

using dotnetCampus.CommandLine.Properties;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.CommandLine.Analyzers.ConvertOptionProperty
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptionPropertyTypeToBooleanCodeFix)), Shared]
    public class OptionPropertyTypeToBooleanCodeFix : ConvertOptionPropertyTypeCodeFix
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            DiagnosticIds.SupportedOptionPropertyType,
            DiagnosticIds.NotSupportedOptionPropertyType);

        protected sealed override string CodeActionTitle => Resources.ConvertOptionPropertyTypeToBooleanFix;

        protected sealed override SyntaxNode CreateTypeSyntaxNode(
            TypeSyntax oldTypeSyntax, CompilationUnitSyntax syntaxRoot, SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            return SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(SyntaxKind.BoolKeyword));
        }
    }
}
