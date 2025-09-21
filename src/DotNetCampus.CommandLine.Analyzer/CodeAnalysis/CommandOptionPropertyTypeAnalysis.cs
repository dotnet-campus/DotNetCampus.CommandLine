using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.CodeAnalysis;

/// <summary>
/// 辅助分析及生成命令行对象属性所支持的类型。
/// </summary>
internal static class CommandOptionPropertyTypeAnalysis
{
    /// <summary>
    /// 如果 <paramref name="typeSymbol"/> 是可空值类型，则递归返回其基础类型，否则直接返回 <paramref name="typeSymbol"/> 本身。<br/>
    /// 不会处理其泛型参数的可空性。
    /// </summary>
    /// <param name="typeSymbol">要处理的类型符号。</param>
    /// <returns>基础类型符号。</returns>
    public static ITypeSymbol GetNotNullTypeSymbol(this ITypeSymbol typeSymbol) => typeSymbol switch
    {
        INamedTypeSymbol
        {
            IsValueType: true,
            IsGenericType: true,
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
        } nullableTypeSymbol => nullableTypeSymbol.TypeArguments[0],
        _ => typeSymbol,
    };

    /// <summary>
    /// 视 <paramref name="typeSymbol"/> 为命令行属性的类型，按命令行属性的要求获取其所需的类型信息。<br/>
    /// 这个过程会丢掉类型的可空性信息。
    /// </summary>
    /// <param name="typeSymbol">类型符号。</param>
    /// <returns>类型信息。</returns>
    public static CommandPropertyTypeInfo GetSymbolInfoAsCommandProperty(this ITypeSymbol typeSymbol)
    {
        return new CommandPropertyTypeInfo(typeSymbol);
    }
}
