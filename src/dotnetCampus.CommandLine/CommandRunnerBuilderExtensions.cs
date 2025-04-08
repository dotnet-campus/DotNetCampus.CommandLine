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
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    public static CommandRunner AddHandler<T>(this CommandLine commandLine, string verbName,
        IVerbCreator<T> optionsCreator, Func<T, Task<int>> handler)
        where T : class, ICommandOptions
    {
        return new CommandRunner(commandLine)
            .AddHandler(verbName, new TaskCommandHandler<T>(commandLine, optionsCreator, handler));
    }

    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <typeparam name="T">命令处理器的类型。</typeparam>
    /// <returns>返回一个命令处理器构建器。</returns>
    public static CommandRunner AddHandler<T>(this CommandLine commandLine, string verbName,
        IVerbCreator<T> handlerCreator)
        where T : class, ICommandHandler
    {
        return new CommandRunner(commandLine)
            .AddHandler(verbName, handlerCreator);
    }

    public static CommandRunner AddHandlers<T>(this CommandLine commandLine)
        where T : ICommandHandlerCollection, new()
    {
        throw new NotImplementedException();
    }
}
