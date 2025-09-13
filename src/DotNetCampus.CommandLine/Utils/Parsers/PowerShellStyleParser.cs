using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <inheritdoc cref="LegacyCommandLineStyle.PowerShell"/>
internal sealed class PowerShellStyleParser : ICommandLineParser
{
    internal static bool ConvertPascalCaseToKebabCase { get; } = true;

    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        var longOptions = new OptionDictionary(true);
        var possibleCommandNamesLength = 0;
        List<string> arguments = [];

        OptionName? lastOption = null;
        var lastType = PowerShellParsedType.Start;

        for (var i = 0; i < commandLineArguments.Count; i++)
        {
            var commandLineArgument = commandLineArguments[i];
            var result = PowerShellArgument.Parse(commandLineArgument, lastType);
            lastType = result.Type;

            if (result.Type is PowerShellParsedType.CommandNameOrPositionalArgument)
            {
                lastOption = null;
                possibleCommandNamesLength++;
                var commandNameOrPositionalArgument = result.Value.ToString();
                arguments.Add(commandNameOrPositionalArgument);
                continue;
            }

            if (result.Type is PowerShellParsedType.PositionalArgument
                or PowerShellParsedType.PostPositionalArgument)
            {
                lastOption = null;
                arguments.Add(result.Value.ToString());
                continue;
            }

            if (result.Type is PowerShellParsedType.Option)
            {
                lastOption = result.Option;
                longOptions.AddOption(result.Option);
                continue;
            }

            if (result.Type is PowerShellParsedType.OptionValue)
            {
                // 选项值
                if (lastOption is { } option)
                {
                    longOptions.AddValue(option, result.Value.ToString());
                }
                continue;
            }

            if (result.Type is PowerShellParsedType.PositionalArgumentSeparator)
            {
                lastOption = null;
            }
        }

        return new CommandLineParsedResult(
            CommandLineParsedResult.MakePossibleCommandNames(commandLineArguments, possibleCommandNamesLength, ConvertPascalCaseToKebabCase),
            longOptions,
            // PowerShell 风格不使用短选项，所以直接使用空字典。
            OptionDictionary.Empty,
            arguments.ToReadOnlyList());
    }
}

internal readonly ref struct PowerShellArgument(PowerShellParsedType type)
{
    public PowerShellParsedType Type { get; } = type;
    public OptionName Option { get; private init; }
    public ReadOnlySpan<char> Value { get; private init; }

    public static PowerShellArgument Parse(string argument, PowerShellParsedType lastType)
    {
        var isPostPositionalArgument = lastType is PowerShellParsedType.PositionalArgumentSeparator or PowerShellParsedType.PostPositionalArgument;

        if (!isPostPositionalArgument && argument is "--")
        {
            // 位置参数分隔符。
            return new PowerShellArgument(PowerShellParsedType.PositionalArgumentSeparator);
        }

        if (!isPostPositionalArgument && argument.StartsWith(
#if NETCOREAPP3_1_OR_GREATER
                '-'
#else
                "-"
#endif
            ) && argument.Length > 1 && !char.IsDigit(argument[1]))
        {
            // PowerShell 风格的选项 (-ParameterName)
            var optionSpan = argument.AsSpan(1);
            return new PowerShellArgument(PowerShellParsedType.Option)
            {
                Option = OptionName.MakeKebabCase(optionSpan, PowerShellStyleParser.ConvertPascalCaseToKebabCase),
            };
        }

        // 处理各种类型的位置参数和选项值
        if (lastType is PowerShellParsedType.Start or PowerShellParsedType.CommandNameOrPositionalArgument)
        {
            // 如果是第一个参数，则后续可能是命令名或位置参数。
            // 如果可能是命令名或位置参数，则后续也可能是命令名或位置参数。
            var isValidName = OptionName.IsValidOptionName(argument.AsSpan());
            return new PowerShellArgument(isValidName ? PowerShellParsedType.CommandNameOrPositionalArgument : PowerShellParsedType.PositionalArgument)
            {
                Value = argument.AsSpan(),
            };
        }

        if (lastType is PowerShellParsedType.PositionalArgument)
        {
            // 如果前一个是位置参数，则当前也是位置参数。
            return new PowerShellArgument(PowerShellParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is PowerShellParsedType.Option)
        {
            // 如果前一个是选项，则当前是选项值。
            return new PowerShellArgument(PowerShellParsedType.OptionValue) { Value = argument.AsSpan() };
        }

        if (lastType is PowerShellParsedType.OptionValue)
        {
            // 如果前一个已经是选项值了，那么后一个是位置参数。
            return new PowerShellArgument(PowerShellParsedType.PositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is PowerShellParsedType.PositionalArgumentSeparator or PowerShellParsedType.PostPositionalArgument)
        {
            // 如果前一个是位置参数分隔符或后置位置参数，则当前是后置位置参数。
            return new PowerShellArgument(PowerShellParsedType.PostPositionalArgument) { Value = argument.AsSpan() };
        }

        // 其他情况，都视为位置参数。
        return new PowerShellArgument(PowerShellParsedType.PositionalArgument) { Value = argument.AsSpan() };
    }
}

internal enum PowerShellParsedType
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
    /// PowerShell风格的选项。-ParameterName
    /// </summary>
    Option,

    /// <summary>
    /// 选项值。
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
