namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 提供一些扩展方法，辅助命令行解析器进行命令行解析。
/// </summary>
public static class CommandLineParsingExtensions
{
    /// <summary>
    /// 此命名风格是否支持 kebab-case 命名法。
    /// </summary>
    /// <param name="namingPolicy">命名风格。</param>
    /// <returns>如果支持 kebab-case 命名法，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    /// <remarks>
    /// 由于我们已经约定在定义属性时，属性已经用 kebab-case 命名风格标记了名字，所以此选项实际上就是在判断是否使用定义的原样字符串。
    /// </remarks>
    public static bool SupportsOrdinal(this CommandNamingPolicy namingPolicy)
    {
        return namingPolicy switch
        {
            CommandNamingPolicy.KebabCase => true,
            CommandNamingPolicy.Both => true,
            CommandNamingPolicy.Ordinal => true,
            _ => false,
        };
    }

    /// <summary>
    /// 此命名风格是否支持 PascalCase/camelCase 命名法。
    /// </summary>
    /// <param name="namingPolicy">命名风格。</param>
    /// <returns>如果支持 PascalCase/camelCase 命名法，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    public static bool SupportsCamelCase(this CommandNamingPolicy namingPolicy)
    {
        return namingPolicy switch
        {
            CommandNamingPolicy.CamelCase => true,
            CommandNamingPolicy.Both => true,
            _ => false,
        };
    }
}
