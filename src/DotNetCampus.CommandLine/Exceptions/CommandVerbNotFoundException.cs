namespace DotNetCampus.Cli.Exceptions;

/// <summary>
/// 表示输入的命令行在匹配多个命令行参数选项、谓词或处理器时，没有任何一个匹配到的异常。
/// </summary>
public class CommandVerbNotFoundException : CommandLineException
{
    /// <summary>
    /// 获取命令行谓词的名称。
    /// </summary>
    public string? VerbName { get; }

    /// <summary>
    /// 初始化 <see cref="CommandVerbNotFoundException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常提示信息。</param>
    /// <param name="verbName">命令行谓词的名称。</param>
    public CommandVerbNotFoundException(string message, string? verbName)
        : base(message)
    {
        VerbName = verbName;
    }
}
