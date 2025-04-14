using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.CommandLine.Utils.CodeAnalysis;

public static class AttributeExtensions
{
    public static bool IsAttributeOf<TAttribute>(this AttributeSyntax attribute)
    {
        var codeName = attribute.Name.ToString();
        var compareName = typeof(TAttribute).Name;
        if (codeName == compareName)
        {
            return true;
        }

        if (compareName.EndsWith("Attribute"))
        {
            compareName = compareName.Substring(0, compareName.Length - "Attribute".Length);
            if (codeName == compareName)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAttributeOf<TAttribute>(this INamedTypeSymbol attribute)
    {
        if (attribute.ContainingNamespace.ToString() != typeof(TAttribute).Namespace)
        {
            return false;
        }

        var compareName = typeof(TAttribute).Name;
        if (attribute.Name == typeof(TAttribute).Name)
        {
            return true;
        }

        if (compareName.EndsWith("Attribute"))
        {
            compareName = compareName.Substring(0, compareName.Length - "Attribute".Length);
            if (attribute.Name == compareName)
            {
                return true;
            }
        }

        return false;
    }
}
