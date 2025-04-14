namespace dotnetCampus.Cli.Exceptions;

/// <summary>
/// 表示一个必须赋值的属性，没有在命令行参数中赋值的异常。
/// </summary>
public class RequiredPropertyNotAssignedException : CommandLineException
{
    /// <summary>
    /// 获取必须属性的名称。
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// 初始化 <see cref="RequiredPropertyNotAssignedException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常提示信息。</param>
    /// <param name="propertyName">必须属性的名称。</param>
    public RequiredPropertyNotAssignedException(string message, string propertyName)
        : base(message)
    {
        PropertyName = propertyName;
    }
}
