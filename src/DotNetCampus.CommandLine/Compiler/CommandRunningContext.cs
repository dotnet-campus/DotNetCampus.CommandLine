namespace DotNetCampus.Cli.Compiler;

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
