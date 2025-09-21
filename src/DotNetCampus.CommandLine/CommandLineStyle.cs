using System.Diagnostics.Contracts;
using DotNetCampus.Cli.Utils;

namespace DotNetCampus.Cli;

/// <summary>
/// 详细指定一种命令行风格的细节。
/// </summary>
public readonly partial record struct CommandLineStyle()
{
    private readonly BooleanValues16 _booleans;

    /// <summary>
    /// 直接由程序员提前算好各种属性赋值完成后的魔数，节省应用程序启动期间的额外计算。
    /// </summary>
    /// <param name="magic">魔数。</param>
    internal CommandLineStyle(ushort magic) : this()
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
    /// 是否支持为布尔选项显式指定值。<br/>
    /// 例如 --option true, --option false, --option:1, --option:0 等等。<br/>
    /// 如果为 <see langword="false"/>，则 --option true 会被视为 --option 选项，true 会被视为下一个位置参数或选项。
    /// </summary>
    public bool SupportsExplicitBooleanOptionValue
    {
        get => _booleans[12];
        init => _booleans[12] = value;
    }

    /// <summary>
    /// 当选项值为集合类型时，是否支持使用空格分隔多个选项值。<br/>
    /// 例如 --option value1 value2 等同于 --option value1,value2。<br/>
    /// 如果为 <see langword="false"/>，则 --option value1 value2 会被视为 --option 的值为 "value1"，而 "value2" 会被视为下一个位置参数或选项。
    /// </summary>
    public bool SupportsSpaceSeparatedCollectionValues
    {
        get => _booleans[13];
        init => _booleans[13] = value;
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
    /// 此命令行风格的名称，用于调试和日志记录。
    /// </summary>
    public string Name { get; init; } = "Custom";

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
