using System.Collections.Concurrent;
using System.ComponentModel;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Handlers;

namespace DotNetCampus.Cli;

/// <summary>
/// 辅助 <see cref="CommandLine"/> 根据已解析的命令行参数执行对应的命令处理器。
/// </summary>
public class CommandRunner : ICommandRunnerBuilder, IAsyncCommandRunnerBuilder
{
    private static ConcurrentDictionary<Type, CommandObjectCreationInfo> CommandObjectCreationInfos { get; } = new(
#if NET5_0_OR_GREATER
        ReferenceEqualityComparer.Instance
#endif
    );

    private readonly CommandLine _commandLine;
    private readonly DictionaryCommandHandlerCollection _dictionaryCommandHandlers = new();
    private readonly ConcurrentDictionary<ICommandHandlerCollection, ICommandHandlerCollection> _assemblyCommandHandlers = [];

    internal CommandRunner(CommandLine commandLine)
    {
        _commandLine = commandLine;
    }

    internal CommandRunner(CommandRunner commandRunner)
    {
        _commandLine = commandRunner._commandLine;
    }

    /// <summary>
    /// 供源生成器调用，注册一个专门用来处理主命令（Main Command）或子命令/多级子命令（Sub Command）的命令处理器。
    /// </summary>
    /// <param name="commandNames">关联的命令。</param>
    /// <param name="creator">命令处理器的创建方法。</param>
    /// <typeparam name="T">选项类型，或命令处理器类型，或任意类型。</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Register<T>(string? commandNames, CommandObjectCreator creator)
        where T : class
    {
        CommandObjectCreationInfos[typeof(T)] = new CommandObjectCreationInfo(commandNames, creator);
    }

    /// <summary>
    /// 创建一个命令处理器实例。
    /// </summary>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令处理器实例。</returns>
    internal static T CreateInstance<T>(CommandLine commandLine)
    {
        if (!CommandObjectCreationInfos.TryGetValue(typeof(T), out var info))
        {
            throw new InvalidOperationException($"Handler '{typeof(T)}' is not registered. This may be a bug of the source generator.");
        }

        return (T)info.Creator(commandLine);
    }

    /// <summary>
    /// 创建一个命令处理器实例。
    /// </summary>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <param name="creator">命令处理器的创建方法。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>命令处理器实例。</returns>
    internal static T CreateInstance<T>(CommandLine commandLine, CommandObjectCreator creator)
    {
        return (T)creator(commandLine);
    }

    CommandRunner ICoreCommandRunnerBuilder.GetOrCreateRunner() => this;

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    internal CommandRunner AddHandler<T>()
        where T : class, ICommandHandler
    {
        if (!CommandObjectCreationInfos.TryGetValue(typeof(T), out var info))
        {
            throw new InvalidOperationException($"Handler '{typeof(T)}' is not registered. This may be a bug of the source generator.");
        }

        _dictionaryCommandHandlers.AddHandler(info.CommandNames, cl => (T)info.Creator(cl));
        return this;
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="command">由拦截器传入的的命令处理器的命令。</param>
    /// <param name="creator">由拦截器传入的命令处理器创建方法。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal CommandRunner AddHandler<T>(string? command, CommandObjectCreator creator)
        where T : class, ICommandHandler
    {
        _dictionaryCommandHandlers.AddHandler(command, creator);
        return this;
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    internal CommandRunner AddHandler<T>(Func<T, Task<int>> handler)
        where T : class
    {
        if (!CommandObjectCreationInfos.TryGetValue(typeof(T), out var info))
        {
            throw new InvalidOperationException($"Handler '{typeof(T)}' is not registered. This may be a bug of the source generator.");
        }

        _dictionaryCommandHandlers.AddHandler(info.CommandNames, cl => new TaskCommandHandler<T>(
            () => (T)info.Creator(cl),
            handler));
        return this;
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="command">由拦截器传入的的命令处理器的命令。</param>
    /// <param name="creator">由拦截器传入的命令处理器创建方法。</param>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    internal CommandRunner AddHandler<T>(string? command, CommandObjectCreator creator, Func<T, Task<int>> handler)
        where T : class
    {
        _dictionaryCommandHandlers.AddHandler(command, cl => new TaskCommandHandler<T>(
            () => (T)creator(cl),
            handler));
        return this;
    }

    internal CommandRunner AddHandlers<T>()
        where T : ICommandHandlerCollection, new()
    {
        var c = new T();
        _assemblyCommandHandlers.TryAdd(c, c);
        return this;
    }

    private ICommandHandler? MatchHandler()
    {
        var guessedCommandNames = _commandLine.PossibleCommandNames;

        // 优先寻找单独添加的处理器。
        if (_dictionaryCommandHandlers.TryMatch(guessedCommandNames, _commandLine) is { } h1)
        {
            return h1;
        }

        // 其次寻找程序集中自动搜集到的处理器。
        foreach (var handler in _assemblyCommandHandlers)
        {
            if (handler.Value.TryMatch(guessedCommandNames, _commandLine) is { } h2)
            {
                return h2;
            }
        }

        // 如果没有找到，那么很可能此命令没有谓词，需要使用默认的处理器。
        if (_dictionaryCommandHandlers.TryMatch("", _commandLine) is { } h3)
        {
            return h3;
        }
        foreach (var handler in _assemblyCommandHandlers)
        {
            if (handler.Value.TryMatch("", _commandLine) is { } h4)
            {
                return h4;
            }
        }

        // 如果连默认的处理器都没有找到，说明根本没有能处理此命令的处理器。
        return null;
    }

    /// <inheritdoc />
    public int Run()
    {
        return RunAsync().Result;
    }

    /// <inheritdoc />
    public Task<int> RunAsync()
    {
        var handler = MatchHandler();

        if (handler is null)
        {
            throw new CommandNameNotFoundException(
                $"No command handler found for command '{_commandLine.PossibleCommandNames}'. Please ensure that the command handler is registered correctly.",
                _commandLine.PossibleCommandNames);
        }

        return handler.RunAsync();
    }

    private readonly record struct CommandObjectCreationInfo(string? CommandNames, CommandObjectCreator Creator);
}
