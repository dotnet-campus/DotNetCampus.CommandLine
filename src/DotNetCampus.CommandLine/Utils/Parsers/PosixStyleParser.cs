using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

internal sealed class PosixStyleParser : ICommandLineParser
{
    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        var shortOptions = new OptionDictionary(CommandLineStyle.Posix, true);
        List<string> arguments = [];
        string? guessedVerbName = null;
        char? currentShortOption = null;
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

            // 处理长选项，POSIX风格不支持--option形式
            if (commandLineArgument.StartsWith("--"))
            {
                // POSIX风格不支持长选项，抛出异常
                throw new CommandLineParseException($"Long options (starting with '--') are not supported in POSIX style: {commandLineArgument}");
            }

            // 处理短选项
            if (commandLineArgument.StartsWith('-'))
            {
                var option = commandLineArgument[1..];
                if (option.Length <= 0)
                {
                    // 选项格式无效。
                    throw new CommandLineParseException($"Invalid option format at index [{i}, 1]: {commandLineArgument}");
                }
                // POSIX风格中，短选项可以组合（如 -abc），每个字符是一个选项
                foreach (var shortOption in option)
                {
                    shortOptions.AddOption(shortOption);
                }
                // 在POSIX风格中，组合短选项的最后一个选项不能带参数
                if (option.Length > 1)
                {
                    // 如果当前是组合短选项，并且下一个参数不是选项，则说明尝试为组合短选项提供参数，这在POSIX风格中是不允许的
                    if (i + 1 < commandLineArguments.Count && !commandLineArguments[i + 1].StartsWith('-'))
                    {
                        throw new CommandLineParseException(
                            $"Combined short options cannot have parameters in POSIX style: {commandLineArgument} {commandLineArguments[i + 1]}");
                    }
                    currentShortOption = null;
                }
                else
                {
                    // 只有单个短选项时可以带参数
                    currentShortOption = option[0];
                }
                continue;
            }
            // 处理选项参数
            if (currentShortOption != null)
            {
                // 如果存在短选项，将参数添加到最后一个短选项
                shortOptions.UpdateValue(currentShortOption.Value, commandLineArgument);
                currentShortOption = null;
                continue;
            }

            // 处理位置参数
            arguments.Add(commandLineArgument);

            if (i is 0)
            {
                guessedVerbName = commandLineArgument;
            }
        }

        // 将选项转换为只读集合。
        return new CommandLineParsedResult(guessedVerbName,
            // POSIX 风格不支持长选项，所以直接使用空字典。
            OptionDictionary.Empty,
            shortOptions,
            arguments.ToReadOnlyList());
    }
}
