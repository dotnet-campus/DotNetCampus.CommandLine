using System.Diagnostics.Contracts;
using DotNetCampus.Cli.Utils;

namespace DotNetCampus.Cli;

/// <summary>
/// 在解析命令行参数时，指定命令行参数的解析方式。
/// </summary>
public readonly record struct CommandLineParsingOptions
{
    /// <inheritdoc cref="CommandLineStyle.Flexible" />
    public static CommandLineParsingOptions Flexible => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails(FlexibleMagic)
        {
            OptionValueSeparators = CommandSeparatorChars.Create(':', '='),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
        },
    };

    private static CommandLineStyleDetails FlexibleDefinition => new CommandLineStyleDetails
    {
        CaseSensitive = false,
        SupportsLongOption = true,
        SupportsShortOption = true,
        SupportsShortOptionCombination = false,
        SupportsMultiCharShortOption = false,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = true,
        NamingPolicy = CommandNamingPolicy.Both,
        OptionPrefix = CommandOptionPrefix.Any,
    };

    /// <inheritdoc cref="CommandLineStyle.DotNet" />
    public static CommandLineParsingOptions DotNet => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails(DotNetMagic)
        {
            OptionValueSeparators = CommandSeparatorChars.Create(':', '='),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
        },
    };

    private static CommandLineStyleDetails DotNetDefinition => new CommandLineStyleDetails
    {
        CaseSensitive = true,
        SupportsLongOption = true,
        SupportsShortOption = true,
        SupportsShortOptionCombination = false,
        SupportsMultiCharShortOption = true,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = true,
        NamingPolicy = CommandNamingPolicy.KebabCase,
        OptionPrefix = CommandOptionPrefix.DoubleDash,
    };

    /// <inheritdoc cref="CommandLineStyle.Gnu" />
    public static CommandLineParsingOptions Gnu => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails(GnuMagic)
        {
            OptionValueSeparators = CommandSeparatorChars.Create('='),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
        },
    };

    private static CommandLineStyleDetails GnuDefinition => new CommandLineStyleDetails
    {
        CaseSensitive = true,
        SupportsLongOption = true,
        SupportsShortOption = true,
        SupportsShortOptionCombination = true,
        SupportsMultiCharShortOption = false,
        SupportsShortOptionValueWithoutSeparator = true,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = false,
        NamingPolicy = CommandNamingPolicy.KebabCase,
        OptionPrefix = CommandOptionPrefix.DoubleDash,
    };

    /// <inheritdoc cref="CommandLineStyle.Posix" />
    public static CommandLineParsingOptions Posix => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails(PosixMagic)
        {
            OptionValueSeparators = CommandSeparatorChars.Create(),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
        },
    };

    private static CommandLineStyleDetails PosixDefinition => new CommandLineStyleDetails
    {
        CaseSensitive = true,
        SupportsLongOption = false,
        SupportsShortOption = true,
        SupportsShortOptionCombination = true,
        SupportsMultiCharShortOption = false,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = true,
        NamingPolicy = CommandNamingPolicy.PascalCase,
        // Posix 不支持长选项，使用 DoubleDash 的含义是 '-' 一定表示短选项。
        OptionPrefix = CommandOptionPrefix.DoubleDash,
    };


    /// <inheritdoc cref="CommandLineStyle.PowerShell" />
    public static CommandLineParsingOptions PowerShell => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails(PowerShellMagic)
        {
            OptionValueSeparators = CommandSeparatorChars.Create(':', '='),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
        },
    };

    /// <inheritdoc cref="CommandLineStyle.PowerShell" />
    private static CommandLineStyleDetails PowerShellDefinition => new CommandLineStyleDetails
    {
        CaseSensitive = false,
        SupportsLongOption = true,
        SupportsShortOption = true,
        SupportsShortOptionCombination = false,
        SupportsMultiCharShortOption = true,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = true,
        NamingPolicy = CommandNamingPolicy.PascalCase,
        OptionPrefix = CommandOptionPrefix.SlashOrDash,
    };

    /// <summary>
    /// 内部使用。当发现命令行参数只有一个，且符合 URL 格式时，无论用户设置了哪种命令行风格，都会使用此风格进行解析。
    /// </summary>
    public static CommandLineStyleDetails UrlStyle => new CommandLineStyleDetails(UrlMagic)
    {
        OptionValueSeparators = CommandSeparatorChars.Create('='),
        CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
    };

    /// <summary>
    /// 内部使用。当发现命令行参数只有一个，且符合 URL 格式时，无论用户设置了哪种命令行风格，都会使用此风格进行解析。
    /// </summary>
    private static CommandLineStyleDetails UrlDefinition => new CommandLineStyleDetails
    {
        CaseSensitive = false,
        SupportsLongOption = true,
        SupportsShortOption = false,
        SupportsShortOptionCombination = false,
        SupportsMultiCharShortOption = false,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = false,
        SupportsSpaceSeparatedCollectionValues = false,
        NamingPolicy = CommandNamingPolicy.Both,
        OptionPrefix = CommandOptionPrefix.DoubleDash,
    };

    /// <summary>
    /// 详细设置命令行解析时的各种细节。
    /// </summary>
    public CommandLineStyleDetails Style { get; init; }

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
    public IReadOnlyList<string>? SchemeNames { get; init; }

    private const ushort FlexibleMagic = 0x18C7;
    private const ushort DotNetMagic = 0x1AE1;
    private const ushort GnuMagic = 0xDE1;
    private const ushort PosixMagic = 0x19A2;
    private const ushort PowerShellMagic = 0x1ADA;
    private const ushort UrlMagic = 0x0043;

#if DEBUG

    /// <summary>
    /// 在单元测试里调用，以验证各种预定义的命令行风格没有被意外修改。
    /// </summary>
    public static void VerifyMagicNumbers()
    {
        var flexibleMagic = FlexibleDefinition.GetMagicNumber();
        var dotNetMagic = DotNetDefinition.GetMagicNumber();
        var gnuMagic = GnuDefinition.GetMagicNumber();
        var posixMagic = PosixDefinition.GetMagicNumber();
        var powerShellMagic = PowerShellDefinition.GetMagicNumber();
        var urlMagic = UrlDefinition.GetMagicNumber();
        if (flexibleMagic != FlexibleMagic)
        {
            throw new InvalidOperationException($"The new magic number of Flexible is 0x{flexibleMagic:X4}.");
        }
        if (dotNetMagic != DotNetMagic)
        {
            throw new InvalidOperationException($"The new magic number of DotNet is 0x{dotNetMagic:X4}.");
        }
        if (gnuMagic != GnuMagic)
        {
            throw new InvalidOperationException($"The new magic number of Gnu is 0x{gnuMagic:X4}.");
        }
        if (posixMagic != PosixMagic)
        {
            throw new InvalidOperationException($"The new magic number of Posix is 0x{posixMagic:X4}.");
        }
        if (powerShellMagic != PowerShellMagic)
        {
            throw new InvalidOperationException($"The new magic number of PowerShell is 0x{powerShellMagic:X4}.");
        }
        if (urlMagic != UrlMagic)
        {
            throw new InvalidOperationException($"The new magic number of UrlStyle is 0x{urlMagic:X4}.");
        }
    }

#endif
}

/// <summary>
/// 详细指定一种命令行风格的细节。
/// </summary>
public readonly record struct CommandLineStyleDetails()
{
    private readonly BooleanValues16 _booleans;

    /// <summary>
    /// 直接由程序员提前算好各种属性赋值完成后的魔数，节省应用程序启动期间的额外计算。
    /// </summary>
    /// <param name="magic">魔数。</param>
    internal CommandLineStyleDetails(ushort magic) : this()
    {
        _booleans = new BooleanValues16(magic);
    }

    /// <summary>
    /// 允许用户在命令行中使用的命令行参数风格。
    /// </summary>
    public CommandNamingPolicy NamingPolicy
    {
        // [0] 表示是否额外编译时转换以支持 PascalCase/CamelCase 命名法
        // [1] 表示视选项上定义的命名法为 kebab-case，并允许用户使用此 kebab-case 命名法输入命令
        get => _booleans[0, 1] switch
        {
            (true, true) => CommandNamingPolicy.Both,
            (true, false) => CommandNamingPolicy.KebabCase,
            (false, true) => CommandNamingPolicy.PascalCase,
            (false, false) => CommandNamingPolicy.Ordinal,
        };
        init => _booleans[0, 1] = value switch
        {
            CommandNamingPolicy.Both => (true, true),
            CommandNamingPolicy.KebabCase => (true, false),
            CommandNamingPolicy.PascalCase => (false, true),
            CommandNamingPolicy.Ordinal => (false, false),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    /// 指定命令行选项前缀的风格。
    /// </summary>
    public CommandOptionPrefix OptionPrefix
    {
        get => _booleans[2, 3, 4]switch
        {
            (false, false, false) => CommandOptionPrefix.DoubleDash,
            (false, false, true) => CommandOptionPrefix.SingleDash,
            (false, true, false) => CommandOptionPrefix.Slash,
            (false, true, true) => CommandOptionPrefix.SlashOrDash,
            (true, _, _) => CommandOptionPrefix.Any,
        };
        init => _booleans[2, 3, 4] = value switch
        {
            CommandOptionPrefix.DoubleDash => (false, false, false),
            CommandOptionPrefix.SingleDash => (false, false, true),
            CommandOptionPrefix.Slash => (false, true, false),
            CommandOptionPrefix.SlashOrDash => (false, true, true),
            CommandOptionPrefix.Any => (true, false, false),
            _ => (true, true, true),
        };
    }

    /// <summary>
    /// 在单独的选项没有特别指定时，默认是否区分大小写。
    /// </summary>
    public bool CaseSensitive
    {
        get => _booleans[5];
        init => _booleans[5] = value;
    }

    /// <summary>
    /// 是否支持长选项。
    /// </summary>
    public bool SupportsLongOption
    {
        get => _booleans[6];
        init => _booleans[6] = value;
    }

    /// <summary>
    /// 是否支持短选项。
    /// </summary>
    public bool SupportsShortOption
    {
        get => _booleans[7];
        init => _booleans[7] = value;
    }

    /// <summary>
    /// 当支持短选项时，是否支持将多个短选项组合在一起使用（短选项捆绑）。<br/>
    /// 例如 -abc 等同于 -a -b -c。<br/>
    /// 如果为 <see langword="false"/>，则 -abc 会被视为一个名为 "abc" 的短选项。
    /// </summary>
    /// <remarks>
    /// 此选项与 <see cref="SupportsMultiCharShortOption"/> 互斥。
    /// </remarks>
    public bool SupportsShortOptionCombination
    {
        get => _booleans[8];
        init => _booleans[8] = value;
    }

    /// <summary>
    /// 当支持短选项时，是否支持多字符短选项名称。<br/>
    /// 例如 -tl 作为 --terminal-logger 的短选项。
    /// </summary>
    /// <remarks>
    /// 此选项与 <see cref="SupportsShortOptionCombination"/> 互斥。
    /// </remarks>
    public bool SupportsMultiCharShortOption
    {
        get => _booleans[9];
        init => _booleans[9] = value;
    }

    /// <summary>
    /// 当支持短选项时，是否支持短选项直接跟值（不使用分隔符）。<br/>
    /// 例如 -abc 会被视为短选项 -a，值为 "bc"。<br/>
    /// 如果为 <see langword="false"/>，则会根据 <see cref="SupportsShortOptionCombination"/> 的值来决定
    /// -abc 是一个名为 "abc" 的短选项，还是 -a -b -c 三个短选项。
    /// </summary>
    public bool SupportsShortOptionValueWithoutSeparator
    {
        get => _booleans[10];
        init => _booleans[10] = value;
    }

    /// <summary>
    /// 是否支持使用空格分隔选项名和选项值。<br/>
    /// 例如 --option value 等同于 --option=value。<br/>
    /// 如果为 <see langword="false"/>，则 --option value 会被视为 --option 选项，value 会被视为下一个位置参数或选项。
    /// </summary>
    public bool SupportsSpaceSeparatedOptionValue
    {
        get => _booleans[11];
        init => _booleans[11] = value;
    }

    /// <summary>
    /// 当选项值为集合类型时，是否支持使用空格分隔多个选项值。<br/>
    /// 例如 --option value1 value2 等同于 --option value1,value2。<br/>
    /// 如果为 <see langword="false"/>，则 --option value1 value2 会被视为 --option 的值为 "value1"，而 "value2" 会被视为下一个位置参数或选项。
    /// </summary>
    public bool SupportsSpaceSeparatedCollectionValues
    {
        get => _booleans[12];
        init => _booleans[12] = value;
    }

    /// <summary>
    /// 允许用户使用哪些分隔符来分隔选项名和选项值。<br/>
    /// 如 ':', '=', ' ' 分别对应: --option:value, --option=value, --option value。
    /// </summary>
    /// <remarks>
    /// 如果指定空格(' ')，则表示选项名和选项值之间可以用空格分隔，如 --option value。<br/>
    /// 如果指定冒号(':')，则表示选项名和选项值之间可以用冒号分隔，如 --option:value。<br/>
    /// 如果指定等号('=')，则表示选项名和选项值之间可以用等号分隔，如 --option=value。<br/>
    /// 而如果指定为空字符('\0')，则此字符只会对短选项生效，表示短选项可以直接跟值，如 -oValue。毕竟长选项跟值也分不开，对吧！<br/>
    /// 基本上不会再存在其他种类的分隔符了……
    /// </remarks>
    public CommandSeparatorChars OptionValueSeparators { get; init; }

    /// <summary>
    /// 允许用户使用哪些分隔符来分隔集合类型的选项值。<br/>
    /// 如 ',', ';', ' ' 分别对应: --option value1,value2, --option value1;value2, --option value1 value2。
    /// </summary>
    public CommandSeparatorChars CollectionValueSeparators { get; init; }

    /// <summary>
    /// 获取用于存储样式细节的魔术数字。
    /// </summary>
    /// <returns>魔术数字。</returns>
    [Pure]
    internal ushort GetMagicNumber() => _booleans.GetMagicNumber();
}

/// <summary>
/// 允许用户在命令行中使用的命令和选项的命名风格。
/// </summary>
[Flags]
public enum CommandNamingPolicy : byte
{
    /// <summary>
    /// 无视明明风格，属性上定义的字符串必须与用户输入的命令或选项名称完全匹配。
    /// </summary>
    Ordinal = 0,

    /// <summary>
    /// PascalCase/camelCase 风格命名。
    /// </summary>
    PascalCase = 1,

    /// <summary>
    /// kebab-case 风格命名。
    /// </summary>
    /// <remarks>
    /// 由于我们已经约定在定义属性时，属性已经用 kebab-case 命名风格标记了名字，所以此选项实际上含义与 <see cref="Ordinal"/> 是等同的。
    /// </remarks>
    KebabCase = 1 << 1,

    /// <summary>
    /// 以 kebab-case 命名风格为主，兼顾支持 PascalCase/camelCase。
    /// </summary>
    Both = PascalCase | KebabCase,
}

/// <summary>
/// 指定命令行选项前缀的风格。
/// </summary>
public enum CommandOptionPrefix : byte
{
    /// <summary>
    /// 使用双短横线（--）作为长选项前缀，使用单个短横线（-）作为短选项前缀。
    /// </summary>
    DoubleDash,

    /// <summary>
    /// 使用单个短横线（-）作为长选项和短选项前缀。<br/>
    /// 注意：如果启用此选项，将不支持短选项组合和短选项直接跟值；仍支持多字符短选项，但解析会造成轻微的性能下降（因为会两次尝试匹配选项名）。
    /// </summary>
    SingleDash,

    /// <summary>
    /// 使用斜杠（/）作为长选项和短选项前缀。<br/>
    /// 注意：如果启用此选项，将不支持短选项组合和短选项直接跟值；仍支持多字符短选项，但解析会造成轻微的性能下降（因为会两次尝试匹配选项名）。
    /// </summary>
    Slash,

    /// <summary>
    /// 使用斜杠（/）或单个短横线（-）作为长选项和短选项前缀。<br/>
    /// 注意：如果启用此选项，将不支持短选项组合和短选项直接跟值；仍支持多字符短选项，但解析会造成轻微的性能下降（因为会两次尝试匹配选项名）。
    /// </summary>
    SlashOrDash,

    /// <summary>
    /// 允许使用任意一种前缀风格（-、--、/）。<br/>
    /// 注意：如果启用此选项，将不支持短选项组合和短选项直接跟值；仍支持多字符短选项，但解析会造成轻微的性能下降（因为会两次尝试匹配选项名）。
    /// </summary>
    Any,
}

/// <summary>
/// 选项值的类型。此枚举中的选项值类型会影响到选项值的解析方式。
/// </summary>
public enum OptionValueType : byte
{
    /// <summary>
    /// 普通值。只解析一个参数。
    /// </summary>
    Normal,

    /// <summary>
    /// 布尔值。会尝试解析一个参数，如果无法解析，则视为 <see langword="true"/>。
    /// </summary>
    Boolean,

    /// <summary>
    /// 集合值。会尝试解析多个参数，直到遇到下一个选项或位置参数分隔符为止。
    /// </summary>
    List,

    /// <summary>
    /// 字典值。会尝试解析多个键值对，直到遇到下一个选项或位置参数分隔符为止。
    /// </summary>
    Dictionary,

    /// <summary>
    /// 用户输入的选项没有命中到任何已知的选项。
    /// </summary>
    NotExist,
}

/// <summary>
/// 位置参数值的类型。此枚举中的位置参数值类型会影响到位置参数值的解析方式。
/// </summary>
public enum PositionalArgumentValueType : byte
{
    /// <summary>
    /// 正常的位置参数。
    /// </summary>
    Normal,

    /// <summary>
    /// 指定位置处的位置参数没有匹配到任何位置参数范围。
    /// </summary>
    NotExist,
}
