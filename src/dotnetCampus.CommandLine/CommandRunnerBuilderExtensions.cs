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
    public static CommandRunner AddHandler<T>(this CommandLine commandLine)
        where T : ICommandHandler
    {
        var builder = new CommandRunner(commandLine);
        builder.AddHandler<T>();
        return builder;
    }

    public static CommandRunner AddHandlers<T>(this CommandLine commandLine)
        where T : ICommandHandlerCollection, new()
    {
        throw new NotImplementedException();
    }
}
