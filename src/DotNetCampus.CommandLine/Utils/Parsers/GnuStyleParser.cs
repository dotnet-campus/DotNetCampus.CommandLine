using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

internal sealed class GnuStyleParser : ICommandLineParser
{
    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        var longOptions = new OptionDictionary(CommandLineStyle.GNU, true);
        var shortOptions = new OptionDictionary(CommandLineStyle.GNU, true);
        List<string> arguments = [];
        string? guessedVerbName = null;
        string? currentOption = null;
        bool? isInPositionalArgumentsSection = null;

        for (var i = 0; i < commandLineArguments.Count; i++)
        {
            var commandLineArgument = commandLineArguments[i];
            if (isInPositionalArgumentsSection is true)
            {
                // 确认已经进入位置参数部分（即已用 -- 分隔，后面只能是位置参数）。
                arguments.Add(commandLineArgument);
                continue;
            }

            if (commandLineArgument == "--")
            {
                // 进入位置参数部分。
                isInPositionalArgumentsSection = true;
                continue;
            }

            if (commandLineArgument.StartsWith("--"))
            {
                // 处理长选项。
                var option = commandLineArgument[2..];
                var indexOfEqual = option.IndexOf('=');
                var indexOfColon = option.IndexOf(':');
                if (option.Length <= 0 || !char.IsLetterOrDigit(option[0]) || indexOfEqual is 0 || indexOfColon is 0)
                {
                    // 选项格式无效。
                    // --= 或 --:
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 2]: {commandLineArgument}");
                }

                if (indexOfEqual < 0 && indexOfColon < 0)
                {
                    // 选项没有值，或使用空格分隔值。
                    longOptions.AddOption(option);
                    currentOption = option;
                    continue;
                }

                if (indexOfEqual > 0 && (indexOfColon < 0 || indexOfEqual < indexOfColon))
                {
                    // 选项使用等号分隔值。
                    var value = option[(indexOfEqual + 1)..];
                    option = option[..indexOfEqual];
                    longOptions.AddValue(option, value);
                    currentOption = null;
                    continue;
                }

                if (indexOfEqual < 0 && (indexOfColon > 0 || indexOfEqual > indexOfColon))
                {
                    // 选项使用冒号分隔值。
                    var value = option[(indexOfColon + 1)..];
                    option = option[..indexOfColon];
                    longOptions.AddValue(option, value);
                    currentOption = null;
                    continue;
                }

                // 未知情况。
                throw new CommandLineParseException($"Invalid option format at index [{i}]: {commandLineArgument}");
            }

            if (commandLineArgument.StartsWith('-'))
            {
                // 处理短选项。
                var option = commandLineArgument[1..];
                var indexOfEqual = option.IndexOf('=');
                var indexOfColon = option.IndexOf(':');
                if (indexOfEqual is 0 || indexOfColon is 0)
                {
                    // 选项格式无效。
                    // -= 或 -:
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 1]: {commandLineArgument}");
                }

                if (indexOfEqual < 0 && indexOfColon < 0)
                {
                    if (option.Length is 1)
                    {
                        // 选项没有值，或使用空格分隔值。
                        shortOptions.AddOption(option[0]);
                        currentOption = option;
                    }
                    else
                    {
                        // 多个短选项合并。
                        // 例如：-abc
                        foreach (var shortOption in option)
                        {
                            shortOptions.AddOption(shortOption);
                        }
                        currentOption = null;
                    }
                    continue;
                }

                if (indexOfEqual > 0 && (indexOfColon < 0 || indexOfEqual < indexOfColon))
                {
                    // 选项使用等号分隔值。
                    var value = option[(indexOfEqual + 1)..];
                    option = option[..indexOfEqual];
                    longOptions.AddValue(option, value);
                    currentOption = null;
                    continue;
                }

                if (indexOfEqual < 0 && (indexOfColon > 0 || indexOfEqual > indexOfColon))
                {
                    // 选项使用冒号分隔值。
                    var value = option[(indexOfColon + 1)..];
                    option = option[..indexOfColon];
                    longOptions.AddValue(option, value);
                    currentOption = null;
                    continue;
                }

                // 未知情况。
                throw new CommandLineParseException($"Invalid option format at index [{i}]: {commandLineArgument}");
            }

            if (currentOption is not null)
            {
                // 如果当前有选项，则将其值设置为此选项的值。
                if (longOptions.ContainsKey(currentOption))
                {
                    longOptions.AddValue(currentOption, commandLineArgument);
                    currentOption = null;
                }
                else if (shortOptions.ContainsKey(currentOption[0]))
                {
                    shortOptions.AddValue(currentOption[0], commandLineArgument);
                    currentOption = null;
                }
                continue;
            }

            // 一开始的位置参数，或在选项后面没有明确通过 -- 分隔的位置参数。
            arguments.Add(commandLineArgument);

            if (i is 0)
            {
                guessedVerbName = commandLineArgument;
            }
        }

        // 将选项转换为只读集合。
        return new CommandLineParsedResult(guessedVerbName,
            longOptions,
            shortOptions,
            arguments.ToReadOnlyList());
    }
}
