using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli;

/// <summary>
/// 辅助创建命令行执行程序。
/// </summary>
public static class CommandRunnerBuilderExtensions
{
    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    public static CommandRunner AddHandler<T>(this CommandLine commandLine)
        where T : class, ICommandHandler
    {
        return new CommandRunner(commandLine)
            .AddHandler<T>();
    }

    /// <inheritdoc cref="AddHandler{T}(CommandLine,Func{T, Task{int}})" />
    public static CommandRunner AddHandler<T>(this CommandLine commandLine, Action<T> handler)
        where T : class
    {
        return new CommandRunner(commandLine)
            .AddHandler<T>(handler);
    }

    /// <inheritdoc cref="AddHandler{T}(CommandLine,Func{T, Task{int}})" />
    public static CommandRunner AddHandler<T>(this CommandLine commandLine, Func<T, int> handler)
        where T : class
    {
        return new CommandRunner(commandLine)
            .AddHandler<T>(handler);
    }

    /// <inheritdoc cref="AddHandler{T}(CommandLine,Func{T, Task{int}})" />
    public static CommandRunner AddHandler<T>(this CommandLine commandLine, Func<T, Task> handler)
        where T : class
    {
        return new CommandRunner(commandLine)
            .AddHandler<T>(handler);
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <param name="handler">用于处理已解析的命令行参数的委托。</param>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    public static CommandRunner AddHandler<T>(this CommandLine commandLine, Func<T, Task<int>> handler)
        where T : class
    {
        return new CommandRunner(commandLine)
            .AddHandler<T>(handler);
    }

    public static CommandRunner AddHandlers<T>(this CommandLine commandLine)
        where T : ICommandHandlerCollection, new()
    {
        return new CommandRunner(commandLine)
            .AddHandlers<T>();
    }
}
