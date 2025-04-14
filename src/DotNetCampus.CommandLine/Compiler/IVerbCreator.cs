namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 供源生成器实现，辅助命令行选项或命令处理器的创建。
/// </summary>
/// <typeparam name="T">命令行选项或命令处理器的类型。</typeparam>
public interface IVerbCreator<out T>
    where T : class
{
    /// <summary>
    /// 创建一个命令行选项或命令处理器的实例。
    /// </summary>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <returns>命令行选项或命令处理器的实例。</returns>
    T CreateInstance(CommandLine commandLine);
}
