using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Utils.CodeAnalysis;

/// <summary>
/// 为 <see cref="ITypeSymbol"/> 提供扩展方法。
/// </summary>
internal static class TypeSymbolExtensions
{
    /// <summary>
    /// 判断一个类型是否是指定类型的子类或实现了指定接口。
    /// </summary>
    /// <param name="type">要判断的类型。</param>
    /// <param name="baseTypeOrInterfaceNames">基类或接口的完整名称（对于泛型类型，不包含泛型参数）。</param>
    /// <param name="trueIfExactMatch">当类型与指定类型完全匹配时是否返回 <see langword="true"/>。</param>
    /// <returns>如果是指定类型的子类或实现了指定接口，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public static bool IsSubclassOrImplementOf(this ITypeSymbol type, IReadOnlyCollection<string> baseTypeOrInterfaceNames, bool trueIfExactMatch = false)
    {
        var typeName = $"{type.ContainingNamespace}.{type.Name}";
        var isExactMatch = baseTypeOrInterfaceNames.Contains(typeName);
        if (isExactMatch)
        {
            return trueIfExactMatch;
        }

        foreach (var baseTypeOrInterfaceName in baseTypeOrInterfaceNames)
        {
            var baseType = type.BaseType;
            while (baseType is not null)
            {
                var name = baseType.ToDisplayString();
                if (name == baseTypeOrInterfaceName || name.StartsWith($"{baseTypeOrInterfaceName}<"))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            foreach (var @interface in type.AllInterfaces)
            {
                var name = @interface.ToDisplayString();
                if (name == baseTypeOrInterfaceName)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
