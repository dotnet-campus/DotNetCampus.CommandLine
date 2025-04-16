using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <inheritdoc cref="CommandLineStyle.Gnu"/>
internal sealed class GnuStyleParser : ICommandLineParser
{
    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        var longOptions = new OptionDictionary(true);
        var shortOptions = new OptionDictionary(true);
        string? guessedVerbName = null;
        List<string> arguments = [];

        OptionName? lastOption = null;
        var lastType = GnuParsedType.Start;
        var shortLowPriorityOptions = new Dictionary<OptionName, string>();

        for (var i = 0; i < commandLineArguments.Count; i++)
        {
            var commandLineArgument = commandLineArguments[i];
            var result = GnuArgument.Parse(lastType, commandLineArgument);
            var tempLastType = lastType;
            lastType = result.Type;

            if (result.Type is GnuParsedType.VerbOrPositionalArgument)
            {
                lastOption = null;
                guessedVerbName = result.Value.ToString();
                arguments.Add(guessedVerbName);
                continue;
            }

            if (result.Type is GnuParsedType.PositionalArgument
                or GnuParsedType.PostPositionalArgument)
            {
                lastOption = null;
                arguments.Add(result.Value.ToString());
                continue;
            }

            if (result.Type is GnuParsedType.LongOption)
            {
                lastOption = result.Option;
                longOptions.AddOption(result.Option);
                continue;
            }

            if (result.Type is GnuParsedType.LongOptionWithValue)
            {
                lastOption = null;
                longOptions.AddValue(result.Option, result.Value.ToString());
                continue;
            }

            if (result.Type is GnuParsedType.ShortOption)
            {
                lastOption = result.Option;
                shortOptions.AddOption(result.Option);
                continue;
            }

            if (result.Type is GnuParsedType.ShortOptionWithValue)
            {
                lastOption = null;
                shortOptions.AddValue(result.Option, result.Value.ToString());
                continue;
            }

            if (result.Type is GnuParsedType.MultiShortOptions)
            {
                lastOption = null;
                foreach (var shortOption in result.Option)
                {
                    shortOptions.AddOption(shortOption);
                }
                continue;
            }

            if (result.Type is GnuParsedType.MultiShortOptionsOrShortOptionWithValue)
            {
                lastOption = null;
                foreach (var shortOption in result.Option)
                {
                    shortOptions.AddOption(shortOption);
                }
                shortLowPriorityOptions[result.Option[0]] = result.Value.ToString();
                continue;
            }

            if (result.Type is GnuParsedType.OptionValue)
            {
                // 选项值，直接添加到参数列表中。
                var options = tempLastType switch
                {
                    GnuParsedType.LongOption => longOptions,
                    GnuParsedType.ShortOption => shortOptions,
                    _ => throw new CommandLineParseException($"Argument value {result.Value.ToString()} does not belong to any option."),
                };
                if (lastOption is { } option)
                {
                    options.AddValue(option, result.Value.ToString());
                }
                continue;
            }

            if (result.Type is GnuParsedType.PositionalArgumentSeparator)
            {
                lastOption = null;
            }
        }

        // 最后，将潜在可能的短选项值添加到短选项中。-abc 其中 a 为选项，bc 为值。
        foreach (var pair in shortLowPriorityOptions)
        {
            if (!shortOptions.ContainsKey(pair.Key))
            {
                shortOptions.AddValue(pair.Key, pair.Value);
            }
        }

        return new CommandLineParsedResult(guessedVerbName,
            longOptions,
            shortOptions,
            arguments.ToReadOnlyList());
    }
}

internal readonly ref struct GnuArgument(GnuParsedType type)
{
    public GnuParsedType Type { get; } = type;
    public OptionName Option { get; private init; }
    public ReadOnlySpan<char> Value { get; private init; }

    public static GnuArgument Parse(GnuParsedType lastType, string argument)
    {
        var isPostPositionalArgument = lastType is GnuParsedType.PositionalArgumentSeparator or GnuParsedType.PostPositionalArgument;

        if (!isPostPositionalArgument && argument is ['-', '-', ..])
        {
            if (argument.Length is 2)
            {
                // 位置参数分隔符。
                return new GnuArgument(GnuParsedType.PositionalArgumentSeparator);
            }

            // 长选项。
            var spans = argument.AsSpan(2);
            for (var i = 0; i < spans.Length; i++)
            {
                if (i == 0 && !char.IsLetterOrDigit(spans[i]))
                {
                    // 长选项的第一个字符必须是字母或数字。
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 2]: {argument}");
                }
                if (spans[i] == '=')
                {
                    // 带值的长选项。--option=value
                    return new GnuArgument(GnuParsedType.LongOptionWithValue)
                        { Option = new OptionName(argument, new Range(2, i + 2)), Value = spans[(i + 1)..] };
                }
            }
            // 单独的长选项。--option
            return new GnuArgument(GnuParsedType.LongOption) { Option = new OptionName(argument, Range.StartAt(2)) };
        }

        if (!isPostPositionalArgument && argument is ['-', _, ..])
        {
            if (argument.Length is 2)
            {
                if (!char.IsLetterOrDigit(argument[1]))
                {
                    // 短选项字符必须是字母或数字。
                    throw new CommandLineParseException($"Invalid option format at index [{argument.Length}, 1]: {argument}");
                }
                // 单独的短选项。
                return new GnuArgument(GnuParsedType.ShortOption) { Option = new OptionName(argument, Range.StartAt(1)) };
            }

            var spans = argument.AsSpan(1);
            for (var i = 0; i < spans.Length; i++)
            {
                if (i == 0 && !char.IsLetterOrDigit(spans[i]))
                {
                    // 短选项的第一个字符必须是字母或数字。
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 1]: {argument}");
                }
                if (spans[i] == '=')
                {
                    // 带值的短选项。
                    return new GnuArgument(GnuParsedType.ShortOptionWithValue)
                        { Option = new OptionName(argument, new Range(1, i + 1)), Value = spans[(i + 1)..] };
                }
                if (!char.IsLetterOrDigit(spans[i]))
                {
                    // 包含非字母或数字，说明必定是带值的短选项。-o1.txt
                    return new GnuArgument(GnuParsedType.ShortOptionWithValue) { Option = new OptionName(argument, new Range(1, i + 1)), Value = spans[i..] };
                }
            }
            // 多个短选项，或者带值的短选项。
            return new GnuArgument(GnuParsedType.MultiShortOptionsOrShortOptionWithValue)
                { Option = new OptionName(argument, Range.StartAt(1)), Value = spans[1..] };
        }

        if (lastType is GnuParsedType.Start)
        {
            // 如果是第一个参数，则可能是或位置参数。
            return new GnuArgument(GnuParsedType.VerbOrPositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is GnuParsedType.VerbOrPositionalArgument or GnuParsedType.PositionalArgument)
        {
            // 如果是位置参数，则必定是位置参数。
            return new GnuArgument(GnuParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is GnuParsedType.OptionValue
            or GnuParsedType.LongOptionWithValue
            or GnuParsedType.ShortOptionWithValue
            or GnuParsedType.MultiShortOptionsOrShortOptionWithValue)
        {
            // 如果前一个已经是选项值了，那么后一个是位置参数。
            return new GnuArgument(GnuParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is GnuParsedType.PositionalArgumentSeparator or GnuParsedType.PostPositionalArgument)
        {
            // 如果是位置参数分隔符或后置位置参数，则必定是后置位置参数。
            return new GnuArgument(GnuParsedType.PostPositionalArgument) { Value = argument.AsSpan() };
        }

        // 其他情况，都是选项的值。
        return new GnuArgument(GnuParsedType.OptionValue) { Value = argument.AsSpan() };
    }
}

internal enum GnuParsedType
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
    /// 长选项。--long-option
    /// </summary>
    LongOption,

    /// <summary>
    /// 带值的长选项。--long-option=value
    /// </summary>
    LongOptionWithValue,

    /// <summary>
    /// 短选项。-o
    /// </summary>
    ShortOption,

    /// <summary>
    /// 带值的短选项。-o=value
    /// </summary>
    ShortOptionWithValue,

    /// <summary>
    /// 多个短选项。-abc
    /// </summary>
    MultiShortOptions,

    /// <summary>
    /// 多个短选项，也可能是带值的短选项。-abc -o1.txt
    /// </summary>
    MultiShortOptionsOrShortOptionWithValue,

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
