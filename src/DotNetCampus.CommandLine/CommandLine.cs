using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using DotNetCampus.Cli.Compiler;
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
    public CommandLineParsingOptions ParsingOptions { get; }

    /// <summary>
    /// 在特定的属性不指定时，默认应使用的大小写敏感性。
    /// </summary>
    public bool DefaultCaseSensitive => ParsingOptions.Style.CaseSensitive;

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

    /// <summary>
    /// 尝试将命令行参数转换为指定类型的实例。
    /// </summary>
    /// <typeparam name="T">要转换的类型。</typeparam>
    /// <returns>转换后的实例。</returns>
    [Pure]
#pragma warning disable CA1822
    public T As<T>() where T : notnull => throw MethodShouldBeInspected();
#pragma warning restore CA1822

    /// <summary>
    /// 尝试将命令行参数转换为指定类型的实例。
    /// </summary>
    /// <param name="factory">由拦截器传入的命令处理器创建方法。</param>
    /// <typeparam name="T">要转换的类型。</typeparam>
    /// <returns>转换后的实例。</returns>
    [Pure, EditorBrowsable(EditorBrowsableState.Never)]
    public T As<T>(CommandObjectFactory factory) where T : notnull
    {
        return (T)factory(this);
    }

    /// <summary>
    /// 输出传入的命令行参数字符串。
    /// </summary>
    /// <returns>传入的命令行参数字符串。</returns>
    [Pure]
    public override string ToString()
    {
        return string.Join(" ", CommandLineArguments.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
    }

    CommandRunner ICoreCommandRunnerBuilder.GetOrCreateRunner() => new(this);

    /// <summary>
    /// 当某个方法本应该被源生成器拦截时，却仍然被调用了，就调用此方法抛出异常。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static InvalidOperationException MethodShouldBeInspected()
    {
        return new InvalidOperationException("源生成器本应该在编译时拦截了此方法的调用。请检查编译警告，查看 DotNetCampus.CommandLine 的源生成器是否正常工作。");
    }
}
