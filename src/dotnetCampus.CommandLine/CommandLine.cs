using System.Collections.Immutable;
using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Utils;

namespace dotnetCampus.Cli;

/// <summary>
/// 为应用程序提供统一的命令行参数解析功能。
/// </summary>
public record CommandLine
{
    /// <summary>
    /// 获取此命令行解析类型所关联的命令行参数。
    /// </summary>
    public readonly ImmutableArray<string> Arguments;

    private CommandLine(ImmutableArray<string> arguments)
    {
        Arguments = arguments;
    }

    /// <summary>
    /// 解析命令行参数，并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="args">命令行参数。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    public static CommandLine Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return new CommandLine([..args]);
    }

    /// <summary>
    /// 解析一整行命令（所有参数被放在了同一个字符串中），并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="singleLineCommandLineArgs">一整行命令。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    public static CommandLine Parse(string singleLineCommandLineArgs)
    {
        var args = CommandLineConverter.SingleLineCommandLineArgsToArrayCommandLineArgs(singleLineCommandLineArgs);
        return new CommandLine(args);
    }

    public T EnsureGetOption<T>(T option)
    {
        throw new NotImplementedException();
    }

    public T EnsureGetValue<T>()
    {
        throw new NotImplementedException();
    }
}
