namespace dotnetCampus.Cli;

/// <summary>
/// 表示可以处理一条命令。
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// 处理一条命令。
    /// </summary>
    /// <returns>返回处理结果。</returns>
    Task<int> RunAsync();
}
