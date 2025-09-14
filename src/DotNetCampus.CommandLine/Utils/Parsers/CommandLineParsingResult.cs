namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// 命令行参数解析结果。
/// </summary>
/// <param name="ErrorMessage">如果解析失败，此处包含错误消息；否则为 <see langword="null"/>。</param>
public readonly record struct CommandLineParsingResult(string? ErrorMessage)
{
    /// <summary>
    /// 获取一个值，指示此解析是否成功。
    /// </summary>
    public bool IsSuccess => ErrorMessage is null;

    /// <summary>
    /// 创建一个表示选项未找到的解析结果。
    /// </summary>
    /// <param name="commandLine">整个命令行参数列表。</param>
    /// <param name="index">当前正在解析的参数索引。</param>
    /// <param name="commandObjectName">正在解析此参数的命令对象的名称。</param>
    /// <param name="optionName">确定没有找到的选项名称。</param>
    /// <returns>表示选项未找到的解析结果。</returns>
    public static CommandLineParsingResult OptionNotFound(CommandLine commandLine, int index,
        string commandObjectName, ReadOnlySpan<char> optionName)
    {
        var message = $"命令行对象 {commandObjectName} 不包含选项 {optionName.ToString()}。参数列表：{commandLine}，索引 {index}，参数 {commandLine.CommandLineArguments[index]}。";
        return new CommandLineParsingResult(message);
    }
}
