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
    CommandRunningResult Run();
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
    Task<CommandRunningResult> RunAsync();
}

/// <summary>
/// 命令行执行器构造器，用于链式创建命令行执行器。
/// </summary>
public interface IStatedAsyncCommandRunnerBuilder
{
    /// <summary>
    /// 为命令处理器提供一个状态对象。
    /// </summary>
    /// <param name="state">状态对象。</param>
    /// <typeparam name="TState">状态对象的类型。</typeparam>
    /// <returns>供链式调用的命令行执行器构造器。</returns>
    IAsyncCommandRunnerBuilder WithState<TState>(TState state);
}
