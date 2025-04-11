using System.Collections.Immutable;
using dotnetCampus.Cli.Utils;
using dotnetCampus.CommandLine.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace dotnetCampus.CommandLine.Analyzers;

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
    private readonly IList<string> _attributeNames = new List<string> { "Option", "OptionAttribute" };

    /// <summary>
    /// Supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        Diagnostics.DCL101_OptionLongNameMustBeKebabCase,
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
            string? attributeName = attributeSyntax.Name switch
            {
                IdentifierNameSyntax identifierName => identifierName.ToString(),
                QualifiedNameSyntax qualifiedName => qualifiedName.ChildNodes().OfType<IdentifierNameSyntax>().LastOrDefault()?.ToString(),
                _ => null,
            };

            if (attributeName != null && _attributeNames.Contains(attributeName))
            {
                var (name, location) = AnalyzeOptionAttributeArguments(attributeSyntax);
                if (name != null && location != null)
                {
                    var diagnostic = Diagnostic.Create(Diagnostics.DCL101_OptionLongNameMustBeKebabCase, location, name);
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
    private (string? name, Location? location) AnalyzeOptionAttributeArguments(AttributeSyntax attributeSyntax)
    {
        var argumentList = attributeSyntax.ChildNodes().OfType<AttributeArgumentListSyntax>().FirstOrDefault();
        if (argumentList != null)
        {
            var attributeArguments = argumentList.ChildNodes().OfType<AttributeArgumentSyntax>().ToList();
            var longNameExpression = attributeArguments.FirstOrDefault()?.Expression as LiteralExpressionSyntax;
            var longName = longNameExpression?.Token.ValueText;
            var ignoreCaseExpression =
                attributeArguments.FirstOrDefault(x => x.NameEquals?.Name.ToString() == "IgnoreCase")?.Expression as LiteralExpressionSyntax;
            var ignoreCase = ignoreCaseExpression?.Token.ValueText.Equals("true", StringComparison.OrdinalIgnoreCase) is true;
            if (longName is not null)
            {
                var kebabCase = NamingHelper.MakeKebabCase(longName, ignoreCase, ignoreCase);
                var isKebabCase = string.Equals(kebabCase, longName, StringComparison.Ordinal);
                if (!isKebabCase)
                {
                    return (longName, longNameExpression?.GetLocation());
                }
            }
        }
        return (null, null);
    }
}
