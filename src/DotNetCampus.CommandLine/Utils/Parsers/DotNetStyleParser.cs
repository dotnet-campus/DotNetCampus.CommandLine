using System.Collections.Immutable;
using DotNetCampus.Cli.Exceptions;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// .NET CLI风格命令行参数解析器：
/// 1. 短选项形式为 -参数:值
/// 2. 长选项可以是 --参数:值
/// 3. 也支持斜杠前缀 /参数:值
/// </summary>
internal sealed class DotNetStyleParser : ICommandLineParser
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

            // 处理长选项（--option:value 或 --option）
            if (commandLineArgument.StartsWith("--"))
            {
                var option = commandLineArgument[2..];
                var indexOfColon = option.IndexOf(':');
                
                if (option.Length <= 0 || !char.IsLetterOrDigit(option[0]) || indexOfColon is 0)
                {
                    // 选项格式无效。
                    // --: 无效
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 2]: {commandLineArgument}");
                }

                if (indexOfColon < 0)
                {
                    // 选项没有值，可能是布尔选项 (--option) 或者下一个参数是值
                    longOptions.TryAdd(option, []);
                    currentOption = option;
                    continue;
                }
                else
                {
                    // 选项使用冒号分隔值 (--option:value)
                    var value = option[(indexOfColon + 1)..];
                    option = option[..indexOfColon];
                    longOptions.TryAdd(option, []);
                    longOptions[option].Add(value);
                    currentOption = null;
                    continue;
                }
            }
            // 处理短选项（-option:value 或 -option）
            else if (commandLineArgument.StartsWith('-'))
            {
                var option = commandLineArgument[1..];
                var indexOfColon = option.IndexOf(':');
                
                if (option.Length <= 0 || indexOfColon is 0)
                {
                    // 选项格式无效。
                    // -: 无效
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 1]: {commandLineArgument}");
                }

                if (indexOfColon < 0)
                {
                    // DotNet风格不支持合并短选项，所以整个都视为一个选项名
                    longOptions.TryAdd(option, []);
                    currentOption = option;
                    continue;
                }
                else
                {
                    // 选项使用冒号分隔值 (-option:value)
                    var value = option[(indexOfColon + 1)..];
                    option = option[..indexOfColon];
                    
                    // 检查是否为单字符选项
                    if (option.Length == 1 && char.IsLetterOrDigit(option[0]))
                    {
                        shortOptions.TryAdd(option[0], []);
                        shortOptions[option[0]].Add(value);
                    }
                    else
                    {
                        longOptions.TryAdd(option, []);
                        longOptions[option].Add(value);
                    }
                    currentOption = null;
                    continue;
                }
            }
            // 处理斜杠前缀选项（/option:value 或 /option）
            else if (commandLineArgument.StartsWith('/'))
            {
                var option = commandLineArgument[1..];
                var indexOfColon = option.IndexOf(':');
                
                if (option.Length <= 0 || indexOfColon is 0)
                {
                    // 选项格式无效。
                    // /: 无效
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 1]: {commandLineArgument}");
                }

                if (indexOfColon < 0)
                {
                    // 选项没有值，可能是布尔选项 (/option) 或者下一个参数是值
                    longOptions.TryAdd(option, []);
                    currentOption = option;
                    continue;
                }
                else
                {
                    // 选项使用冒号分隔值 (/option:value)
                    var value = option[(indexOfColon + 1)..];
                    option = option[..indexOfColon];
                    
                    // 对于斜杠风格，通常都视为长选项
                    longOptions.TryAdd(option, []);
                    longOptions[option].Add(value);
                    currentOption = null;
                    continue;
                }
            }

            if (currentOption is not null)
            {
                // 如果当前有选项，则将其值设置为此选项的值。
                if (currentOption.Length == 1 && char.IsLetterOrDigit(currentOption[0]))
                {
                    shortOptions.TryAdd(currentOption[0], []);
                    shortOptions[currentOption[0]].Add(commandLineArgument);
                }
                else
                {
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
