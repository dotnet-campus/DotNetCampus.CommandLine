using System.Collections.Immutable;
using DotNetCampus.Cli.Exceptions;

namespace DotNetCampus.Cli.Utils.Parsers;

internal sealed class FlexibleStyleParser : ICommandLineParser
{
    public CommandLineParsedResult Parse(ImmutableArray<string> commandLineArguments)
    {
        Dictionary<string, List<string>> longOptions = [];
        Dictionary<char, List<string>> shortOptions = [];
        List<string> arguments = [];
        string? guessedVerbName = null;
        string? currentOption = null;
        bool? isInPositionalArgumentsSection = null;

        for (var i = 0; i < commandLineArguments.Length; i++)
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

            // 处理以 "--" 开头的选项（长选项，GNU风格）
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
                    option = NamingHelper.MakeKebabCase(option);
                    longOptions.TryAdd(option, []);
                    currentOption = option;
                    continue;
                }

                if (indexOfEqual > 0 && (indexOfColon < 0 || indexOfEqual < indexOfColon))
                {
                    // 选项使用等号分隔值。
                    var value = option[(indexOfEqual + 1)..];
                    option = option[..indexOfEqual];
                    option = NamingHelper.MakeKebabCase(option);
                    longOptions.TryAdd(option, []);
                    longOptions[option].Add(value);
                    currentOption = null;
                    continue;
                }

                if (indexOfEqual < 0 && (indexOfColon > 0 || indexOfEqual > indexOfColon))
                {
                    // 选项使用冒号分隔值。
                    var value = option[(indexOfColon + 1)..];
                    option = option[..indexOfColon];
                    option = NamingHelper.MakeKebabCase(option);
                    longOptions.TryAdd(option, []);
                    longOptions[option].Add(value);
                    currentOption = null;
                    continue;
                }

                // 未知情况。
                throw new CommandLineParseException($"Invalid option format at index [{i}]: {commandLineArgument}");
            }

            // 处理以 "-" 开头的选项（短选项或长选项，GNU风格和POSIX风格）
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
                        // 单个短选项。
                        shortOptions.TryAdd(option[0], []);
                        currentOption = option;
                    }
                    else
                    {
                        option = NamingHelper.MakeKebabCase(option);
                        // 对于灵活风格，如果不是单字符，将其视为长选项
                        longOptions.TryAdd(option, []);
                        // 同时也将多个短选项合并
                        // 例如：-abc 被解析为 -abc 长选项，也被解析为 -a -b -c 组合短选项
                        foreach (var shortOption in option)
                        {
                            shortOptions.TryAdd(shortOption, []);
                        }
                        currentOption = option;
                    }
                    continue;
                }

                if (indexOfEqual > 0 && (indexOfColon < 0 || indexOfEqual < indexOfColon))
                {
                    // 选项使用等号分隔值。
                    var value = option[(indexOfEqual + 1)..];
                    option = option[..indexOfEqual];

                    if (option.Length is 1)
                    {
                        // 短选项
                        shortOptions.TryAdd(option[0], []);
                        shortOptions[option[0]].Add(value);
                    }
                    else
                    {
                        // 长选项
                        option = NamingHelper.MakeKebabCase(option);
                        longOptions.TryAdd(option, []);
                        longOptions[option].Add(value);
                    }
                    currentOption = null;
                    continue;
                }

                if (indexOfEqual < 0 && (indexOfColon > 0 || indexOfEqual > indexOfColon))
                {
                    // 选项使用冒号分隔值。
                    var value = option[(indexOfColon + 1)..];
                    option = option[..indexOfColon];

                    if (option.Length is 1)
                    {
                        // 短选项
                        shortOptions.TryAdd(option[0], []);
                        shortOptions[option[0]].Add(value);
                    }
                    else
                    {
                        // 长选项
                        option = NamingHelper.MakeKebabCase(option);
                        longOptions.TryAdd(option, []);
                        longOptions[option].Add(value);
                    }
                    currentOption = null;
                    continue;
                }

                // 未知情况。
                throw new CommandLineParseException($"Invalid option format at index [{i}]: {commandLineArgument}");
            }

            // 处理以 "/" 开头的选项（Windows风格）
            if (commandLineArgument.StartsWith('/'))
            {
                // 处理Windows风格选项
                var option = commandLineArgument[1..];
                var indexOfEqual = option.IndexOf('=');
                var indexOfColon = option.IndexOf(':');

                if (option.Length <= 0 || !char.IsLetterOrDigit(option[0]) || indexOfEqual is 0 || indexOfColon is 0)
                {
                    // 选项格式无效。
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 1]: {commandLineArgument}");
                }

                if (indexOfEqual < 0 && indexOfColon < 0)
                {
                    // 选项没有值，或使用空格分隔值。
                    option = NamingHelper.MakeKebabCase(option);
                    longOptions.TryAdd(option, []);
                    currentOption = option;
                    continue;
                }

                if (indexOfEqual > 0 && (indexOfColon < 0 || indexOfEqual < indexOfColon))
                {
                    // 选项使用等号分隔值。
                    var value = option[(indexOfEqual + 1)..];
                    option = option[..indexOfEqual];
                    option = NamingHelper.MakeKebabCase(option);
                    longOptions.TryAdd(option, []);
                    longOptions[option].Add(value);
                    currentOption = null;
                    continue;
                }

                if (indexOfEqual < 0 && (indexOfColon > 0 || indexOfEqual > indexOfColon))
                {
                    // 选项使用冒号分隔值。
                    var value = option[(indexOfColon + 1)..];
                    option = option[..indexOfColon];
                    option = NamingHelper.MakeKebabCase(option);
                    longOptions.TryAdd(option, []);
                    longOptions[option].Add(value);
                    currentOption = null;
                    continue;
                }

                // 未知情况。
                throw new CommandLineParseException($"Invalid option format at index [{i}]: {commandLineArgument}");
            }

            if (currentOption is not null)
            {
                // 如果当前有选项，则将其值设置为此选项的值。
                if (currentOption.Length is 1 && shortOptions.TryGetValue(currentOption[0], out var shortValue))
                {
                    shortValue.Add(commandLineArgument);
                    currentOption = null;
                }
                else if (longOptions.TryGetValue(currentOption, out var longValue))
                {
                    longValue.Add(commandLineArgument);
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

        // 将选项转换为不可变集合。
        return new CommandLineParsedResult(guessedVerbName,
            longOptions.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
            shortOptions.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
            [..arguments]);
    }
}
