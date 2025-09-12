using System.Diagnostics.Contracts;
using DotNetCampus.Cli.Utils;

namespace DotNetCampus.Cli;

/// <summary>
/// 为应用程序提供统一的命令行参数解析功能。
/// </summary>
public class CommandLine : ICoreCommandRunnerBuilder
{
    /// <summary>
    /// 获取此命令行解析类型所关联的命令行参数。
    /// </summary>
    public IReadOnlyList<string> CommandLineArguments { get; }

    /// <summary>
    /// 获取解析此命令行时所使用的各种选项。
    /// </summary>
    internal CommandLineParsingOptions ParsingOptions { get; }

    /// <summary>
    /// 在特定的属性不指定时，默认应使用的大小写敏感性。
    /// </summary>
    public bool DefaultCaseSensitive => ParsingOptions.CaseSensitive;

    private CommandLine()
    {
        CommandLineArguments = [];
        ParsingOptions = CommandLineParsingOptions.Flexible;
    }

    private CommandLine(IReadOnlyList<string> arguments, CommandLineParsingOptions? parsingOptions = null)
    {
        CommandLineArguments = arguments;
        ParsingOptions = parsingOptions ?? CommandLineParsingOptions.Flexible;
    }

    /// <summary>
    /// 解析命令行参数，并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="args">命令行参数。</param>
    /// <param name="parsingOptions">以此方式解析命令行参数。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    [Pure]
    public static CommandLine Parse(IReadOnlyList<string> args, CommandLineParsingOptions? parsingOptions = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(args);
#else
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }
#endif
        return args.Count is 0
            ? new CommandLine()
            : new CommandLine(args, parsingOptions);
    }

    /// <summary>
    /// 解析一整行命令（所有参数被放在了同一个字符串中），并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="singleLineCommandLineArgs">一整行命令。</param>
    /// <param name="parsingOptions">以此方式解析命令行参数。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    [Pure]
    public static CommandLine Parse(string singleLineCommandLineArgs, CommandLineParsingOptions? parsingOptions = null)
    {
        var args = CommandLineConverter.SingleLineToList(singleLineCommandLineArgs);
        return new CommandLine(args, parsingOptions);
    }

    CommandRunner ICoreCommandRunnerBuilder.GetOrCreateRunner() => new(this);
}
