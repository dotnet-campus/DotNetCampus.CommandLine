using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.CommandLine.Utils.CodeAnalysis;

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
        var compareName = typeof(TAttribute).Name;
        if (attribute.Name == compareName)
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
