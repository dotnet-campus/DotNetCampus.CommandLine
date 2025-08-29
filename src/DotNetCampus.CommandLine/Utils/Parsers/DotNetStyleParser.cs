using System.Runtime.InteropServices;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <inheritdoc cref="CommandLineStyle.DotNet"/>
internal sealed class DotNetStyleParser : ICommandLineParser
{
    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        var longOptions = new OptionDictionary(true);
        var shortOptions = new OptionDictionary(true);
        string? guessedVerbName = null;
        List<string> arguments = [];

        OptionName? lastOption = null;
        var lastType = DotNetParsedType.Start;

        for (var i = 0; i < commandLineArguments.Count; i++)
        {
            var commandLineArgument = commandLineArguments[i];
            var result = DotNetArgument.Parse(commandLineArgument, lastType);
            var tempLastType = lastType;
            lastType = result.Type;

            if (result.Type is DotNetParsedType.VerbOrPositionalArgument)
            {
                lastOption = null;
                guessedVerbName = result.Value.ToString();
                arguments.Add(guessedVerbName);
                continue;
            }

            if (result.Type is DotNetParsedType.PositionalArgument
                or DotNetParsedType.PostPositionalArgument)
            {
                lastOption = null;
                arguments.Add(result.Value.ToString());
                continue;
            }

            if (result.Type is DotNetParsedType.LongOption)
            {
                lastOption = result.Option;
                longOptions.AddOption(result.Option);
                continue;
            }

            if (result.Type is DotNetParsedType.LongOptionWithValue)
            {
                lastOption = null;
                longOptions.AddValue(result.Option, result.Value.ToString());
                continue;
            }

            if (result.Type is DotNetParsedType.ShortOption)
            {
                lastOption = result.Option;
                shortOptions.AddOption(result.Option);
                continue;
            }

            if (result.Type is DotNetParsedType.ShortOptionWithValue)
            {
                lastOption = null;
                shortOptions.AddValue(result.Option, result.Value.ToString());
                continue;
            }

            if (result.Type is DotNetParsedType.OptionValue)
            {
                // 选项值，直接添加到参数列表中。
                var options = tempLastType switch
                {
                    DotNetParsedType.LongOption => longOptions,
                    DotNetParsedType.ShortOption => shortOptions,
                    _ => throw new CommandLineParseException($"Argument value {result.Value.ToString()} does not belong to any option."),
                };
                if (lastOption is { } option)
                {
                    options.AddValue(option, result.Value.ToString());
                }
                continue;
            }

            if (result.Type is DotNetParsedType.PositionalArgumentSeparator)
            {
                lastOption = null;
            }
        }

        return new CommandLineParsedResult(guessedVerbName,
            longOptions,
            shortOptions,
            arguments.ToReadOnlyList());
    }
}

internal readonly ref struct DotNetArgument(DotNetParsedType type)
{
    public DotNetParsedType Type { get; } = type;
    public OptionName Option { get; private init; }
    public ReadOnlySpan<char> Value { get; private init; }

    public static DotNetArgument Parse(string argument, DotNetParsedType lastType)
    {
        var isPostPositionalArgument = lastType is DotNetParsedType.PositionalArgumentSeparator or DotNetParsedType.PostPositionalArgument;
        var hasPrefix =
#if NET5_0_OR_GREATER
            OperatingSystem.IsWindows()
#else
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
#endif
                ? argument.Length > 0 && (argument[0] is '-' or '/')
                : argument.Length > 0 && argument[0] is '-';

        if (!isPostPositionalArgument && hasPrefix)
        {
            if (argument.Length is 1)
            {
                // 只有一个破折号或斜杠，这在.NET CLI风格中通常被视为位置参数。
                return new DotNetArgument(DotNetParsedType.PositionalArgument) { Value = argument.AsSpan() };
            }

            if (argument.Length is 2)
            {
                if (argument[0] is '-' && argument[1] is '-')
                {
                    // 位置参数分隔符。
                    return new DotNetArgument(DotNetParsedType.PositionalArgumentSeparator);
                }
                if (char.IsLetterOrDigit(argument[1]))
                {
                    // 短选项。
                    return new DotNetArgument(DotNetParsedType.ShortOption) { Option = new OptionName(argument, Range.StartAt(1)) };
                }
                throw new CommandLineParseException($"Invalid option format at index [0, 1]: {argument}");
            }

            // 长选项。
            var isKebabCase = true;
            var wordStartIndex = argument[1] is '-' ? 2 : 1;
            var spans = argument.AsSpan(wordStartIndex);
            for (var i = 0; i < spans.Length; i++)
            {
                var c = spans[i];
                if (i == 0 && !char.IsLetterOrDigit(c))
                {
                    // 长选项的第一个字符必须是字母或数字。
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 2]: {argument}");
                }
                if (i > 0 && char.IsUpper(c) && spans[i - 1] != '-')
                {
                    // 遇到 PascalCase 或 camelCase，需要转换为 kebab-case。
                    isKebabCase = false;
                }
                if (c is ':')
                {
                    // 带值的长选项。--option:value
                    return new DotNetArgument(DotNetParsedType.LongOptionWithValue)
                    {
                        Option = isKebabCase
                            ? new OptionName(argument, new Range(wordStartIndex, i + wordStartIndex))
                            : new OptionName(OptionName.MakeKebabCase(spans[..i]), Range.All),
                        Value = spans[(i + 1)..],
                    };
                }
            }
            // 单独的长选项。--option
            return new DotNetArgument(DotNetParsedType.LongOption)
            {
                Option = isKebabCase
                    ? new OptionName(argument, Range.StartAt(wordStartIndex))
                    : new OptionName(OptionName.MakeKebabCase(spans), Range.All),
            };
        }

        // 处理各种类型的位置参数
        if (lastType is DotNetParsedType.Start)
        {
            // 如果是第一个参数，则可能是或位置参数。
            return new DotNetArgument(DotNetParsedType.VerbOrPositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is DotNetParsedType.VerbOrPositionalArgument or DotNetParsedType.PositionalArgument)
        {
            // 如果是位置参数，则必定是位置参数。
            return new DotNetArgument(DotNetParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is DotNetParsedType.OptionValue
            or DotNetParsedType.LongOptionWithValue
            or DotNetParsedType.ShortOptionWithValue)
        {
            // 如果前一个已经是选项值了，那么后一个是位置参数。
            return new DotNetArgument(DotNetParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is DotNetParsedType.PositionalArgumentSeparator or DotNetParsedType.PostPositionalArgument)
        {
            // 如果是位置参数分隔符或后置位置参数，则必定是后置位置参数。
            return new DotNetArgument(DotNetParsedType.PostPositionalArgument) { Value = argument.AsSpan() };
        }

        // 其他情况，都是选项的值。
        return new DotNetArgument(DotNetParsedType.OptionValue) { Value = argument.AsSpan() };
    }
}

internal enum DotNetParsedType
{
    /// <summary>
    /// 尚未开始解析。
    /// </summary>
    Start,

    /// <summary>
    /// 第一个位置参数，也可能是谓词。
    /// </summary>
    VerbOrPositionalArgument,

    /// <summary>
    /// 位置参数。
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
