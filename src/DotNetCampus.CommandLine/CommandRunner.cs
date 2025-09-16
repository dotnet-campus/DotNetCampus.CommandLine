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
    private readonly SortedList<string, CommandObjectFactory> _factories;
    private readonly bool _supportsOrdinal;
    private readonly bool _supportsPascalCase;
    private CommandObjectFactory? _defaultFactory;

    internal CommandRunner(CommandLine commandLine)
    {
        _commandLine = commandLine;
        _factories = commandLine.DefaultCaseSensitive
            ? new SortedList<string, CommandObjectFactory>(StringLengthDescendingComparer.CaseSensitive)
            : new SortedList<string, CommandObjectFactory>(StringLengthDescendingComparer.CaseInsensitive);
        _supportsOrdinal = commandLine.ParsingOptions.Style.NamingPolicy.SupportsOrdinal();
        _supportsPascalCase = commandLine.ParsingOptions.Style.NamingPolicy.SupportsPascalCase();
    }

    /// <inheritdoc />
    public int Run() => RunAsync().Result;

    /// <inheritdoc />
    CommandRunner ICoreCommandRunnerBuilder.GetOrCreateRunner() => this;

    /// <inheritdoc />
    public Task<int> RunAsync()
    {
        var (possibleCommandNames, factory) = MatchCreator();

        if (factory is null)
        {
            throw new CommandNameNotFoundException(
                string.IsNullOrEmpty(possibleCommandNames)
                    ? "No command handler found. Please ensure that at least one command handler is registered by AddHandler()."
                    : $"No command handler found for command '{possibleCommandNames}'. Please ensure that the command handler is registered by AddHandler().",
                possibleCommandNames);
        }

        var handler = (ICommandHandler)factory(_commandLine);
        return handler.RunAsync();
    }

    private (string PossibleCommandNames, CommandObjectFactory? Creator) MatchCreator()
    {
        if (_factories.Count > 0)
        {
            var maxLength = _factories.Keys[0].Length;
            var header = _commandLine.GetHeader(maxLength);
            var stringComparison = _commandLine.DefaultCaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            foreach (var (command, factory) in _factories)
            {
                if (header.StartsWith(command, stringComparison))
                {
                    return (header, factory);
                }
            }
        }

        if (_defaultFactory is { } defaultFactory)
        {
            return ("", defaultFactory);
        }

        return (_commandLine.GetHeader(1), null);
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="command">由拦截器传入的的命令处理器的命令，<see langword="default"/> 表示此处理器没有命令名称。</param>
    /// <param name="factory">由拦截器传入的命令处理器创建方法。</param>
    /// <returns>返回一个命令处理器构建器。</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal CommandRunner AddHandlerCore(NamingPolicyNameGroup command, CommandObjectFactory factory)
    {
        if (_supportsOrdinal)
        {
            if (command.Ordinal is { } ordinal && !string.IsNullOrWhiteSpace(ordinal))
            {
                // 包含命令名称。
                var isAdded = _factories.TryAdd(ordinal, factory);
                if (!isAdded)
                {
                    throw new CommandNameAmbiguityException($"The command '{ordinal}' is already registered.", ordinal);
                }
            }
            else
            {
                // 不包含命令名称，表示这是默认命令。
                if (_defaultFactory is not null)
                {
                    throw new CommandNameAmbiguityException("The default command handler is already registered.", null);
                }
                _defaultFactory = factory;
            }
        }
        if (_supportsPascalCase)
        {
            if (command.PascalCase is { } pascal && !string.IsNullOrWhiteSpace(pascal))
            {
                // 包含命令名称。
                var isAdded = _factories.TryAdd(pascal, factory);
                if (!isAdded && !_supportsOrdinal)
                {
                    // 转换的名称，之后在仅用转换名称时才需要抛出异常；否则很可能前面已经添加了一个相同的名称。
                    throw new CommandNameAmbiguityException($"The command '{pascal}' is already registered.", pascal);
                }
            }
            else
            {
                // 不包含命令名称，表示这是默认命令。
                if (_defaultFactory is not null && !_supportsOrdinal)
                {
                    // 如果支持双命名法，则允许前面已经注册了一个默认命令。
                    throw new CommandNameAmbiguityException("The default command handler is already registered.", null);
                }
                _defaultFactory = factory;
            }
        }
        return this;
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
