using System.Collections.Immutable;
using DotNetCampus.Cli.Temp40.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotNetCampus.CommandLine.Temp40.Analyzers;

/// <summary>
/// [Option("LongName")]
/// The LongName must be kebab-case. If not, this analyzer report diagnostics.
/// </summary>
#pragma warning disable RS1001
// [DiagnosticAnalyzer(LanguageNames.CSharp)]
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
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
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
            if (fullAttributeName != null && _attributeNames.Contains(fullAttributeName))
            {
                var (name, location, type) = AnalyzeOptionAttributeArguments(attributeSyntax, hasSeparator);
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
    /// <param name="hasSeparator"></param>
    /// <returns>
    /// name: the LongName value.
    /// location: the syntax tree location of the LongName argument value.
    /// </returns>
    private (string? Name, Location? Location, SuggestionType SuggestionType) AnalyzeOptionAttributeArguments(AttributeSyntax attributeSyntax,
        bool hasSeparator)
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
                var kebabCase1 = MakeKebabCase(longName, true, false, hasSeparator);
                var isKebabCase1 = string.Equals(kebabCase1, longName, StringComparison.Ordinal);
                if (!isKebabCase1)
                {
                    return (longName, longNameExpression?.GetLocation(), SuggestionType.Warning);
                }

                // 宽松检查。
                var kebabCase2 = MakeKebabCase(longName, true, true, hasSeparator);
                var isKebabCase2 = string.Equals(kebabCase2, longName, StringComparison.Ordinal);
                if (!isKebabCase2)
                {
                    return (longName, longNameExpression?.GetLocation(), SuggestionType.Hidden);
                }
            }
        }
        return (null, null, SuggestionType.Hidden);
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
        Hidden,
        Warning,
    }
}
