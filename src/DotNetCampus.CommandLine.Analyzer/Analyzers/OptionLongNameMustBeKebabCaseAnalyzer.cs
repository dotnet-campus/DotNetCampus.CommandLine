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

            foreach (var (name, location, type) in AnalyzeOptionAttributeArguments(attributeSyntax, hasSeparator))
            {
                if (name is null || location is null)
                {
                    continue;
                }

                var descriptor = type is SuggestionType.Warning
                    ? Diagnostics.DCL101_OptionLongNameMustBeKebabCase
                    : Diagnostics.DCL102_OptionLongNameCanBeKebabCase;
                var diagnostic = Diagnostic.Create(descriptor, location, name);
                context.ReportDiagnostic(diagnostic);
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
    private IEnumerable<(string? Name, Location? Location, SuggestionType SuggestionType)> AnalyzeOptionAttributeArguments(
        AttributeSyntax attributeSyntax, bool hasSeparator)
    {
        var argumentList = attributeSyntax.ChildNodes().OfType<AttributeArgumentListSyntax>().FirstOrDefault();
        if (argumentList == null)
        {
            yield break;
        }

        var attributeArguments = argumentList.ChildNodes().OfType<AttributeArgumentSyntax>().ToList();
        var optionNameArguments = attributeArguments.Where(x => x.NameEquals is null).ToList();
        var longNameExpressions = optionNameArguments.Count switch
        {
            1 => optionNameArguments[0].DescendantNodes().OfType<LiteralExpressionSyntax>().ToList(),
            2 => optionNameArguments[1].DescendantNodes().OfType<LiteralExpressionSyntax>().ToList(),
            _ => [],
        };
        foreach (var longNameExpression in longNameExpressions)
        {
            var longName = longNameExpression?.Token.ValueText;
            var caseSensitiveExpression = attributeArguments
                .FirstOrDefault(x => x.NameEquals?.Name.ToString() == "CaseSensitive")?
                .Expression as LiteralExpressionSyntax;
            var caseSensitive = caseSensitiveExpression?.Token.ValueText.Equals("true", StringComparison.OrdinalIgnoreCase) is true;
            if (!caseSensitive && longName is not null)
            {
                // 严格检查。
                var kebabCase = MakeKebabCase(longName, true, false, hasSeparator);
                var isKebabCase = string.Equals(kebabCase, longName, StringComparison.Ordinal);
                if (!isKebabCase)
                {
                    yield return (longName, longNameExpression?.GetLocation(), SuggestionType.Warning);
                }

                // 宽松检查。
                var kebabCase2 = MakeKebabCase(longName, true, true, hasSeparator);
                var isKebabCase2 = string.Equals(kebabCase2, longName, StringComparison.Ordinal);
                if (!isKebabCase2)
                {
                    yield return (longName, longNameExpression?.GetLocation(), SuggestionType.Info);
                }
            }
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
