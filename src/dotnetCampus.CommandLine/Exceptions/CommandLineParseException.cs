namespace dotnetCampus.Cli.Exceptions;

/// <summary>
/// 在解析命令行参数的过程中发生的异常。
/// </summary>
public class CommandLineParseException : CommandLineException
{
    private const string DefaultMessage = "Parse the command line failed.";

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    public CommandLineParseException() : base(DefaultMessage)
    {
    }

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    public CommandLineParseException(string message) : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    /// <param name="innerException">内部异常。</param>
    public CommandLineParseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// 在解析命令行参数的值的过程中发生的异常。
/// </summary>
public class CommandLineParseValueException : CommandLineParseException
{
    private const string DefaultMessage = "Failed to parse the command line value.";

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    public CommandLineParseValueException() : base(DefaultMessage)
    {
    }

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    public CommandLineParseValueException(string message) : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="CommandLineException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    /// <param name="innerException">内部异常。</param>
    public CommandLineParseValueException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
