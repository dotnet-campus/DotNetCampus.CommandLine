namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 从已解析的命令行参数创建命令数据模型或处理器的委托。
/// </summary>
/// <param name="context">命令数据模型或处理器的创建上下文。</param>
/// <returns>命令数据模型或处理器。</returns>
public delegate object CommandObjectFactory(CommandRunningContext context);

/// <summary>
/// 运行命令处理器的委托。
/// </summary>
/// <param name="state">传递给命令处理器的状态对象。</param>
/// <returns>命令处理器的返回值。</returns>
public delegate Task<int> CommandHandlerRunner(object state);

/// <summary>
/// 命令执行的上下文。
/// </summary>
public readonly struct CommandRunningContext
{
    /// <summary>
    /// 已解析的命令行参数。
    /// </summary>
    public required CommandLine CommandLine { get; init; }

    /// <summary>
    /// 运行执行器的实例。如果直接通过 <see cref="DotNetCampus.Cli.CommandLine.As{T}()"/> 方法创建执行器，则该属性为 null。
    /// </summary>
    internal CommandRunner? CommandRunner { get; init; }
}
