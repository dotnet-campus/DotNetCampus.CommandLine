using System.Collections.Immutable;
using DotNetCampus.Cli.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotNetCampus.CommandLine.Analyzers;

/// <summary>
/// [Option("LongName")]
/// The LongName must be kebab-case. If not, this analyzer report diagnostics.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionLongNameMustBeKebabCaseAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Recognize these attributes.
    /// </summary>
    private readonly ImmutableHashSet<string> _attributeNames =
    [
        "DotNetCampus.Cli.Compiler.OptionAttribute",
        "DotNetCampus.Cli.Compiler.CommandAttribute",
        "DotNetCampus.Cli.Compiler.VerbAttribute",
    ];

    /// <summary>
    /// Supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        Diagnostics.DCL101_OptionLongNameMustBeKebabCase,
        Diagnostics.DCL102_OptionLongNameCanBeKebabCase,
        Diagnostics.DCL103_OptionNameIsInvalid,
    ];

    /// <summary>
    /// Register property analyzer.
    /// </summary>
    /// <param name="context"></param>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeRecord, SyntaxKind.RecordDeclaration);
    }

    /// <summary>
    /// Find CommandAttribute from a property.
    /// </summary>
    /// <param name="context"></param>
    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classNode = (ClassDeclarationSyntax)context.Node;
        AnalyzeAttribute(context, classNode.AttributeLists, true);
    }

    /// <summary>
    /// Find CommandAttribute from a property.
    /// </summary>
    /// <param name="context"></param>
    private void AnalyzeRecord(SyntaxNodeAnalysisContext context)
    {
        var classNode = (RecordDeclarationSyntax)context.Node;
        AnalyzeAttribute(context, classNode.AttributeLists, true);
    }

    /// <summary>
    /// Find OptionAttribute from a property.
    /// </summary>
    /// <param name="context"></param>
    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyNode = (PropertyDeclarationSyntax)context.Node;
        AnalyzeAttribute(context, propertyNode.AttributeLists, false);
    }

    /// <summary>
    /// Find OptionAttribute/CommandAttribute from attributes.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="attributeSyntaxes"></param>
    /// <param name="hasSeparator"></param>
    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context, SyntaxList<AttributeListSyntax> attributeSyntaxes, bool hasSeparator)
    {
        foreach (var attributeSyntax in attributeSyntaxes.SelectMany(x => x.Attributes))
        {
            var attributeTypeSymbol = context.SemanticModel.GetTypeInfo(attributeSyntax).Type;
            var fullAttributeName = attributeTypeSymbol?.ToDisplayString();
            if (fullAttributeName == null || !_attributeNames.Contains(fullAttributeName))
            {
                continue;
            }

            AnalyzeOptionAttributeArguments(context, attributeSyntax, hasSeparator);
        }
    }

    /// <summary>
    /// Find LongName argument from the OptionAttribute.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="attributeSyntax"></param>
    /// <param name="hasSeparator"></param>
    /// <returns>
    /// name: the LongName value.
    /// location: the syntax tree location of the LongName argument value.
    /// </returns>
    private void AnalyzeOptionAttributeArguments(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax, bool hasSeparator)
    {
        var argumentList = attributeSyntax.ChildNodes().OfType<AttributeArgumentListSyntax>().FirstOrDefault();
        if (argumentList == null)
        {
            return;
        }

        var attributeArguments = argumentList.ChildNodes().OfType<AttributeArgumentSyntax>().ToList();
        var optionNameArguments = attributeArguments.Where(x => x.NameEquals is null).ToList();
        List<ExpressionSyntax> shortNameExpressions = optionNameArguments.Count switch
        {
            2 =>
            [
                ..optionNameArguments[0].DescendantNodes().OfType<LiteralExpressionSyntax>(),
                ..optionNameArguments[0].DescendantNodes().OfType<InvocationExpressionSyntax>(),
            ],
            _ => [],
        };
        List<ExpressionSyntax> longNameExpressions = optionNameArguments.Count switch
        {
            1 =>
            [
                ..optionNameArguments[0].DescendantNodes().OfType<LiteralExpressionSyntax>(),
                ..optionNameArguments[0].DescendantNodes().OfType<InvocationExpressionSyntax>(),
            ],
            2 =>
            [
                ..optionNameArguments[1].DescendantNodes().OfType<LiteralExpressionSyntax>(),
                ..optionNameArguments[1].DescendantNodes().OfType<InvocationExpressionSyntax>(),
            ],
            _ => [],
        };
        foreach (var shortNameExpression in shortNameExpressions)
        {
            var shortName = shortNameExpression switch
            {
                LiteralExpressionSyntax le => le.Token.ValueText,
                InvocationExpressionSyntax ie => ie.DescendantNodes().OfType<ArgumentSyntax>()
                    .Select(x => x.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault()?.Identifier.ToString()).FirstOrDefault(),
                _ => null,
            };
            if (shortName is null)
            {
                continue;
            }
            if (CheckIsInvalidOptionName(shortName, hasSeparator))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.DCL103_OptionNameIsInvalid,
                    shortNameExpression?.GetLocation(), shortName));
            }
        }
        foreach (var longNameExpression in longNameExpressions)
        {
            var longName = longNameExpression switch
            {
                LiteralExpressionSyntax le => le.Token.ValueText,
                InvocationExpressionSyntax ie => ie.DescendantNodes().OfType<ArgumentSyntax>()
                    .Select(x => x.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault()?.Identifier.ToString()).FirstOrDefault(),
                _ => null,
            };
            if (longName is null)
            {
                continue;
            }
            if (CheckIsInvalidOptionName(longName, hasSeparator))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.DCL103_OptionNameIsInvalid,
                    longNameExpression?.GetLocation(), longName));
            }
            var caseSensitiveExpression = attributeArguments
                .FirstOrDefault(x => x.NameEquals?.Name.ToString() == "CaseSensitive")?
                .Expression as LiteralExpressionSyntax;
            var caseSensitive = caseSensitiveExpression?.Token.ValueText.Equals("true", StringComparison.OrdinalIgnoreCase) is true;
            if (!caseSensitive)
            {
                // 严格检查。
                var kebabCase = MakeKebabCase(longName, true, false, hasSeparator);
                var isKebabCase = string.Equals(kebabCase, longName, StringComparison.Ordinal);
                if (!isKebabCase)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.DCL101_OptionLongNameMustBeKebabCase,
                        longNameExpression?.GetLocation(), longName));
                }
                else
                {
                    // 宽松检查。
                    var kebabCase2 = MakeKebabCase(longName, true, true, hasSeparator);
                    var isKebabCase2 = string.Equals(kebabCase2, longName, StringComparison.Ordinal);
                    if (!isKebabCase2)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Diagnostics.DCL102_OptionLongNameCanBeKebabCase,
                            longNameExpression?.GetLocation(), longName));
                    }
                }
            }
        }
    }

    private bool CheckIsInvalidOptionName(string optionName, bool hasSeparator)
    {
        if (hasSeparator)
        {
            return optionName.Split([' '], StringSplitOptions.RemoveEmptyEntries)
                .Any(CheckIsInvalidOptionNameCore);
        }
        else
        {
            return CheckIsInvalidOptionNameCore(optionName);
        }

        static bool CheckIsInvalidOptionNameCore(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            var span = name.AsSpan();
            for (var i = 0; i < span.Length; i++)
            {
                var c = span[i];
                if (i is 0 && c is '-')
                {
                    return true;
                }
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                {
                    return true;
                }
            }

            return false;
        }
    }

    private string MakeKebabCase(string oldName, bool isUpperSeparator, bool toLower, bool hasSeparator)
    {
        if (hasSeparator)
        {
            return string.Join(" ", oldName.Split([' '], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => NamingHelper.MakeKebabCase(x, isUpperSeparator, toLower)));
        }
        else
        {
            return NamingHelper.MakeKebabCase(oldName, isUpperSeparator, toLower);
        }
    }

    private enum SuggestionType
    {
        Info,
        Warning,
    }
}
