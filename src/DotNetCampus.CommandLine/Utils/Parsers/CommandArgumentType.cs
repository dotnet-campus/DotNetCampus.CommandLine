namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// 命令行参数的类型。
/// </summary>
internal enum CommandArgumentType
{
    /// <summary>
    /// 尚未开始解析。
    /// </summary>
    Start,

    /// <summary>
    /// 命令（主命令、子命令或多级子命令）。
    /// </summary>
    Command,

    /// <summary>
    /// 混在选项间的位置参数。
    /// </summary>
    PositionalArgument,

    /// <summary>
    /// 长选项。--option -Option /option -tl /tl
    /// </summary>
    LongOption,

    /// <summary>
    /// 带值的长选项。--option:value -Option:value /option:value -tl:off /tl:off
    /// </summary>
    LongOptionWithValue,

    /// <summary>
    /// 短选项。-o /o
    /// </summary>
    ShortOption,

    /// <summary>
    /// 带值的短选项。-o:value /o:value
    /// </summary>
    ShortOptionWithValue,

    /// <summary>
    /// 无法确定长还是短的选项。-o /o /option -tl /tl -Option
    /// </summary>
    Option,

    /// <summary>
    /// 带值的无法确定长还是短的选项。-o:value /o:value /option:value -tl:off /tl:off -Option:value
    /// </summary>
    OptionWithValue,

    /// <summary>
    /// 无法解析的选项。
    /// </summary>
    ErrorOption,

    /// <summary>
    /// 多个短选项。-abc
    /// </summary>
    MultiShortOptions,

    /// <summary>
    /// 不确定是多个短选项，还是一个无分隔符的带值短选项。-a1.txt
    /// </summary>
    MultiShortOptionsOrShortOptionConcatWithValue,

    /// <summary>
    /// 选项值。value
    /// </summary>
    OptionValue,

    /// <summary>
    /// 位置参数分隔符。-- 之后的参数都被视为位置参数。
    /// </summary>
    PositionalArgumentSeparator,

    /// <summary>
    /// 后置的位置参数。
    /// </summary>
    PostPositionalArgument,
}
