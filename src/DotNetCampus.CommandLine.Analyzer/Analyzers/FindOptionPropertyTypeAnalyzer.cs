using System.Collections.Immutable;
using DotNetCampus.CommandLine.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotNetCampus.CommandLine.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FindOptionPropertyTypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        Diagnostics.DCL201_SupportedOptionPropertyType,
        Diagnostics.DCL202_NotSupportedOptionPropertyType,
        Diagnostics.DCL203_NotSupportedRawArgumentsPropertyType,
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
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyNode = (PropertyDeclarationSyntax)context.Node;
        var optionTypes = new[]
        {
            context.Compilation.GetTypeByMetadataName("DotNetCampus.Cli.Compiler.OptionAttribute"),
            context.Compilation.GetTypeByMetadataName("DotNetCampus.Cli.Compiler.ValueAttribute"),
        };
        var rawArgumentsTypes = new[]
        {
            context.Compilation.GetTypeByMetadataName("DotNetCampus.Cli.Compiler.RawArgumentsAttribute"),
        };

        foreach (var attributeSyntax in propertyNode.AttributeLists.SelectMany(x => x.Attributes))
        {
            var attributeName = attributeSyntax.Name switch
            {
                IdentifierNameSyntax identifierName => identifierName.ToString(),
                QualifiedNameSyntax qualifiedName => qualifiedName.ChildNodes().OfType<IdentifierNameSyntax>().LastOrDefault()?.ToString(),
                _ => null,
            };

            if (attributeName == null)
            {
                continue;
            }

            var attributeType = context.SemanticModel.GetTypeInfo(attributeSyntax).Type;
            var isOptionAttributeType = optionTypes.Any(x => SymbolEqualityComparer.Default.Equals(x, attributeType));
            var isRawArgumentsAttributeType = rawArgumentsTypes.Any(x => SymbolEqualityComparer.Default.Equals(x, attributeType));

            // [Option], [Value]
            if (isOptionAttributeType)
            {
                var isValidPropertyUsage = AnalyzeOptionPropertyType(context.SemanticModel, propertyNode);
                var diagnostic = CreateDiagnosticForTypeSyntax(
                    context.SemanticModel,
                    isValidPropertyUsage
                        ? Diagnostics.DCL201_SupportedOptionPropertyType
                        : Diagnostics.DCL202_NotSupportedOptionPropertyType,
                    propertyNode);
                context.ReportDiagnostic(diagnostic);
                break;
            }

            // [RawArguments]
            if (isRawArgumentsAttributeType)
            {
                var isValidPropertyUsage = AnalyzeRawArgumentsPropertyType(context.SemanticModel, propertyNode);
                if (!isValidPropertyUsage)
                {
                    var diagnostic = CreateDiagnosticForTypeSyntax(
                        context.SemanticModel,
                        Diagnostics.DCL203_NotSupportedRawArgumentsPropertyType,
                        propertyNode);
                    context.ReportDiagnostic(diagnostic);
                    break;
                }
            }
        }
    }

    private Diagnostic CreateDiagnosticForTypeSyntax(SemanticModel semanticModel, DiagnosticDescriptor rule, PropertyDeclarationSyntax propertySyntax)
    {
        var typeSyntax = propertySyntax.Type is NullableTypeSyntax nullableTypeSyntax
            ? nullableTypeSyntax.ElementType
            : propertySyntax.Type;
        var propertyTypeSymbol = (ITypeSymbol)semanticModel.GetSymbolInfo(propertySyntax.Type).Symbol!;
        return Diagnostic.Create(rule, typeSyntax.GetLocation(), propertyTypeSymbol.GetSymbolInfoAsCommandProperty().GetSimpleName());
    }

    /// <summary>
    /// Check whether the property type is supported by OptionAttribute or ValueAttribute.
    /// </summary>
    /// <param name="semanticModel"></param>
    /// <param name="propertySyntax"></param>
    /// <returns></returns>
    private bool AnalyzeOptionPropertyType(SemanticModel semanticModel, PropertyDeclarationSyntax propertySyntax)
    {
        var propertyTypeSymbol = (ITypeSymbol)semanticModel.GetSymbolInfo(propertySyntax.Type).Symbol!;
        var propertyInfo = propertyTypeSymbol.GetSymbolInfoAsCommandProperty();
        return propertyInfo.Kind is not CommandValueKind.Unknown;
    }

    /// <summary>
    /// Check whether the property type is supported by RawArgumentsAttribute.
    /// </summary>
    /// <param name="semanticModel"></param>
    /// <param name="propertySyntax"></param>
    /// <returns></returns>
    private bool AnalyzeRawArgumentsPropertyType(SemanticModel semanticModel, PropertyDeclarationSyntax propertySyntax)
    {
        var propertyTypeSymbol = (ITypeSymbol)semanticModel.GetSymbolInfo(propertySyntax.Type).Symbol!;
        var propertyInfo = propertyTypeSymbol.GetSymbolInfoAsCommandProperty();
        return propertyInfo.IsAssignableFromArrayOrList();
    }
}
