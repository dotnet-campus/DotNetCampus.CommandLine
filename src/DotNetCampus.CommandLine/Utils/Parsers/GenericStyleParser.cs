using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

public class GenericStyleParser
{
    public void Parse(ReadOnlySpan<string> arguments)
    {
        OptionName? lastOption = null;
        var lastType = CommandArgumentType.Start;

        for (var i = 0; i < arguments.Length; i++)
        {
            var argument = arguments[i];
            var result = CommandArgumentType.Parse(argument, lastType);

        }
    }
}

internal readonly ref struct CommandArgumentParser(CommandArgumentType type)
{
    public CommandArgumentType Type { get; } = type;
    public OptionName Option { get; private init; }
    public ReadOnlySpan<char> Value { get; private init; }

    public static CommandArgumentParser Parse(string argument, CommandArgumentType lastType)
    {

    }
}



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
    /// 多个短选项。-abc
    /// </summary>
    MultiShortOptions,

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
