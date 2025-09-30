namespace DotNetCampus.Cli;

/// <summary>
/// 表示一个可以接收命令行参数的对象。
/// </summary>
public interface ICommandObject;

/// <summary>
/// 表示可以接收命令行参数，然后带着额外状态处理一条命令。
/// </summary>
public interface IStatedCommandHandler : ICommandObject
#pragma warning disable CS0618 // 类型或成员已过时
    , ICommandOptions
#pragma warning restore CS0618 // 类型或成员已过时
{
}

/// <summary>
/// 表示可以接收命令行参数，然后处理一条命令。
/// </summary>
public interface ICommandHandler : ICommandObject
#pragma warning disable CS0618 // 类型或成员已过时
    , ICommandOptions
#pragma warning restore CS0618 // 类型或成员已过时
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

/// <summary>
/// 表示一个可以接收命令行参数的对象。
/// </summary>
[Obsolete("已重命名为 ICommandObject")]
public interface ICommandOptions;
