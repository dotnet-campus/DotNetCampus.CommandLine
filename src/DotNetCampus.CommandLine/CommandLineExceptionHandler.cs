using DotNetCampus.Cli.Utils.Parsers;

namespace DotNetCampus.Cli;

internal class CommandLineExceptionHandler(CommandLine commandLine, bool ignoreAllExceptions) : ICommandHandler
{
    public CommandLineParsingResult ErrorResult { get; set; }

    public Task<int> RunAsync()
    {
        if (!ignoreAllExceptions)
        {
            ErrorResult.ThrowIfError();
        }
        else
        {
            Console.WriteLine(commandLine);
        }
        return Task.FromResult(-1);
    }
}

/// <summary>
/// 辅助创建命令行异常处理器。
/// </summary>
public static class CommandLineExceptionHandlerExtensions
{
    /// <summary>
    /// 添加一个命令处理器。
    /// </summary>
    /// <param name="builder">命令行执行器构造的链式调用。</param>
    /// <param name="ignoreAllExceptions">是否忽略所有异常。</param>
    /// <returns>命令行执行器构造的链式调用。</returns>
    [Obsolete("此方法的实现正在讨论中，API 可能不稳定，请谨慎使用。")]
    public static IAsyncCommandRunnerBuilder HandleException(this ICoreCommandRunnerBuilder builder, bool ignoreAllExceptions)
    {
        return builder.AsRunner().AddFallbackHandler(c => new CommandLineExceptionHandler(c.CommandLine, ignoreAllExceptions));
    }
}
