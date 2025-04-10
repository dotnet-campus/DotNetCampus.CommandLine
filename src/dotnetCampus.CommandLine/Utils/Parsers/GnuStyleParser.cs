using System.Collections.Immutable;
using dotnetCampus.Cli.Exceptions;

namespace dotnetCampus.Cli.Utils.Parsers;

internal sealed class GnuStyleParser : ICommandLineParser
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
                    longOptions.TryAdd(option, []);
                    currentOption = option;
                    continue;
                }

                if (indexOfEqual > 0 && (indexOfColon < 0 || indexOfEqual < indexOfColon))
                {
                    // 选项使用等号分隔值。
                    var value = option[(indexOfEqual + 1)..];
                    option = option[..indexOfEqual];
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
                    longOptions.TryAdd(option, []);
                    longOptions[option].Add(value);
                    currentOption = null;
                    continue;
                }

                // 未知情况。
                throw new CommandLineParseException($"Invalid option format at index [{i}]: {commandLineArgument}");
            }

            if (commandLineArgument.StartsWith('-') && commandLineArgument.Length > 1)
            {
                // 处理短选项。
                var option = commandLineArgument[1..];
                var indexOfEqual = option.IndexOf('=');
                var indexOfColon = option.IndexOf(':');
                
                // 处理带等号或冒号的短选项
                if (indexOfEqual > 0 || indexOfColon > 0)
                {
                    char optionChar = option[0];
                    string value;
                    
                    if (indexOfEqual > 0 && (indexOfColon < 0 || indexOfEqual < indexOfColon))
                    {
                        // 使用等号分隔值
                        value = option[(indexOfEqual + 1)..];
                    }
                    else
                    {
                        // 使用冒号分隔值
                        value = option[(indexOfColon + 1)..];
                    }
                    
                    shortOptions.TryAdd(optionChar, []);
                    shortOptions[optionChar].Add(value);
                    currentOption = null;
                    continue;
                }
                
                // 特殊处理单个短选项的情况
                if (option.Length == 1)
                {
                    char shortOption = option[0];
                    shortOptions.TryAdd(shortOption, []);
                    currentOption = option;
                    continue;
                }
                
                // 处理短选项后直接跟值的情况，例如 -vtest
                char firstChar = option[0];
                string restOfOption = option[1..];
                
                // 检查是否所有字符都是字母（短选项组合，如 -abc）
                bool allLetters = true;
                for (int j = 0; j < option.Length; j++)
                {
                    if (!char.IsLetter(option[j]))
                    {
                        allLetters = false;
                        break;
                    }
                }
                
                if (allLetters && option.Length <= 3) // 短选项组合，如 -abc
                {
                    // 多个短选项组合在一起，例如 -abc
                    foreach (var shortOption in option)
                    {
                        shortOptions.TryAdd(shortOption, []);
                    }
                    currentOption = null;
                }
                else
                {
                    // 短选项后直接跟值，例如 -vtest
                    shortOptions.TryAdd(firstChar, []);
                    shortOptions[firstChar].Add(restOfOption);
                    currentOption = null;
                }
                
                continue;
            }

            if (currentOption is not null)
            {
                // 如果当前有选项，则将其值设置为此选项的值。
                if (currentOption.Length == 1)
                {
                    // 短选项
                    char shortOption = currentOption[0];
                    shortOptions.TryAdd(shortOption, []);
                    shortOptions[shortOption].Add(commandLineArgument);
                }
                else
                {
                    // 长选项
                    longOptions.TryAdd(currentOption, []);
                    longOptions[currentOption].Add(commandLineArgument);
                }
                
                currentOption = null;
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
