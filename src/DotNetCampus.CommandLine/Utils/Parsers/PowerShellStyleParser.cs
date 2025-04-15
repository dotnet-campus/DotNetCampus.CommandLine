using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <inheritdoc cref="CommandLineStyle.PowerShell"/>
internal sealed class PowerShellStyleParser : ICommandLineParser
{
    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        var longOptions = new OptionDictionary(true);
        string? guessedVerbName = null;
        List<string> arguments = [];

        OptionName? lastOption = null;
        var lastType = PowerShellParsedType.Start;

        foreach (var commandLineArgument in commandLineArguments)
        {
            var result = PowerShellArgument.Parse(lastType, commandLineArgument);
            var tempLastType = lastType;
            lastType = result.Type;

            if (result.Type is PowerShellParsedType.VerbOrPositionalArgument)
            {
                lastOption = null;
                guessedVerbName = result.Value.ToString();
                arguments.Add(guessedVerbName);
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
                    // 如果有逗号分隔的数组值
                    var valueSpan = result.Value;
                    if (valueSpan.Contains(','))
                    {
                        var arrayValues = result.Value.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var value in arrayValues)
                        {
                            longOptions.AddValue(option, value.Trim());
                        }
                    }
                    else
                    {
                        longOptions.AddValue(option, result.Value.ToString());
                    }

                    lastOption = null; // 在 PowerShell 中处理完一个值后，即完成当前选项的解析
                }
                continue;
            }

            if (result.Type is PowerShellParsedType.PositionalArgumentSeparator)
            {
                lastOption = null;
            }
        }

        return new CommandLineParsedResult(guessedVerbName,
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

    public static PowerShellArgument Parse(PowerShellParsedType lastType, string argument)
    {
        var isPostPositionalArgument = lastType is PowerShellParsedType.PositionalArgumentSeparator or PowerShellParsedType.PostPositionalArgument;

        if (!isPostPositionalArgument && argument is "--")
        {
            // 位置参数分隔符。
            return new PowerShellArgument(PowerShellParsedType.PositionalArgumentSeparator);
        }

        if (!isPostPositionalArgument && argument.StartsWith('-') && argument.Length > 1 && !char.IsDigit(argument[1]))
        {
            // PowerShell 风格的选项 (-ParameterName)
            var optionSpan = argument.AsSpan(1);
            return new PowerShellArgument(PowerShellParsedType.Option)
            {
                Option = new OptionName(OptionName.MakeKebabCase(optionSpan), Range.All),
            };
        }

        // 处理各种类型的位置参数和选项值
        if (lastType is PowerShellParsedType.Start)
        {
            // 如果是第一个参数，则视为谓词或位置参数。
            return new PowerShellArgument(PowerShellParsedType.VerbOrPositionalArgument) { Value = argument.AsSpan() };
        }

        if (lastType is PowerShellParsedType.VerbOrPositionalArgument or PowerShellParsedType.PositionalArgument)
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
            // 如果前一个已经是选项值，则当前是位置参数。
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
    /// 第一个位置参数，也可能是谓词。
    /// </summary>
    VerbOrPositionalArgument,

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
