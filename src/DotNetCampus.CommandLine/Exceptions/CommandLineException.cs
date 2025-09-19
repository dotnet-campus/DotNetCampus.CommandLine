namespace DotNetCampus.Cli.Temp40.Exceptions;

/// <summary>
/// 表示命令行解析或执行过程中发生的异常。
/// </summary>
public class CommandLineException : Exception
{
    private const string DefaultMessage = "Operation failed due to an error in the command line mechanism.";

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    public CommandLineException() : base(DefaultMessage)
    {
    }

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    public CommandLineException(string message) : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    /// <param name="innerException">内部异常。</param>
    public CommandLineException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
