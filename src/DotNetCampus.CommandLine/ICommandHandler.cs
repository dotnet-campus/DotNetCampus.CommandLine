namespace DotNetCampus.Cli;

/// <summary>
/// 表示一个可以接收命令行参数的对象。
/// </summary>
public interface ICommandOptions
{
}

/// <summary>
/// 表示可以接收命令行参数，然后带着额外状态处理一条命令。
/// </summary>
public interface IStatedCommandHandler : ICommandOptions
{
}

/// <summary>
/// 表示可以接收命令行参数，然后处理一条命令。
/// </summary>
public interface ICommandHandler : ICommandOptions
{
    /// <summary>
    /// 处理一条命令。
    /// </summary>
    /// <returns>返回处理结果。</returns>
    Task<int> RunAsync();
}

/// <summary>
/// 表示可以接收命令行参数，然后带着额外状态处理一条命令。
/// </summary>
public interface ICommandHandler<in T> : IStatedCommandHandler
{
    /// <summary>
    /// 处理一条命令。
    /// </summary>
    /// <param name="state">额外状态。</param>
    /// <returns>返回处理结果。</returns>
    Task<int> RunAsync(T state);
}
