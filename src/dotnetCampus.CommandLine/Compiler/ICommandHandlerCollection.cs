namespace dotnetCampus.Cli.Compiler;

/// <summary>
/// 管理一组命令处理器的集合，在谓词匹配的情况下辅助执行对应的命令处理器。
/// </summary>
public interface ICommandHandlerCollection
{
    /// <summary>
    /// 尝试匹配一个命令处理器。
    /// </summary>
    /// <param name="verb">要匹配的谓词。</param>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <returns>匹配的命令处理器，如果没有匹配的命令处理器，则返回 <see langword="null"/>。</returns>
    ICommandHandler? TryMatch(string? verb, CommandLine commandLine);
}
