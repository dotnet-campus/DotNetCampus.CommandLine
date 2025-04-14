namespace DotNetCampus.Cli;

/// <summary>
/// 命令行执行器构造器，用于链式创建命令行执行器。
/// </summary>
public interface ICoreCommandRunnerBuilder
{
    /// <summary>
    /// 获取或创建一个命令行执行器。
    /// </summary>
    /// <returns>命令行执行器。</returns>
    internal CommandRunner GetOrCreateRunner();
}

/// <summary>
/// 命令行执行器构造器，用于链式创建命令行执行器。
/// </summary>
public interface ICommandRunnerBuilder : ICoreCommandRunnerBuilder
{
    /// <summary>
    /// 以同步方式运行命令行处理器。
    /// </summary>
    /// <returns>将被执行的命令行处理器的返回值。</returns>
    int Run();
}

/// <summary>
/// 命令行执行器构造器，用于链式创建命令行执行器。
/// </summary>
public interface IAsyncCommandRunnerBuilder : ICoreCommandRunnerBuilder
{
    /// <summary>
    /// 以异步方式运行命令行处理器。
    /// </summary>
    /// <returns>将被执行的命令行处理器的返回值。</returns>
    Task<int> RunAsync();
}
