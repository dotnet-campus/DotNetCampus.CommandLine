using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <inheritdoc cref="CommandLineStyle.Posix"/>
internal sealed class PosixStyleParser : ICommandLineParser
{
    internal static bool ConvertPascalCaseToKebabCase { get; } = false;

    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        var shortOptions = new OptionDictionary(true);
        var possibleCommandNamesLength = 0;
        List<string> arguments = [];

        OptionName? lastOption = null;
        var lastType = PosixParsedType.Start;

        for (var i = 0; i < commandLineArguments.Count; i++)
        {
            var commandLineArgument = commandLineArguments[i];
            var result = PosixArgument.Parse(commandLineArgument, lastType);
            var tempLastType = lastType;
            lastType = result.Type;

            if (result.Type is PosixParsedType.CommandNameOrPositionalArgument)
            {
                lastOption = null;
                possibleCommandNamesLength++;
                var commandNameOrPositionalArgument = result.Value.ToString();
                arguments.Add(commandNameOrPositionalArgument);
                continue;
            }

            if (result.Type is PosixParsedType.PositionalArgument
                or PosixParsedType.PostPositionalArgument)
            {
                lastOption = null;
                arguments.Add(result.Value.ToString());
                continue;
            }

            if (result.Type is PosixParsedType.ShortOption)
            {
                lastOption = result.Option;
                shortOptions.AddOption(result.Option);
                continue;
            }

            if (result.Type is PosixParsedType.MultiShortOptions)
            {
                lastOption = null;
                foreach (var shortOption in result.Option)
                {
                    shortOptions.AddOption(shortOption);
                }
                continue;
            }

            if (result.Type is PosixParsedType.OptionValue)
            {
                // 选项值，直接添加到参数列表中。
                var options = tempLastType switch
                {
                    PosixParsedType.ShortOption => shortOptions,
                    _ => throw new CommandLineParseException($"Argument value {result.Value.ToString()} does not belong to any option."),
                };
                if (lastOption is { } option)
                {
                    options.AddValue(option, result.Value.ToString());
                    lastOption = null;
                }
                continue;
            }

            if (result.Type is PosixParsedType.PositionalArgumentSeparator)
            {
                lastOption = null;
            }
        }

        return new CommandLineParsedResult(
            CommandLineParsedResult.MakePossibleCommandNames(commandLineArguments, possibleCommandNamesLength, ConvertPascalCaseToKebabCase),
            OptionDictionary.Empty, // POSIX 风格不支持长选项
            shortOptions,
            arguments.ToReadOnlyList());
    }
}

internal readonly ref struct PosixArgument(PosixParsedType type)
{
    public PosixParsedType Type { get; } = type;
    public OptionName Option { get; private init; }
    public ReadOnlySpan<char> Value { get; private init; }

    public static PosixArgument Parse(string argument, PosixParsedType lastType)
    {
        var isPostPositionalArgument = lastType is PosixParsedType.PositionalArgumentSeparator or PosixParsedType.PostPositionalArgument;

        if (!isPostPositionalArgument && argument is "--")
        {
            // 位置参数分隔符。
            return new PosixArgument(PosixParsedType.PositionalArgumentSeparator);
        }

        if (!isPostPositionalArgument && argument is ['-', '-', ..])
        {
            // POSIX 风格不支持长选项
            throw new CommandLineParseException($"Long options (starting with '--') are not supported in POSIX style: {argument}");
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
                return new PosixArgument(PosixParsedType.ShortOption) { Option = new OptionName(argument, Range.StartAt(1)) };
            }

            // 检查所有字符是否都是有效的选项字符
            for (var i = 1; i < argument.Length; i++)
            {
                if (!char.IsLetterOrDigit(argument[i]))
                {
                    throw new CommandLineParseException($"Invalid option character in POSIX style: {argument[i]} in {argument}");
                }
            }

            // 多个短选项，如 -abc
            return new PosixArgument(PosixParsedType.MultiShortOptions) { Option = new OptionName(argument, Range.StartAt(1)) };
        }

        if (lastType is PosixParsedType.Start or PosixParsedType.CommandNameOrPositionalArgument)
        {
            // 如果是第一个参数，则后续可能是命令名或位置参数。
            // 如果可能是命令名或位置参数，则后续也可能是命令名或位置参数。
            var isValidName = OptionName.IsValidOptionName(argument.AsSpan());
            return new PosixArgument(isValidName ? PosixParsedType.CommandNameOrPositionalArgument : PosixParsedType.PositionalArgument)
            {
                Value = argument.AsSpan(),
            };
        }

        if (lastType is PosixParsedType.PositionalArgument)
        {
            // 如果上一个是位置参数，则这个也是位置参数。
            return new PosixArgument(PosixParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is PosixParsedType.OptionValue)
        {
            // 如果前一个已经是选项值了，那么后一个是位置参数。
            return new PosixArgument(PosixParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is PosixParsedType.PositionalArgumentSeparator or PosixParsedType.PostPositionalArgument)
        {
            // 如果是位置参数分隔符或后置位置参数，则必定是后置位置参数。
            return new PosixArgument(PosixParsedType.PostPositionalArgument) { Value = argument.AsSpan() };
        }        if (lastType is PosixParsedType.MultiShortOptions)
        {
            // 在POSIX风格中，组合短选项后面不能直接跟参数值
            throw new CommandLineParseException($"Combined short options cannot have parameters in POSIX style: {argument}");
        }

        // 其他情况，是单个短选项的值。
        return new PosixArgument(PosixParsedType.OptionValue) { Value = argument.AsSpan() };
    }
}

internal enum PosixParsedType
{
    /// <summary>
    /// 尚未开始解析。
    /// </summary>
    Start,

    /// <summary>
    /// 前几个位置参数，也可能是命令名。
    /// </summary>
    CommandNameOrPositionalArgument,

    /// <summary>
    /// 位置参数。
    /// </summary>
    PositionalArgument,

    /// <summary>
    /// 短选项。-o
    /// </summary>
    ShortOption,

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
