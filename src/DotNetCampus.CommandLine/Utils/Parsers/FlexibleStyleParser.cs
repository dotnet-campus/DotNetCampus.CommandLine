using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <inheritdoc cref="CommandLineStyle.Flexible"/>
internal sealed class FlexibleStyleParser : ICommandLineParser
{
    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        var longOptions = new OptionDictionary(true);
        var shortOptions = new OptionDictionary(true);
        string? guessedVerbName = null;
        List<string> arguments = [];

        OptionDictionary? lastOptions = null;
        OptionName? lastOption = null;
        var lastType = FlexibleParsedType.Start;

        for (var i = 0; i < commandLineArguments.Count; i++)
        {
            var commandLineArgument = commandLineArguments[i];
            var result = FlexibleArgument.Parse(commandLineArgument, lastType);
            var tempLastType = lastType;
            lastType = result.Type;

            if (result.Type is FlexibleParsedType.VerbOrPositionalArgument)
            {
                lastOptions = null;
                lastOption = null;
                guessedVerbName = result.Value.ToString();
                arguments.Add(guessedVerbName);
                continue;
            }

            if (result.Type is FlexibleParsedType.PositionalArgument
                or FlexibleParsedType.PostPositionalArgument)
            {
                lastOptions = null;
                lastOption = null;
                arguments.Add(result.Value.ToString());
                continue;
            }

            if (result.Type is FlexibleParsedType.LongOption)
            {
                lastOptions = longOptions;
                lastOption = result.Option;
                longOptions.AddOption(result.Option);
                continue;
            }

            if (result.Type is FlexibleParsedType.LongOptionWithValue)
            {
                lastOptions = null;
                lastOption = null;
                longOptions.AddValue(result.Option, result.Value.ToString());
                continue;
            }

            if (result.Type is FlexibleParsedType.ShortOption)
            {
                lastOptions = shortOptions;
                lastOption = result.Option;
                shortOptions.AddOption(result.Option);
                continue;
            }

            if (result.Type is FlexibleParsedType.ShortOptionWithValue)
            {
                lastOptions = null;
                lastOption = null;
                shortOptions.AddValue(result.Option, result.Value.ToString());
                continue;
            }

            if (result.Type is FlexibleParsedType.OptionValue)
            {
                // 选项值，直接添加到参数列表中。
                if (lastOptions is { } options && lastOption is { } option)
                {
                    options.AddValue(option, result.Value.ToString());
                }
                continue;
            }

            if (result.Type is FlexibleParsedType.PositionalArgumentSeparator)
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

internal readonly ref struct FlexibleArgument(FlexibleParsedType type)
{
    public FlexibleParsedType Type { get; } = type;
    public OptionName Option { get; private init; }
    public ReadOnlySpan<char> Value { get; private init; }

    public static FlexibleArgument Parse(string argument, FlexibleParsedType lastType)
    {
        var isPostPositionalArgument = lastType is FlexibleParsedType.PositionalArgumentSeparator or FlexibleParsedType.PostPositionalArgument;
        var hasPrefix = OperatingSystem.IsWindows()
            ? argument.Length > 0 && (argument[0] is '-' or '/')
            : argument.Length > 0 && argument[0] is '-';

        if (!isPostPositionalArgument && hasPrefix)
        {
            if (argument.Length is 1)
            {
                // 只有一个破折号或斜杠，这在.NET CLI风格中通常被视为位置参数。
                return new FlexibleArgument(FlexibleParsedType.PositionalArgument) { Value = argument.AsSpan() };
            }

            if (argument.Length is 2)
            {
                if (argument[0] is '-' && argument[1] is '-')
                {
                    // 位置参数分隔符。
                    return new FlexibleArgument(FlexibleParsedType.PositionalArgumentSeparator);
                }
                if (char.IsLetterOrDigit(argument[1]))
                {
                    // 短选项。
                    return new FlexibleArgument(FlexibleParsedType.ShortOption) { Option = new OptionName(argument, Range.StartAt(1)) };
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
                if (c is ':' or '=')
                {
                    // 带值的长选项。--option:value --option=value
                    return new FlexibleArgument(FlexibleParsedType.LongOptionWithValue)
                    {
                        Option = isKebabCase
                            ? new OptionName(argument, new Range(wordStartIndex, i + wordStartIndex))
                            : new OptionName(OptionName.MakeKebabCase(spans[..i]), Range.All),
                        Value = spans[(i + 1)..],
                    };
                }
            }
            // 单独的长选项。--option
            return new FlexibleArgument(FlexibleParsedType.LongOption)
            {
                Option = isKebabCase
                    ? new OptionName(argument, Range.StartAt(wordStartIndex))
                    : new OptionName(OptionName.MakeKebabCase(spans), Range.All),
            };
        }

        // 处理各种类型的位置参数
        if (lastType is FlexibleParsedType.Start)
        {
            // 如果是第一个参数，则可能是或位置参数。
            return new FlexibleArgument(FlexibleParsedType.VerbOrPositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is FlexibleParsedType.VerbOrPositionalArgument or FlexibleParsedType.PositionalArgument)
        {
            // 如果是位置参数，则必定是位置参数。
            return new FlexibleArgument(FlexibleParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is FlexibleParsedType.OptionValue)
        {
            // Flexible 允许选项后面的多个单独的选项值。
            return new FlexibleArgument(FlexibleParsedType.OptionValue) { Value = argument.AsSpan() };
        }

        if (lastType is FlexibleParsedType.LongOptionWithValue
            or FlexibleParsedType.ShortOptionWithValue)
        {
            // 如果前一个已经是带有值的选项了，那么后一个是位置参数。
            return new FlexibleArgument(FlexibleParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is FlexibleParsedType.PositionalArgumentSeparator or FlexibleParsedType.PostPositionalArgument)
        {
            // 如果是位置参数分隔符或后置位置参数，则必定是后置位置参数。
            return new FlexibleArgument(FlexibleParsedType.PostPositionalArgument) { Value = argument.AsSpan() };
        }

        // 其他情况，都是选项的值。
        return new FlexibleArgument(FlexibleParsedType.OptionValue) { Value = argument.AsSpan() };
    }
}

internal enum FlexibleParsedType
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
