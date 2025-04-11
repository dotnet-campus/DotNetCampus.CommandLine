using System.Collections.Immutable;

namespace dotnetCampus.Cli;

/// <summary>
/// 在解析命令行参数时，指定命令行参数的解析方式。
/// </summary>
public record CommandLineParsingOptions
{
    /// <inheritdoc cref="CommandLineStyle.Flexible" />
    public static CommandLineParsingOptions Flexible { get; } = new CommandLineParsingOptions
    {
        Style = CommandLineStyle.Flexible,
        CaseSensitive = false,
    };

    /// <inheritdoc cref="CommandLineStyle.GNU" />
    public static CommandLineParsingOptions GNU { get; } = new CommandLineParsingOptions
    {
        Style = CommandLineStyle.GNU,
        CaseSensitive = true,
    };

    /// <inheritdoc cref="CommandLineStyle.POSIX" />
    public static CommandLineParsingOptions POSIX { get; } = new CommandLineParsingOptions
    {
        Style = CommandLineStyle.POSIX,
        CaseSensitive = true,
    };

    /// <inheritdoc cref="CommandLineStyle.DotNet" />
    public static CommandLineParsingOptions DotNet { get; } = new CommandLineParsingOptions
    {
        Style = CommandLineStyle.DotNet,
        CaseSensitive = false,
    };

    /// <inheritdoc cref="CommandLineStyle.PowerShell" />
    public static CommandLineParsingOptions PowerShell { get; } = new CommandLineParsingOptions
    {
        Style = CommandLineStyle.PowerShell,
        CaseSensitive = false,
    };

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

    /// <summary>
    /// 此命令行解析器支持从 Web 打开本地应用时传入的参数。<br/>
    /// 此属性指定用于 URI 协议注册的方案名（scheme name）。
    /// </summary>
    /// <remarks>
    /// 例如：sample://open?url=DotNetCampus%20is%20a%20great%20team<br/>
    /// 这里的 "sample" 就是方案名。<br/>
    /// 当解析命令行参数时，如果只传入了一个参数，且参数开头满足 sample:// 格式时，则会认为方案名匹配，将进行后续 url 的参数解析。
    /// </remarks>
    /// <returns>返回协议方案名，如 "sample"</returns>
    public ImmutableArray<string> SchemeNames { get; init; }
}
