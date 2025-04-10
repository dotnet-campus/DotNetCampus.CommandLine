namespace dotnetCampus.Cli;

/// <summary>
/// 在解析命令行参数时，指定命令行参数的解析方式。
/// </summary>
public record CommandLineParsingOptions
{
    /// <summary>
    /// 以此风格解析命令行参数。
    /// </summary>
    /// <remarks>
    /// 不指定时会自动根据用户输入的命令行参数判断风格。
    /// </remarks>
    public CommandLineStyle Style { get; init; }

    /// <summary>
    /// 默认是大小写不敏感的，设置此值为 <see langword="true" /> 可以让命令行参数大小写敏感。
    /// </summary>
    /// <remarks>
    /// 当然，可以在单独的属性上设置大小写敏感，设置后将在那个属性上覆盖此默认值。不设置的属性会使用此默认值。
    /// </remarks>
    public bool CaseSensitive { get; init; }
}
