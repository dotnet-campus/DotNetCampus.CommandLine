namespace DotNetCampus.Cli.Exceptions;

/// <summary>
/// 表示多个命令行参数选项、处理器被标记为匹配同一个命令行子命令的异常。
/// </summary>
public class CommandNameAmbiguityException : CommandLineException
{
    /// <summary>
    /// 获取命令行的命令名称。
    /// </summary>
    public string? CommandName { get; }

    /// <summary>
    /// 获取命令行的命令名称。
    /// </summary>
    [Obsolete("请使用 CommandName 属性。")]
    public string? VerbName => CommandName;

    /// <summary>
    /// 初始化 <see cref="CommandNameAmbiguityException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常提示信息。</param>
    /// <param name="commandName">命令行的命令名称。</param>
    public CommandNameAmbiguityException(string message, string? commandName)
        : base(message)
    {
        CommandName = commandName;
    }
}
