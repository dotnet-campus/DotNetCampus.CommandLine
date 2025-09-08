namespace DotNetCampus.Cli.Exceptions;

/// <summary>
/// 表示多个命令行参数选项、谓词或处理器被标记为匹配同一个命令行谓词的异常。
/// </summary>
public class CommandVerbAmbiguityException : CommandLineException
{
    /// <summary>
    /// 获取命令行谓词的名称。
    /// </summary>
    public string? CommandName { get; }

    /// <summary>
    /// 获取命令行谓词的名称。
    /// </summary>
    [Obsolete("请使用 CommandName 属性。")]
    public string? VerbName => CommandName;

    /// <summary>
    /// 初始化 <see cref="CommandVerbAmbiguityException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常提示信息。</param>
    /// <param name="commandName">命令行谓词的名称。</param>
    public CommandVerbAmbiguityException(string message, string? commandName)
        : base(message)
    {
        CommandName = commandName;
    }
}
