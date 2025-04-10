using System.Collections.Immutable;
using dotnetCampus.Cli.Exceptions;

namespace dotnetCampus.Cli.Utils.Parsers;

internal sealed class PowerShellStyleParser : ICommandLineParser
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

            if (commandLineArgument.StartsWith("-") && commandLineArgument.Length > 1 && !char.IsDigit(commandLineArgument[1]))
            {
                // 处理 PowerShell 风格的选项 (-ParameterName)
                var option = commandLineArgument[1..];
                
                // PowerShell 参数不使用等号或冒号，而是用空格分隔
                // 将其作为长选项处理
                longOptions.TryAdd(option, []);
                currentOption = option;
                continue;
            }

            if (currentOption is not null)
            {
                // 如果当前有选项，则将其值设置为此选项的值。
                if (longOptions.TryGetValue(currentOption, out var longValue))
                {
                    // 检查是否有逗号分隔的数组
                    if (commandLineArgument.Contains(','))
                    {
                        // 按逗号拆分值并添加到选项值列表
                        var arrayValues = commandLineArgument.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var value in arrayValues)
                        {
                            longValue.Add(value.Trim());
                        }
                    }
                    else
                    {
                        longValue.Add(commandLineArgument);
                    }
                    currentOption = null; // 在PowerShell中处理完一个值后，即完成当前选项的解析
                }
                else if (shortOptions.TryGetValue(currentOption[0], out List<string>? shortValue))
                {
                    // 检查是否有逗号分隔的数组
                    if (commandLineArgument.Contains(','))
                    {
                        // 按逗号拆分值并添加到选项值列表
                        var arrayValues = commandLineArgument.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var value in arrayValues)
                        {
                            shortValue.Add(value.Trim());
                        }
                    }
                    else
                    {
                        shortValue.Add(commandLineArgument);
                    }
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
