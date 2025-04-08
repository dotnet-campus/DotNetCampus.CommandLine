using System.Collections.Concurrent;
using System.ComponentModel;
using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Utils.Handlers;

namespace dotnetCampus.Cli;

/// <summary>
/// 辅助 <see cref="CommandLine"/> 根据已解析的命令行参数执行对应的命令处理器。
/// </summary>
public class CommandRunner
{
    private static ConcurrentDictionary<Type, VerbCreationInfo> VerbCreationInfos { get; } = [];

    private readonly CommandLine _commandLine;
    private readonly DictionaryCommandHandlerCollection _dictionaryVerbHandlers = new();
    private readonly List<ICommandHandlerCollection> _assemblyVerbHandlers = [];

    internal CommandRunner(CommandLine commandLine)
    {
        _commandLine = commandLine;
    }

    internal CommandRunner(CommandRunner commandRunner)
    {
        _commandLine = commandRunner._commandLine;
    }

    /// <summary>
    /// 供源生成器调用，注册一个专门用来处理谓词 <paramref name="verbName"/> 的命令处理器。
    /// </summary>
    /// <param name="verbName">关联的谓词。</param>
    /// <param name="creator">命令处理器的创建方法。</param>
    /// <typeparam name="T">选项类型，或命令处理器类型，或任意类型。</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Register<T>(string? verbName, Func<CommandLine, T> creator)
        where T : class, ICommandOptions
    {
        VerbCreationInfos[typeof(T)] = new VerbCreationInfo(verbName, creator);
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    public CommandRunner AddHandler<T>()
        where T : class, ICommandHandler
    {
        if (!VerbCreationInfos.TryGetValue(typeof(T), out var info))
        {
            throw new InvalidOperationException($"Handler '{typeof(T)}' is not registered. This may be a bug of the source generator.");
        }

        _dictionaryVerbHandlers.AddHandler(info.VerbName, cl => (T)info.Creator(cl));
        return this;
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    public CommandRunner AddHandler<T>(Func<T, Task<int>> handler)
        where T : class
    {
        if (!VerbCreationInfos.TryGetValue(typeof(T), out var info))
        {
            throw new InvalidOperationException($"Handler '{typeof(T)}' is not registered. This may be a bug of the source generator.");
        }

        _dictionaryVerbHandlers.AddHandler(info.VerbName, cl => new TaskCommandHandler<T>(
            () => (T)info.Creator(cl),
            handler));
        return this;
    }

    public CommandRunner AddHandlers<T>()
        where T : ICommandHandlerCollection, new()
    {
        _assemblyVerbHandlers.Add(new T());
        return this;
    }

    private ICommandHandler? MatchHandler()
    {
        var verbName = _commandLine.GuessedVerbName;

        if (_dictionaryVerbHandlers.TryMatch(verbName, _commandLine) is { } h1)
        {
            return h1;
        }

        foreach (var handler in _assemblyVerbHandlers)
        {
            if (handler.TryMatch(verbName, _commandLine) is { } h2)
            {
                return h2;
            }
        }

        return null;
    }

    public int Run()
    {
        return RunAsync().Result;
    }

    public Task<int> RunAsync()
    {
        var handler = MatchHandler();

        if (handler is null)
        {
            throw new InvalidOperationException(
                $"No command handler found for verb '{_commandLine.GuessedVerbName}'. Please ensure that the command handler is registered correctly.");
        }

        return handler.RunAsync();
    }

    private readonly record struct VerbCreationInfo(string? VerbName, Func<CommandLine, ICommandOptions> Creator);
}
