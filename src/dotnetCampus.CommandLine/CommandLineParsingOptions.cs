namespace dotnetCampus.Cli;

/// <summary>
/// 在解析命令行参数时，指定命令行参数的解析方式。
/// </summary>
public record CommandLineParsingOptions
{
    /// <summary>
    /// 以此风格解析命令行参数。
    /// </summary>
    public CommandLineStyle Style { get; init; }

    /// <summary>
    /// 在不特别指定某个选项大小写敏感的情况下，全局忽略大小写。
    /// </summary>
    public bool IgnoreCase { get; init; }
}
