using System.ComponentModel;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;

namespace DotNetCampus.Cli;

/// <summary>
/// 辅助 <see cref="CommandLine"/> 根据已解析的命令行参数执行对应的命令处理器。
/// </summary>
public class CommandRunner : ICommandRunnerBuilder, IAsyncCommandRunnerBuilder
{
    private readonly CommandLine _commandLine;

    private readonly SortedList<string, CommandObjectCreationInfo> _creators = new(StringLengthDescendingComparer.CaseSensitive);

    internal CommandRunner(CommandLine commandLine)
    {
        _commandLine = commandLine;
    }

    /// <inheritdoc />
    public int Run() => RunAsync().Result;

    /// <inheritdoc />
    CommandRunner ICoreCommandRunnerBuilder.GetOrCreateRunner() => this;

    /// <inheritdoc />
    public Task<int> RunAsync()
    {
        var (possibleCommandNames, creator) = MatchCreator();

        if (creator is null)
        {
            throw new CommandNameNotFoundException(
                string.IsNullOrEmpty(possibleCommandNames)
                    ? "No default command handler found. Please ensure that a default command handler is registered correctly."
                    : $"No command handler found for command '{possibleCommandNames}'. Please ensure that the command handler is registered correctly.",
                possibleCommandNames);
        }

        var handler = (ICommandHandler)creator(_commandLine);
        return handler.RunAsync();
    }

    private (string PossibleCommandNames, ExperimentalCommandObjectCreator? Creator) MatchCreator()
    {
        if (_creators.Count is 0)
        {
            return ("", null);
        }

        var maxLength = _creators.Keys[0].Length;
        var header = _commandLine.GetHeader(maxLength);
        var stringComparison = _commandLine.DefaultCaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        foreach (var (command, info) in _creators)
        {
            if (header.StartsWith(command, stringComparison)
                || info.CommandAliases.Any(alias => header.StartsWith(alias, stringComparison)))
            {
                return (header, info.Creator);
            }
        }

        return (header, null);
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="command">由拦截器传入的的命令处理器的命令，<see langword="null"/> 表示此处理器没有命令名称。</param>
    /// <param name="creator">由拦截器传入的命令处理器创建方法。</param>
    /// <param name="commandAliases">命令的别名列表，由源生成器生成，用于根据不同的命令行风格生成不同的命名法名称。</param>
    /// <returns>返回一个命令处理器构建器。</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal CommandRunner AddHandlerCore(string? command, ExperimentalCommandObjectCreator creator,
        IReadOnlyList<string>? commandAliases
    )
    {
        var isAdded = _creators.TryAdd(command ?? "", new CommandObjectCreationInfo
        {
            Creator = creator,
            CommandAliases = commandAliases ?? [],
        });
        if (!isAdded)
        {
            throw new InvalidOperationException($"The command '{command}' is already registered.");
        }
        return this;
    }

    private readonly record struct CommandObjectCreationInfo
    {
        public required ExperimentalCommandObjectCreator Creator { get; init; }

        public required IReadOnlyList<string> CommandAliases { get; init; }
    }
}

file static class CommandRunnerExtensions
{
    /// <summary>
    /// 获取命令行前几个字符组成的字符串（空格分隔），长度等于或轻微超过指定的最大长度，除非命令行本身没有那么长。
    /// </summary>
    /// <param name="commandLine">要获取前缀的命令行。</param>
    /// <param name="compareToLength">要比较的长度。</param>
    /// <returns>命令行前几个字符组成的字符串（空格分隔）。</returns>
    public static string GetHeader(this CommandLine commandLine, int compareToLength)
    {
        var args = commandLine.CommandLineArguments;
        if (args.Count is 0 || compareToLength <= 0)
        {
            return "";
        }

        int index;
        var currentLength = 0;
        for (index = 0; index < args.Count; index++)
        {
            if (index > 0)
            {
                // 加上空格的长度。
                currentLength++;
            }

            var arg = args[index];
            var length = currentLength + arg.Length;
            if (length > compareToLength)
            {
                break;
            }

            currentLength = length;
        }

        return string.Join(" ", args.Take(index + 1));
    }
}

/// <summary>
/// 按长度比较字符串的比较器。更长的字符串在排序中更靠前。
/// </summary>
/// <param name="caseSensitive"></param>
file class StringLengthDescendingComparer(bool caseSensitive) : IComparer<string>
{
    /// <summary>
    /// 区分大小写的字符串长度降序比较器。
    /// </summary>
    public static StringLengthDescendingComparer CaseSensitive { get; } = new StringLengthDescendingComparer(true);

    /// <summary>
    /// 不区分大小写的字符串长度降序比较器。
    /// </summary>
    public static StringLengthDescendingComparer CaseInsensitive { get; } = new StringLengthDescendingComparer(false);

    public int Compare(string? x, string? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }
        if (x == null)
        {
            return 1;
        }
        if (y == null)
        {
            return -1;
        }

        // 先按长度比较，长度更长的排在前面。
        var lengthComparison = y.Length.CompareTo(x.Length);
        if (lengthComparison != 0)
        {
            return lengthComparison;
        }

        // 当长度相同时，按字典序比较。
        return caseSensitive
            ? string.Compare(x, y, StringComparison.Ordinal)
            : string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
    }
}
