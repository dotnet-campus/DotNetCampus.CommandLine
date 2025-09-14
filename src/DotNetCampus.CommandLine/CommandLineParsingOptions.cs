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
        Style = new CommandLineStyleDetails
        {
            CaseSensitive = false,
            SupportsLongOption = true,
            SupportsShortOption = true,
            SupportsShortOptionCombination = false,
            SupportsShortOptionValueWithoutSeparator = false,
            NamingPolicy = CommandNamingPolicy.Both,
            OptionPrefix = CommandOptionPrefix.Any,
            OptionValueSeparators = CommandSeparatorChars.Create(':', '=', ' '),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';', ' '),
            DictionaryValueSeparators = CommandSeparatorChars.Create('='),
        },
    };

    /// <inheritdoc cref="CommandLineStyle.DotNet" />
    public static CommandLineParsingOptions DotNet => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails
        {
            CaseSensitive = true,
            SupportsLongOption = true,
            SupportsShortOption = true,
            SupportsShortOptionCombination = false,
            SupportsShortOptionValueWithoutSeparator = false,
            NamingPolicy = CommandNamingPolicy.KebabCase,
            OptionPrefix = CommandOptionPrefix.DoubleDash,
            OptionValueSeparators = CommandSeparatorChars.Create(':', '=', ' '),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';', ' '),
            DictionaryValueSeparators = CommandSeparatorChars.Create('='),
        },
    };

    /// <inheritdoc cref="CommandLineStyle.Gnu" />
    public static CommandLineParsingOptions Gnu => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails
        {
            CaseSensitive = true,
            SupportsLongOption = true,
            SupportsShortOption = true,
            SupportsShortOptionCombination = true,
            SupportsShortOptionValueWithoutSeparator = true,
            NamingPolicy = CommandNamingPolicy.KebabCase,
            OptionPrefix = CommandOptionPrefix.DoubleDash,
            OptionValueSeparators = CommandSeparatorChars.Create('=', ' '),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';', ' '),
            DictionaryValueSeparators = CommandSeparatorChars.Create('='),
        },
    };

    /// <inheritdoc cref="CommandLineStyle.Posix" />
    public static CommandLineParsingOptions Posix => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails
        {
            CaseSensitive = true,
            SupportsLongOption = false,
            SupportsShortOption = true,
            SupportsShortOptionCombination = false,
            SupportsShortOptionValueWithoutSeparator = false,
            NamingPolicy = CommandNamingPolicy.CamelCase,
            OptionPrefix = CommandOptionPrefix.SingleDash,
            OptionValueSeparators = CommandSeparatorChars.Create(' '),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
            DictionaryValueSeparators = CommandSeparatorChars.Create('='),
        },
    };

    /// <inheritdoc cref="CommandLineStyle.PowerShell" />
    public static CommandLineParsingOptions PowerShell => new CommandLineParsingOptions
    {
        Style = new CommandLineStyleDetails
        {
            CaseSensitive = false,
            SupportsLongOption = true,
            SupportsShortOption = true,
            SupportsShortOptionCombination = false,
            SupportsShortOptionValueWithoutSeparator = false,
            NamingPolicy = CommandNamingPolicy.PascalCase,
            OptionPrefix = CommandOptionPrefix.Slash,
            OptionValueSeparators = CommandSeparatorChars.Create(':', '=', ' '),
            CollectionValueSeparators = CommandSeparatorChars.Create(',', ';', ' '),
            DictionaryValueSeparators = CommandSeparatorChars.Create('='),
        },
    };

    /// <summary>
    /// 详细设置命令行解析时的各种细节。
    /// </summary>
    public CommandLineStyleDetails Style { get; init; }
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
        // [1] 表示原样大小写，还是编译时按命名法转小写
        // [2] 表示是否同时支持 kebab-case 和 PascalCase/CamelCase 命名法
        get => _booleans[0, 1, 2] switch
        {
            (true, true, false) => CommandNamingPolicy.PascalCase,
            (true, false, false) => CommandNamingPolicy.CamelCase,
            (false, true, false) => CommandNamingPolicy.KebabCase,
            (false, false, false) => CommandNamingPolicy.KebabCaseLower,
            (true, true, true) => CommandNamingPolicy.Both,
            (true, false, true) => CommandNamingPolicy.BothLower,
            _ => throw new InvalidOperationException("Invalid naming policy."),
        };
        init => _booleans[0, 1, 2] = value switch
        {
            CommandNamingPolicy.PascalCase => (true, true, false),
            CommandNamingPolicy.CamelCase => (true, false, false),
            CommandNamingPolicy.KebabCase => (false, true, false),
            CommandNamingPolicy.KebabCaseLower => (false, false, false),
            CommandNamingPolicy.Both => (true, true, true),
            CommandNamingPolicy.BothLower => (true, false, true),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    /// 指定命令行选项前缀的风格。
    /// </summary>
    public CommandOptionPrefix OptionPrefix
    {
        // [3] 表示是否使用短横线（-）作为选项前缀
        // [4] 表示长选项是否使用双短横线（--）
        get => _booleans[3, 4]switch
        {
            (true, true) => CommandOptionPrefix.DoubleDash,
            (true, false) => CommandOptionPrefix.SingleDash,
            (false, _) => CommandOptionPrefix.Slash,
        };
        init => _booleans[3, 4] = value switch
        {
            CommandOptionPrefix.DoubleDash => (true, true),
            CommandOptionPrefix.SingleDash => (true, false),
            CommandOptionPrefix.Slash => (false, false),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
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
    public bool SupportsShortOptionCombination
    {
        get => _booleans[8];
        init => _booleans[8] = value;
    }

    /// <summary>
    /// 当支持短选项时，是否支持短选项直接跟值（不使用分隔符）。<br/>
    /// 例如 -abc 会被视为短选项 -a，值为 "bc"。<br/>
    /// 如果为 <see langword="false"/>，则会根据 <see cref="SupportsShortOptionCombination"/> 的值来决定
    /// -abc 是一个名为 "abc" 的短选项，还是 -a -b -c 三个短选项。
    /// </summary>
    public bool SupportsShortOptionValueWithoutSeparator
    {
        get => _booleans[9];
        init => _booleans[9] = value;
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
    /// 允许用户使用哪些分隔符来分隔字典类型的选项值中的键和值。<br/>
    /// 如 '=', ':' 分别对应: --option key=value, --option key:value。
    /// </summary>
    public CommandSeparatorChars DictionaryValueSeparators { get; init; }
}

/// <summary>
/// 允许用户在命令行中使用的命令和选项的命名风格。
/// </summary>
/// <remarks>
/// 虽然在不区分大小写时，<see cref="PascalCase"/> 和 <see cref="CamelCase"/> 看起来是一样的，但在输出帮助文档时会以设定的为准。
/// </remarks>
public enum CommandNamingPolicy : byte
{
    /// <summary>
    /// PascalCase 风格命名。
    /// </summary>
    PascalCase,

    /// <summary>
    /// camelCase 风格命名。
    /// </summary>
    CamelCase,

    /// <summary>
    /// kebab-case 风格命名，保持原样大小写。
    /// </summary>
    KebabCase,

    /// <summary>
    /// kebab-case 风格命名，且所有字母均为小写。
    /// </summary>
    KebabCaseLower,

    /// <summary>
    /// 以 kebab-case 命名风格为主，兼顾支持 PascalCase。
    /// </summary>
    Both,

    /// <summary>
    /// 以 kebab-case 命名风格为主（所有字母均为小写），兼顾支持 PascalCase 和 camelCase。
    /// </summary>
    BothLower,
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
    Collection,

    /// <summary>
    /// 用户输入的选项没有命中到任何已知的选项。
    /// </summary>
    NotExist,
}
