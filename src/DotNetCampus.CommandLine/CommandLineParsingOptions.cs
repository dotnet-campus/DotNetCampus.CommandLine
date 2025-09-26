namespace DotNetCampus.Cli;

/// <summary>
/// 在解析命令行参数时，指定命令行参数的解析方式。
/// </summary>
public readonly record struct CommandLineParsingOptions
{
    /// <inheritdoc cref="CommandLineStyle.Flexible" />
    public static CommandLineParsingOptions Flexible => new() { Style = CommandLineStyle.Flexible };

    /// <inheritdoc cref="CommandLineStyle.DotNet" />
    public static CommandLineParsingOptions DotNet => new() { Style = CommandLineStyle.DotNet };

    /// <inheritdoc cref="CommandLineStyle.Gnu" />
    public static CommandLineParsingOptions Gnu => new() { Style = CommandLineStyle.Gnu };

    /// <inheritdoc cref="CommandLineStyle.Posix" />
    public static CommandLineParsingOptions Posix => new() { Style = CommandLineStyle.Posix };

    /// <inheritdoc cref="CommandLineStyle.PowerShell" />
    public static CommandLineParsingOptions PowerShell => new() { Style = CommandLineStyle.PowerShell };

    /// <summary>
    /// 详细设置命令行解析时的各种细节。
    /// </summary>
    public CommandLineStyle Style { get; init; }

    /// <summary>
    /// 指定在解析命令行参数时，遇到无法识别的参数时的处理方式。
    /// </summary>
    public UnknownCommandArgumentHandling UnknownArgumentsHandling { get; init; }

    /// <summary>
    /// 此命令行解析器支持从 Web 打开本地应用时传入的参数。<br/>
    /// 此属性指定用于 URI 协议注册的方案名（scheme name）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 例如：sample://open?url=DotNetCampus%20is%20a%20great%20team<br/>
    /// 这里的 "sample" 就是方案名。<br/>
    /// 当解析命令行参数时，如果只传入了一个参数，且参数开头满足 sample:// 格式时，则会认为方案名匹配，将进行后续 url 的参数解析。设置此属性后，无论选择哪种命令行风格(<see cref="Style"/>)，都会优先识别并解析URL格式的参数。
    /// </para>
    /// <para>
    /// <list type="number">
    /// <item>完整格式为 [scheme://][path1/path2][?option1=value1&amp;option2=value2]</item>
    /// <item>整个解析过程不区分大小写</item>
    /// <item>scheme 为方案名，根据传入的命令行命名法进行匹配</item>
    /// <item>path1, path2 等路径会被视为命令和位置参数，具体是命令还是位置参数，跟普通命令行一样，优先匹配命令，剩下的全是位置参数</item>
    /// <item>option1, option2 等参数会被视为选项，只支持长选项；选项名根据传入的命令行命名法进行匹配</item>
    /// <item>提取命令、位置参数、选项名和值时，会根据 URL 编码规则进行解码</item>
    /// <item>支持布尔选项（无值选项），视为 true</item>
    /// </list>
    /// </para>
    /// </remarks>
    public IReadOnlyList<string>? SchemeNames { get; init; }
}

/// <summary>
/// 指定在解析命令行参数时，遇到无法识别的参数时的处理方式。
/// </summary>
public enum UnknownCommandArgumentHandling : byte
{
    /// <summary>
    /// 所有参数都必须被识别，否则进入到回退处理逻辑。当然，就算在回退处理逻辑里面也可以继续忽略未识别的参数。
    /// </summary>
    AllArgumentsMustBeRecognized,

    /// <summary>
    /// 忽略未识别的选项。
    /// </summary>
    IgnoreUnknownOptionalArguments,

    /// <summary>
    /// 忽略未识别的位置参数。
    /// </summary>
    IgnoreUnknownPositionalArguments,

    /// <summary>
    /// 忽略所有未识别的参数。
    /// </summary>
    IgnoreAllUnknownArguments,
}
