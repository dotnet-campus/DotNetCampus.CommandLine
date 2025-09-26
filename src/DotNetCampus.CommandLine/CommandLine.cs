using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Utils;

namespace DotNetCampus.Cli;

/// <summary>
/// 为应用程序提供统一的命令行参数解析功能。
/// </summary>
public class CommandLine
{
    /// <summary>
    /// 存储特殊处理过 URL 的命令行参数。
    /// </summary>
    private readonly IReadOnlyList<string>? _urlNormalizedArguments;

    /// <summary>
    /// 获取此命令行解析类型所关联的命令行参数。
    /// </summary>
    /// <remarks>
    /// 如果命令行参数中传入的是 URL，则此参数不会保存原始的 URL，而是将 URL 转换为普通的命令行参数列表。
    /// </remarks>
    public IReadOnlyList<string> CommandLineArguments => _urlNormalizedArguments ?? RawArguments;

    /// <summary>
    /// 获取命令行传入的原始参数列表。
    /// </summary>
    public IReadOnlyList<string> RawArguments { get; }

    /// <summary>
    /// 获取解析此命令行时所使用的各种选项。
    /// </summary>
    public CommandLineParsingOptions ParsingOptions { get; }

    /// <summary>
    /// 如果此命令行是从 Web 请求的 URL 中解析出来的，则此属性保存 URL 的 Scheme 部分。
    /// </summary>
    internal string? MatchedUrlScheme { get; }

    private CommandLine()
    {
        RawArguments = [];
        ParsingOptions = CommandLineParsingOptions.Flexible;
    }

    private CommandLine(IReadOnlyList<string> arguments, CommandLineParsingOptions? parsingOptions = null)
    {
        RawArguments = arguments;
        ParsingOptions = parsingOptions ?? CommandLineParsingOptions.Flexible;
        (MatchedUrlScheme, _urlNormalizedArguments) = CommandLineConverter.TryNormalizeUrlArguments(arguments, ParsingOptions);
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
        return (T)factory(new CommandRunningContext { CommandLine = this });
    }

    /// <summary>
    /// 输出传入的命令行参数字符串。如果命令行参数中传入的是 URL，此方法会将 URL 转换为普通的命令行参数再输出。
    /// </summary>
    /// <returns>传入的命令行参数字符串。</returns>
    [Pure]
    public override string ToString()
    {
        return string.Join(" ", CommandLineArguments.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
    }

    /// <summary>
    /// 输出原始版本的传入的命令行参数字符串。如果命令行参数中传入的是 URL，此方法会原样输出 URL。
    /// </summary>
    /// <returns>原始传入的命令行参数字符串。</returns>
    [Pure]
    public string ToRawString()
    {
        return string.Join(" ", RawArguments.Select(x => x.Contains(' ') ? $"\"{x}\"" : x));
    }

    /// <summary>
    /// 创建一个命令行执行器，以支持根据命令自动选择命令处理器运行。<br/>
    /// 创建后，可通过 AddHandler 方法添加多个命令处理器。
    /// </summary>
    /// <returns>命令行执行器。</returns>
    [Pure]
    public ICommandRunnerBuilder ToRunner() => new CommandRunner(this);

    /// <summary>
    /// 当某个方法本应该被源生成器拦截时，却仍然被调用了，就调用此方法抛出异常。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static InvalidOperationException MethodShouldBeInspected()
    {
        return new InvalidOperationException("源生成器本应该在编译时拦截了此方法的调用。请检查编译警告，查看 DotNetCampus.CommandLine 的源生成器是否正常工作。");
    }
}

#pragma warning disable CS1591
[Obsolete("此类型仅供辅助升级代码用。", true)]
public static class CommandLineExtensions
{
    [Obsolete("请在调用本方法前先调用 ToRunner() 方法，以确保对象可被正确垃圾回收。", true)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this CommandLine builder) => throw MethodShouldBeInspected();

    [Obsolete("请在调用本方法前先调用 ToRunner() 方法，以确保对象可被正确垃圾回收。", true)]
    public static ICommandRunnerBuilder AddHandler<T>(this CommandLine builder, Action<T> handler) => throw MethodShouldBeInspected();

    [Obsolete("请在调用本方法前先调用 ToRunner() 方法，以确保对象可被正确垃圾回收。", true)]
    public static ICommandRunnerBuilder AddHandler<T>(this CommandLine builder, Func<T, int> handler) => throw MethodShouldBeInspected();

    [Obsolete("请在调用本方法前先调用 ToRunner() 方法，以确保对象可被正确垃圾回收。", true)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this CommandLine builder, Func<T, Task> handler) => throw MethodShouldBeInspected();

    [Obsolete("请在调用本方法前先调用 ToRunner() 方法，以确保对象可被正确垃圾回收。", true)]
    public static IAsyncCommandRunnerBuilder AddHandler<T>(this CommandLine builder, Func<T, Task<int>> handler) => throw MethodShouldBeInspected();

    private static NotSupportedException MethodShouldBeInspected() => new("在本调用前先调用 ToRunner() 方法。");
}
#pragma warning restore CS1591
