namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 同一个名称的不同命名法表示。
/// </summary>
public readonly record struct NamingPolicyNameGroup
{
    /// <summary>
    /// 创建一个新的命名组。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <param name="pascalCase"></param>
    public NamingPolicyNameGroup(string ordinal, string pascalCase)
    {
        Ordinal = ordinal;
        PascalCase = pascalCase;
    }

    /// <summary>
    /// 原始名称（我们将视之为 kebab-case）。
    /// </summary>
    public string? Ordinal { get; }

    /// <summary>
    /// kebab-case 名称，与 <see cref="Ordinal"/> 相同。
    /// </summary>
    public string? KebabCase => Ordinal;

    /// <summary>
    /// PascalCase 名称，基于 <see cref="Ordinal"/> 转换而来。
    /// </summary>
    public string? PascalCase { get; }
}
