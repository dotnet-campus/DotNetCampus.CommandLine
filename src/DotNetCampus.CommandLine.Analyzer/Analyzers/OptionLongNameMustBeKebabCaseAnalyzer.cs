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
    private readonly ImmutableHashSet<string> _attributeNames = ["DotNetCampus.Cli.Compiler.OptionAttribute"];

    /// <summary>
    /// Supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        Diagnostics.DCL101_OptionLongNameMustBeKebabCase,
        Diagnostics.DCL102_OptionLongNameCanBeKebabCase,
    ];

    /// <summary>
    /// Register property analyzer.
    /// </summary>
    /// <param name="context"></param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    /// <summary>
    /// Find OptionAttribute from a property.
    /// </summary>
    /// <param name="context"></param>
    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyNode = (PropertyDeclarationSyntax)context.Node;

        foreach (var attributeSyntax in propertyNode.AttributeLists.SelectMany(x => x.Attributes))
        {
            var attributeTypeSymbol = context.SemanticModel.GetTypeInfo(attributeSyntax).Type;
            var fullAttributeName = attributeTypeSymbol?.ToDisplayString();
            if (fullAttributeName != null && _attributeNames.Contains(fullAttributeName))
            {
                var (name, location, type) = AnalyzeOptionAttributeArguments(attributeSyntax);
                if (name != null && location != null)
                {
                    var descriptor = type is SuggestionType.Warning
                        ? Diagnostics.DCL101_OptionLongNameMustBeKebabCase
                        : Diagnostics.DCL102_OptionLongNameCanBeKebabCase;
                    var diagnostic = Diagnostic.Create(descriptor, location, name);
                    context.ReportDiagnostic(diagnostic);
                }
                break;
            }
        }
    }

    /// <summary>
    /// Find LongName argument from the OptionAttribute.
    /// </summary>
    /// <param name="attributeSyntax"></param>
    /// <returns>
    /// name: the LongName value.
    /// location: the syntax tree location of the LongName argument value.
    /// </returns>
    private (string? Name, Location? Location, SuggestionType SuggestionType) AnalyzeOptionAttributeArguments(AttributeSyntax attributeSyntax)
    {
        var argumentList = attributeSyntax.ChildNodes().OfType<AttributeArgumentListSyntax>().FirstOrDefault();
        if (argumentList != null)
        {
            var attributeArguments = argumentList.ChildNodes().OfType<AttributeArgumentSyntax>().ToList();
            var longNameExpression = attributeArguments.FirstOrDefault()?.Expression as LiteralExpressionSyntax;
            var longName = longNameExpression?.Token.ValueText;
            var ignoreCaseExpression =
                attributeArguments.FirstOrDefault(x => x.NameEquals?.Name.ToString() == "ExactSpelling")?.Expression as LiteralExpressionSyntax;
            var exactSpelling = ignoreCaseExpression?.Token.ValueText.Equals("true", StringComparison.OrdinalIgnoreCase) is true;
            if (!exactSpelling && longName is not null)
            {
                // 严格检查。
                var kebabCase1 = NamingHelper.MakeKebabCase(longName, true, false);
                var isKebabCase1 = string.Equals(kebabCase1, longName, StringComparison.Ordinal);
                if (!isKebabCase1)
                {
                    return (longName, longNameExpression?.GetLocation(), SuggestionType.Warning);
                }

                // 宽松检查。
                var kebabCase2 = NamingHelper.MakeKebabCase(longName, true, true);
                var isKebabCase2 = string.Equals(kebabCase2, longName, StringComparison.Ordinal);
                if (!isKebabCase2)
                {
                    return (longName, longNameExpression?.GetLocation(), SuggestionType.Hidden);
                }
            }
        }
        return (null, null, SuggestionType.Hidden);
    }

    private enum SuggestionType
    {
        Hidden,
        Warning,
    }
}
