namespace DotNetCampus.Cli.Exceptions;

/// <summary>
/// 表示输入的命令行在匹配多个命令行参数选项、子命令或处理器时，没有任何一个匹配到的异常。
/// </summary>
public class CommandNameNotFoundException : CommandLineException
{
    /// <summary>
    /// 获取命令行的命令（主命令和子命令）。
    /// </summary>
    public IReadOnlyList<string> CommandNames { get; }

    /// <summary>
    /// 获取命令行谓词的名称。
    /// </summary>
    public string? VerbName => CommandNames.Count > 0 ? CommandNames[0] : null;

    /// <summary>
    /// 初始化 <see cref="CommandNameNotFoundException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常提示信息。</param>
    /// <param name="commandNames">已解析的命令行的命令（主命令和子命令）。</param>
    public CommandNameNotFoundException(string message, IReadOnlyList<string> commandNames)
        : base(message)
    {
        CommandNames = commandNames;
    }

    /// <summary>
    /// 初始化 <see cref="CommandNameNotFoundException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常提示信息。</param>
    /// <param name="mainCommandName">主命令名称。</param>
    public CommandNameNotFoundException(string message, string? mainCommandName)
        : base(message)
    {
        CommandNames = mainCommandName is null
            ? []
            : [mainCommandName];
    }
}
