namespace DotNetCampus.Cli.Temp40;

/// <summary>
/// 在解析命令行参数时，指定命令行参数的解析方式。
/// </summary>
public readonly record struct CommandLineParsingOptions()
{
    /// <inheritdoc cref="CommandLineStyle.Flexible" />
    public static CommandLineParsingOptions Flexible => new CommandLineParsingOptions
    {
        Style = CommandLineStyle.Flexible,
        CaseSensitive = false,
    };

    /// <inheritdoc cref="CommandLineStyle.Gnu" />
    public static CommandLineParsingOptions Gnu => new CommandLineParsingOptions
    {
        Style = CommandLineStyle.Gnu,
        CaseSensitive = true,
    };

    /// <inheritdoc cref="CommandLineStyle.Posix" />
    public static CommandLineParsingOptions Posix => new CommandLineParsingOptions
    {
        Style = CommandLineStyle.Posix,
        CaseSensitive = true,
    };

    /// <inheritdoc cref="CommandLineStyle.DotNet" />
    public static CommandLineParsingOptions DotNet => new CommandLineParsingOptions
    {
        Style = CommandLineStyle.DotNet,
        CaseSensitive = false,
    };

    /// <inheritdoc cref="CommandLineStyle.PowerShell" />
    public static CommandLineParsingOptions PowerShell => new CommandLineParsingOptions
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
    /// <para>
    /// 例如：sample://open?url=DotNetCampus%20is%20a%20great%20team<br/>
    /// 这里的 "sample" 就是方案名。<br/>
    /// 当解析命令行参数时，如果只传入了一个参数，且参数开头满足 sample:// 格式时，则会认为方案名匹配，将进行后续 url 的参数解析。设置此属性后，无论选择哪种命令行风格(<see cref="Style"/>)，都会优先识别并解析URL格式的参数。
    /// </para>
    /// /// <para>
    /// URL风格命令行参数模拟Web请求中的查询字符串格式，适用于习惯于Web开发的用户，以及需要通过URL协议方案(URL Scheme)启动的应用程序。<br/>
    /// <br/>
    /// 详细规则：<br/>
    /// 1. 完整格式为 [scheme://][path][?option1=value1&amp;option2=value2]<br/>
    /// 2. 参数部分以问号(?)开始，后面是键值对<br/>
    /// 3. 多个参数之间用(&amp;)符号分隔<br/>
    /// 4. 每个参数的键值之间用等号(=)分隔<br/>
    /// 5. 支持URL编码规则，如空格编码为%20，特殊字符需编码<br/>
    /// 6. 支持数组格式参数，如tags=tag1&amp;tags=tag2表示tags参数有多个值<br/>
    /// 7. 支持无值参数，被视为布尔值true，如?enabled<br/>
    /// 8. 参数值为空字符串时保留等号，如?name=<br/>
    /// 9. 路径部分(path)一般情况下会被视为位置参数，例如 myapp://documents/open 中，documents/open 被视为位置参数<br/>
    /// 10. 但在某些情况下，路径的前几个部分可能会被当作命令（含子命令），例如 myapp://open/file.txt 中，open 可能是命令，file.txt 是位置参数。具体解释为位置参数还是命令取决于应用的命令行处理器实现<br/>
    /// 11. 整个URL可以用引号包围，以避免特殊字符被shell解释<br/>
    /// </para>
    /// <code>
    /// # 完整URL格式(通常由Web浏览器或其他应用程序传递)
    /// myapp://open?url=https://example.com        # 包含方案(scheme)、路径和参数
    /// myapp://user/profile?id=123&amp;tab=info    # 带层级路径
    /// sample://document/edit?id=42&amp;mode=full  # 多参数和路径组合
    ///
    /// # 特殊字符与编码
    /// yourapp://search?q=hello%20world            # 编码空格
    /// myapp://open?query=C%23%20programming       # 特殊字符编码
    /// appname://tags?value=c%23&amp;value=.net    # 数组参数(相同参数名多次出现)
    ///
    /// # 无值和空值参数
    /// myapp://settings?debug                      # 无值参数(视为true)
    /// yourapp://profile?name=&amp;id=123          # 空字符串值
    ///
    /// # 路径与命令示例
    /// myapp://documents/open?readonly=true        # documents 和 open 作为位置参数
    /// myapp://open/file.txt?temporary=true        # open 是命令，file.txt 是位置参数；或 open 和 file.txt 都是位置参数
    /// </code>
    /// </remarks>
    public IReadOnlyList<string> SchemeNames { get; init; } = [];
}
