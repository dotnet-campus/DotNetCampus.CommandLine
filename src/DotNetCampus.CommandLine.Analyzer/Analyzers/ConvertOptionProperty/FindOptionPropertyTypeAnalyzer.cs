using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotNetCampus.CommandLine.Analyzers.ConvertOptionProperty;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FindOptionPropertyTypeAnalyzer : DiagnosticAnalyzer
{
    private readonly ImmutableHashSet<string> _nonGenericTypeNames =
    [
        "String", "string", "Boolean", "bool", "Byte", "byte", "Int16", "short", "UInt16", "ushort", "Int32", "int", "UInt32", "uint", "Int64", "long",
        "UInt64", "ulong", "Single", "float", "Double", "double", "Decimal", "decimal", "IList", "ICollection", "IEnumerable",
    ];

    private readonly ImmutableHashSet<string> _oneGenericTypeNames =
    [
        "[]", "ImmutableArray", "List", "IList", "IReadOnlyList", "ImmutableHashSet", "Collection", "ICollection", "IReadOnlyCollection", "IEnumerable",
    ];

    private readonly ImmutableHashSet<string> _twoGenericTypeNames = ["ImmutableDictionary", "Dictionary", "IDictionary", "IReadOnlyDictionary", "KeyValuePair"];
    private readonly ImmutableHashSet<string> _genericKeyArgumentTypeNames = ["String", "string"];
    private readonly ImmutableHashSet<string> _genericArgumentTypeNames = ["String", "string"];

    /// <summary>
    /// Supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        Diagnostics.DCL201_SupportedOptionPropertyType,
        Diagnostics.DCL202_NotSupportedOptionPropertyType,
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

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyNode = (PropertyDeclarationSyntax)context.Node;
        var optionTypes = new[]
        {
            context.Compilation.GetTypeByMetadataName("DotNetCampus.Cli.Compiler.OptionAttribute"),
            context.Compilation.GetTypeByMetadataName("DotNetCampus.Cli.Compiler.ValueAttribute"),
        };

        foreach (var attributeSyntax in propertyNode.AttributeLists.SelectMany(x => x.Attributes))
        {
            string? attributeName = attributeSyntax.Name switch
            {
                IdentifierNameSyntax identifierName => identifierName.ToString(),
                QualifiedNameSyntax qualifiedName => qualifiedName.ChildNodes().OfType<IdentifierNameSyntax>().LastOrDefault()?.ToString(),
                _ => null,
            };

            if (attributeName != null)
            {
                var attributeType = context.SemanticModel.GetTypeInfo(attributeSyntax).Type;
                var isTheAttributeType = optionTypes.Any(x => SymbolEqualityComparer.Default.Equals(x, attributeType));
                if (isTheAttributeType)
                {
                    var isValidPropertyUsage = AnalyzeOptionPropertyType(context.SemanticModel, propertyNode);
                    var diagnostic = CreateDiagnosticForTypeSyntax(
                        isValidPropertyUsage
                            ? Diagnostics.DCL201_SupportedOptionPropertyType
                            : Diagnostics.DCL202_NotSupportedOptionPropertyType,
                        propertyNode);
                    context.ReportDiagnostic(diagnostic);
                    break;
                }
            }
        }
    }

    private Diagnostic CreateDiagnosticForTypeSyntax(DiagnosticDescriptor rule, PropertyDeclarationSyntax propertySyntax)
    {
        var typeSyntax = propertySyntax.Type;
        if (typeSyntax is NullableTypeSyntax nullableTypeSyntax)
        {
            // string?
            typeSyntax = nullableTypeSyntax.ElementType;
        }
        string typeName = GetTypeName(typeSyntax);

        return Diagnostic.Create(rule, typeSyntax.GetLocation(), typeName);
    }

    /// <summary>
    /// Find LongName argument from the OptionAttribute.
    /// </summary>
    /// <param name="semanticModel"></param>
    /// <param name="propertySyntax"></param>
    /// <returns>
    /// typeName: the LongName value.
    /// location: the syntax tree location of the LongName argument value.
    /// </returns>
    private bool AnalyzeOptionPropertyType(SemanticModel semanticModel, PropertyDeclarationSyntax propertySyntax)
    {
        var propertyTypeSyntax = propertySyntax.Type;
        string typeName = GetTypeName(propertyTypeSyntax);
        var (genericType0, genericType1) = GetGenericTypeNames(propertyTypeSyntax);

        if (IsTwoGenericType(typeName)
            && genericType0 != null && genericType1 != null
            && IsGenericKeyArgumentType(genericType0)
            && IsGenericArgumentType(genericType1))
        {
            return true;
        }

        if (IsOneGenericType(typeName)
            && genericType0 != null
            && IsGenericArgumentType(genericType0))
        {
            return true;
        }

        if (IsNonGenericType(typeName))
        {
            return true;
        }

        if (propertyTypeSyntax is NullableTypeSyntax nullableTypeSyntax
            && semanticModel.GetSymbolInfo(nullableTypeSyntax.ElementType).Symbol is INamedTypeSymbol { TypeKind: TypeKind.Enum })
        {
            // Enum?
            return true;
        }

        if (semanticModel.GetSymbolInfo(propertyTypeSyntax).Symbol is INamedTypeSymbol { TypeKind: TypeKind.Enum })
        {
            // Enum
            return true;
        }

        return false;
    }

    private string GetTypeName(TypeSyntax typeSyntax)
    {
        if (typeSyntax is NullableTypeSyntax nullableTypeSyntax)
        {
            // string?
            typeSyntax = nullableTypeSyntax.ElementType;
        }

        if (typeSyntax is GenericNameSyntax genericNameSyntax)
        {
            // List<string>
            // Dictionary<string, string>
            return genericNameSyntax.Identifier.ToString();
        }

        if (typeSyntax is ArrayTypeSyntax)
        {
            // string[]
            return "[]";
        }

        if (typeSyntax is PredefinedTypeSyntax predefinedTypeSyntax)
        {
            // string
            return predefinedTypeSyntax.ToString();
        }

        if (typeSyntax is QualifiedNameSyntax qualifiedNameSyntax)
        {
            // System.String
            return qualifiedNameSyntax.ChildNodes().OfType<IdentifierNameSyntax>().Last().ToString();
        }

        // String
        return typeSyntax.ToString();
    }

    private (string?, string?) GetGenericTypeNames(TypeSyntax typeSyntax)
    {
        if (typeSyntax is NullableTypeSyntax nullableTypeSyntax)
        {
            // string?
            typeSyntax = nullableTypeSyntax.ElementType;
        }

        string? genericType0 = null, genericType1 = null;
        if (typeSyntax is GenericNameSyntax genericNameSyntax)
        {
            var genericTypes = genericNameSyntax.TypeArgumentList.ChildNodes().OfType<TypeSyntax>().ToList();
            genericType0 = GetTypeName(genericTypes[0]);
            if (genericTypes.Count == 2)
            {
                genericType1 = GetTypeName(genericTypes[1]);
            }
            else if (genericTypes.Count > 2)
            {
                genericType0 = null;
                genericType1 = null;
            }
        }
        else if (typeSyntax is ArrayTypeSyntax arrayTypeSyntax)
        {
            genericType0 = GetTypeName(arrayTypeSyntax.ElementType);
        }
        return (genericType0, genericType1);
    }

    private bool IsNonGenericType(string typeName)
        => _nonGenericTypeNames.Contains(typeName, StringComparer.Ordinal);

    private bool IsOneGenericType(string typeName)
        => _oneGenericTypeNames.Contains(typeName, StringComparer.Ordinal);

    private bool IsTwoGenericType(string typeName)
        => _twoGenericTypeNames.Contains(typeName, StringComparer.Ordinal);

    private bool IsGenericKeyArgumentType(string typeName)
        => _genericKeyArgumentTypeNames.Contains(typeName, StringComparer.Ordinal);

    private bool IsGenericArgumentType(string typeName)
        => _genericArgumentTypeNames.Contains(typeName, StringComparer.Ordinal);
}
