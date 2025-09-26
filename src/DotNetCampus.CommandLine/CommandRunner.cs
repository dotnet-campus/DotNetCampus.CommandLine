using System.ComponentModel;
using System.Runtime.ExceptionServices;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Handlers;
using DotNetCampus.Cli.Utils.Parsers;

namespace DotNetCampus.Cli;

/// <summary>
/// 辅助 <see cref="Cli.CommandLine"/> 根据已解析的命令行参数执行对应的命令处理器。
/// </summary>
public class CommandRunner : ICommandRunnerBuilder, IAsyncCommandRunnerBuilder
{
    private readonly SortedList<string, CommandObjectFactory> _factories;
    private readonly StringComparison _stringComparison;
    private readonly bool _supportsOrdinal;
    private readonly bool _supportsPascalCase;
    private CommandObjectFactory? _defaultFactory;
    private CommandObjectFactory? _fallbackFactory;

    internal CommandRunner(CommandLine commandLine)
    {
        CommandLine = commandLine;
        var caseSensitive = commandLine.ParsingOptions.Style.CaseSensitive;
        _factories = caseSensitive
            ? new SortedList<string, CommandObjectFactory>(StringLengthDescendingComparer.CaseSensitive)
            : new SortedList<string, CommandObjectFactory>(StringLengthDescendingComparer.CaseInsensitive);
        _stringComparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        _supportsOrdinal = commandLine.ParsingOptions.Style.NamingPolicy.SupportsOrdinal();
        _supportsPascalCase = commandLine.ParsingOptions.Style.NamingPolicy.SupportsPascalCase();
    }

    /// <summary>
    /// 要执行的命令行。
    /// </summary>
    internal CommandLine CommandLine { get; }

    /// <inheritdoc />
    public CommandRunningResult Run()
    {
        try
        {
            return RunAsync().Result;
        }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
        {
            // 当内部只有一个异常时，直接抛出这个异常，而不是 AggregateException。
            // 以便让同步方法的调用者看起来更像在用一个同步方法。
            ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
            throw;
        }
    }

    /// <summary>
    /// 处理命令解析过程中发生的错误。
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal bool RunFallback(CommandLineParsingResult result)
    {
        if (_fallbackFactory?.Invoke(CommandLine) is not ICommandHandler fallback)
        {
            return false;
        }

        if (fallback is CommandLineExceptionHandler exceptionHandler)
        {
            exceptionHandler.ErrorResult = result;
        }

        fallback.RunAsync().Wait();
        return true;
    }

    /// <inheritdoc />
    CommandRunner ICoreCommandRunnerBuilder.GetOrCreateRunner() => this;

    /// <inheritdoc />
    public async Task<CommandRunningResult> RunAsync()
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

        var handler = (ICommandHandler)factory(CommandLine);
        var exitCode = await handler.RunAsync();
        return new CommandRunningResult
        {
            ExitCode = exitCode,
            HandledBy = handler switch
            {
                IAnonymousCommandHandler anonymousHandler => anonymousHandler.CreatedCommandOptions,
                _ => handler,
            },
        };
    }

    private (string PossibleCommandNames, CommandObjectFactory? Creator) MatchCreator()
    {
        if (_factories.Count > 0)
        {
            var maxLength = _factories.Keys[0].Length;
            var header = CommandLine.GetHeader(maxLength);

            foreach (var (command, factory) in _factories)
            {
                if (header.StartsWith(command, _stringComparison))
                {
                    // 前缀已匹配成功，接下来判断这是否是命令单词边界。
                    if (header.Length == command.Length || char.IsWhiteSpace(header[command.Length]))
                    {
                        return (command, factory);
                    }
                }
            }
        }

        if (_defaultFactory is { } defaultFactory)
        {
            return ("", defaultFactory);
        }

        return (CommandLine.GetHeader(1), null);
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

    /// <summary>
    /// 添加一个回退的命令处理器。当其他命令出现了错误时，会执行此命令处理器。
    /// </summary>
    /// <param name="factory">回退命令处理器的创建方法。</param>
    /// <returns>返回一个命令处理器构建器。</returns>
    internal CommandRunner AddFallbackHandler(CommandObjectFactory factory)
    {
        _fallbackFactory = factory;
        return this;
    }
}

/// <summary>
/// 表示命令行处理器的运行结果。
/// </summary>
public readonly record struct CommandRunningResult
{
    /// <summary>
    /// 命令行处理器的退出代码。
    /// </summary>
    public required int ExitCode { get; init; }

    /// <summary>
    /// 处理此命令行的命令处理器实例。
    /// </summary>
    public object? HandledBy { get; init; }

    /// <summary>
    /// 隐式转换为退出代码。
    /// </summary>
    /// <param name="result">命令行处理结果。</param>
    /// <returns>退出代码。</returns>
    public static implicit operator int(CommandRunningResult result) => result.ExitCode;
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
