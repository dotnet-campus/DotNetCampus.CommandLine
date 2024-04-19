#if NETCOREAPP3_0_OR_GREATER
#define SUPPORT_SPAN
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using dotnetCampus.Cli.Compatibility;
using dotnetCampus.Cli.Core;
using dotnetCampus.Cli.StateMachine;

using static dotnetCampus.Cli.Utils.CommandLineHelpers;

using ListGroupItem = System.Collections.Generic.KeyValuePair<string, dotnetCampus.Cli.Core.SingleOptimizedStrings?>;

#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA1825 // Avoid zero-length array allocations.

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 为应用程序提供统一的命令行参数解析功能。
    /// <para>支持的命令行格式风格有 Windows 和 Linux 的主流风格，以及从 URL 协议关联传入的命令行。</para>
    /// <para>当使用 <see cref="Parse(string[], ResourceManager)"/> 方法生成此类型的实例后，无论是哪一种命令行风格，都会转换为标准的 PascalCase 风格的命令行选项。</para>
    /// </summary>
    [DebuggerDisplay("CommandLine: {DebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(CommandLineDebugView))]
    public sealed class CommandLine : ICommandLineHandlerBuilder, IEnumerable<ListGroupItem>
    {
        private readonly ListGroup<SingleOptimizedStrings> _optionArgs;
        private readonly List<CommandLineFilterMatch> _filterCreatorList = new List<CommandLineFilterMatch>();
        private readonly List<CommandLineTypeMatcher<Task<int>>> _toMatchList = new List<CommandLineTypeMatcher<Task<int>>>();

        private CommandLine(ListGroup<SingleOptimizedStrings> optionArgs, ResourceManager? resourceManager)
        {
            _optionArgs = optionArgs ?? throw new ArgumentNullException(nameof(optionArgs));
            ResourceManager = resourceManager;
        }

        /// <summary>
        /// 获取本地化字符串的资源管理器。
        /// </summary>
        internal ResourceManager? ResourceManager { get; }

        /// <summary>
        /// 收集的过滤器。
        /// </summary>
        internal IReadOnlyList<CommandLineFilterMatch> FilterMatchList => _filterCreatorList;

        /// <summary>
        /// 收集的谓词处理方法。
        /// </summary>
        internal IReadOnlyList<CommandLineTypeMatcher<Task<int>>> VerbMatchList => _toMatchList;

        /// <summary>
        /// 自动查找命令行类型 <typeparamref name="T"/> 的解析器，然后解析出参数 <typeparamref name="T"/> 的一个新实例。
        /// <para />
        /// 注意，自动查找解析器是一个耗时的操作，所需时间大约是直接解析的三倍多，所以建议使用 <see cref="As{T}(ICommandLineOptionParser{T})"/> 重载。
        /// </summary>
        /// <typeparam name="T">解析出来的参数实例。</typeparam>
        /// <returns>命令行参数的新实例。</returns>
        /// <exception cref="NotSupportedException">当无法自动查找到 <typeparamref name="T"/> 类型的解析器时引发。</exception>
        [Pure]
        public T As<T>()
        {
            var parser = FindParser<T>();
            return As(parser);
        }

        /// <summary>
        /// 使用指定的命令行参数解析器 <paramref name="parser"/> 解析出参数 <typeparamref name="T"/> 的一个新实例。
        /// </summary>
        /// <typeparam name="T">解析出来的参数实例。</typeparam>
        /// <param name="parser">用于解析 <typeparamref name="T"/> 的解析器实例，此类型通常会在编译期间自动生成。</param>
        /// <returns>命令行参数的新实例。</returns>
        [Pure]
        public T As<T>(ICommandLineOptionParser<T> parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            foreach (var optionValue in _optionArgs)
            {
                var option = optionValue.Key;
                var values = optionValue.Value;
                var valueCount = values?.Count ?? 0;
                if (option.Length == 0)
                {
                    // 没有选项，只有值。
                    // 包括此 if 分支之外的任何情况下，值都需要保持传入时的大小写。
                    if (valueCount > 0)
                    {
                        if (parser.Verb is null)
                        {
                            parser.SetValue(values!);
                        }
                        else
                        {
                            parser.SetValue(values!.Skip(1).ToList());
                        }
                    }
                }
                else if (option.Length == 1)
                {
                    // 短名称。
                    var shortName = option[0];
                    if (parser is IRawCommandLineOptionParser<T> rawParser)
                    {
                        rawParser.SetValue(shortName, values);
                    }
                    else
                    {
                        if (valueCount == 0)
                        {
                            parser.SetValue(shortName, true);
                        }
                        else if (valueCount == 1)
                        {
                            if (bool.TryParse(values![0], out var @bool))
                            {
                                parser.SetValue(shortName, @bool);
                            }
                            else
                            {
                                parser.SetValue(shortName, values[0]);
                            }
                        }
                        else
                        {
                            parser.SetValue(shortName, values!);
                        }
                    }
                }
                else
                {
                    // 长名称。
                    var longName = option;
                    if (parser is IRawCommandLineOptionParser<T> rawParser)
                    {
                        rawParser.SetValue(longName, values);
                    }
                    else
                    {
                        if (valueCount == 0)
                        {
                            parser.SetValue(longName, true);
                        }
                        else if (valueCount == 1)
                        {
                            if (bool.TryParse(values![0], out var @bool))
                            {
                                parser.SetValue(longName, @bool);
                            }
                            else
                            {
                                parser.SetValue(longName, values[0]);
                            }
                        }
                        else
                        {
                            parser.SetValue(longName, values!);
                        }
                    }
                }
            }

            return parser.Commit();
        }

        internal void AddMatch(CommandLineFilterMatch filterCreator)
            => _filterCreatorList.Add(filterCreator);

        internal void AddMatch<TVerb>(CommandLineTypeMatcher<Task<int>> matcher)
            => _toMatchList.Add(matcher);

        //internal void AddMatch<TVerb>(Func<string?, CommandLineTypeMatchResult<Task<int>>> match)
        //    => _toMatchList.Add(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb), match));

        /// <summary>
        /// 将命令行参数转换为字符串值的字典。Key 为选项，Value 为选项后面的值。
        /// 对于布尔类型，Value 为空字符串；对于字符串集合，Value 为此集合拼接的字符串。
        /// </summary>
        public Dictionary<string, string> ToDictionary() => this.ToDictionary(x => x.Key, x => x.Value is null
            ? ""
            : string.Join(" ", x.Value), StringComparer.OrdinalIgnoreCase);

        IEnumerator<ListGroupItem> IEnumerable<ListGroupItem>.GetEnumerator() => _optionArgs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _optionArgs.GetEnumerator();

        CommandLine ICommandLineHandlerBuilder.CommandLine => this;

        /// <summary>
        /// 解析命令行参数，并返回解析后的 <see cref="CommandLine"/> 类型的新实例。
        /// 注意，暂不支持带有可执行程序路径的命令行参数，可查阅以下文章了解更多：
        /// <para>.NET 命令行参数包含应用程序路径吗？</para>
        /// <para>https://blog.walterlv.com/post/when-will-the-command-line-args-contain-the-executable-path.html</para>
        /// </summary>
        /// <param name="args">命令行参数。</param>
        /// <param name="protocolName">
        /// 协议名称
        /// <para>如果需要支持 URL 协议解析，请传入 <paramref name="protocolName"/> 参数：
        /// 对于 `walterlv://open/?id=ff` 这样的 URL，其协议名为 `walterlv`。</para>
        /// </param>
        /// <returns>包含命令行解析后结果的 <see cref="CommandLine"/> 类型的新实例。</returns>
        [Pure]
        public static CommandLine Parse(string[] args, string? protocolName = null) => Parse(args, protocolName, null);

        /// <summary>
        /// 解析命令行参数，并返回解析后的 <see cref="CommandLine"/> 类型的新实例。
        /// 注意，暂不支持带有可执行程序路径的命令行参数，可查阅以下文章了解更多：
        /// <para>.NET 命令行参数包含应用程序路径吗？</para>
        /// <para>https://blog.walterlv.com/post/when-will-the-command-line-args-contain-the-executable-path.html</para>
        /// </summary>
        /// <param name="args">命令行参数。</param>
        /// <param name="resourceManager">
        /// 用于本地化命令行参数的资源。
        /// 例如，当你建了 Resources.resx、Resources.zh-CN.resx 等文件后，可通过 Resources.ResourceManager 传入来获得本地化支持。
        /// </param>
        /// <returns>包含命令行解析后结果的 <see cref="CommandLine"/> 类型的新实例。</returns>
        [Pure]
        public static CommandLine Parse(string[] args, ResourceManager resourceManager) => Parse(args, null, resourceManager);

        /// <summary>
        /// 解析命令行参数，并返回解析后的 <see cref="CommandLine"/> 类型的新实例。
        /// 注意，暂不支持带有可执行程序路径的命令行参数，可查阅以下文章了解更多：
        /// <para>.NET 命令行参数包含应用程序路径吗？</para>
        /// <para>https://blog.walterlv.com/post/when-will-the-command-line-args-contain-the-executable-path.html</para>
        /// </summary>
        /// <param name="args">命令行参数。</param>
        /// <param name="protocolName">
        /// 协议名称
        /// <para>如果需要支持 URL 协议解析，请传入 <paramref name="protocolName"/> 参数：
        /// 对于 `walterlv://open/?id=ff` 这样的 URL，其协议名为 `walterlv`。</para>
        /// </param>
        /// <param name="resourceManager">
        /// 用于本地化命令行参数的资源。
        /// 例如，当你建了 Resources.resx、Resources.zh-CN.resx 等文件后，可通过 Resources.ResourceManager 传入来获得本地化支持。
        /// </param>
        /// <returns>包含命令行解析后结果的 <see cref="CommandLine"/> 类型的新实例。</returns>
        [Pure]
        public static CommandLine Parse(string[] args, string? protocolName, ResourceManager? resourceManager)
        {
            if (args is null || args.Length == 0)
            {
                return new CommandLine(new ListGroup<SingleOptimizedStrings>(), resourceManager);
            }

            if (!string.IsNullOrWhiteSpace(protocolName)
                && args.Length > 0
                && args[0].StartsWith($"{protocolName}://", StringComparison.OrdinalIgnoreCase))
            {
                // 如果传入的参数是协议参数，那么进行协议参数解析，并转换成命令行参数风格。
                // 由于 URL 解析不是主流程，所以这里暂时不考虑性能问题。
                args = ConvertUrlToArgs(args[0]);
            }

            var optionPrefix = AutoFindPrefix(args);
            var stateMachine = new CommandLineStateMachine(args, optionPrefix);
            var parsedArgs = stateMachine.Run();
            NormalizeParsedArgs(parsedArgs, optionPrefix);

            return new CommandLine(parsedArgs, resourceManager);
        }

        /// <summary>
        /// 将 URL 转换成符合命令行格式的参数列表。
        /// </summary>
        /// <param name="url">来源于 Web 的 URL。</param>
        /// <returns>符合命令行格式的参数列表。</returns>
        [Pure]
        private static string[] ConvertUrlToArgs(string url)
        {
            if (url != null)
            {
                url = HttpUtility.UrlDecode(url);
                var start = url.IndexOf('?', StringComparison.Ordinal);
                if (start >= 0 && url != null)
                {
                    var arguments = url.Substring(start + 1);
                    var args = from keyValueString in arguments.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                               let keyValue = keyValueString.Split('=')
                               select new[] { FormatShellLongName(keyValue[0]), keyValue[1] };
                    return args.SelectMany(x => x).ToArray();
                }
            }

#if NET45
            return new string[0];
#else
            return Array.Empty<string>();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NormalizeParsedArgs(ListGroup<SingleOptimizedStrings> parsedArgs, char optionPrefix)
        {
            foreach (var optionValue in parsedArgs.ForIndexes())
            {
                var index = optionValue.Key;
                var option = optionValue.Value;
                if (string.IsNullOrWhiteSpace(option))
                {
                    // 没有选项，只有值。
                }
                else if (option.Length == 2 && option[0] == optionPrefix)
                {
                    // 短名称。
                    // 短名称的种类：
                    //  -n -N /n /N
                    // 短名称是大小写敏感的，不同大小写表示不同的含义。
                    var shortName = option[1];
                    parsedArgs.ReplaceKey(index, shortName.ToString(CultureInfo.InvariantCulture));
                }
                else if (option.Length > 2 && option[0] == optionPrefix)
                {
                    // 长名称。
                    // 长名称的种类：
                    //  -LongName /LongName --long-name
                    // 以上示例的两种是等价的，但无论哪种，都至少需要三个字符。
                    // 长名称是大小写敏感的，大小写不同的参数将不会被识别。

                    // 格式化长名称，如果不是 -LongName 型，就都转换成 -LongName 型。
                    var longName = option[1] == optionPrefix
                        // --long-name 型
                        ? FormatCoreLongName(option, optionPrefix)
                        // -LongName 型或者 /LongName 型
                        : option.Substring(1);
                    parsedArgs.ReplaceKey(index, longName);
                }
                else
                {
                    // 参数格式不正确或不支持。
                    throw new NotSupportedException($"不支持命令行选项：{option}。");
                }
            }
        }

        /// <summary>
        /// 将 longName 这种名称转换为 -LongName 这种名称。
        /// </summary>
        /// <param name="option">要转换的一个 Option（而不是它的值）。</param>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatShellLongName(string option)
        {
            var chars = option.ToCharArray();
            if (char.IsUpper(chars[0]))
            {
                return $"-{option}";
            }

            chars[0] = char.ToUpperInvariant(chars[0]);
            return $"-{new string(chars)}";
        }

        /// <summary>
        /// 将 --long-name 这种名称转换为 -LongName 这种名称。
        /// </summary>
        /// <param name="option">要转换的一个 Option（而不是它的值）。</param>
        /// <param name="prefix">前缀符号。</param>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatCoreLongName(
#if SUPPORT_SPAN
            ReadOnlySpan<char>
#else
            IEnumerable<char>
#endif
            option, char prefix)
        {
            var builder = new StringBuilder();
            var isWordFirstLetter = true;
            foreach (var current in option
#if SUPPORT_SPAN
                .Slice(2)
#else
                .Skip(2)
#endif
                )
            {
                if (current == prefix)
                {
                    isWordFirstLetter = true;
                }
                else if (isWordFirstLetter)
                {
                    isWordFirstLetter = false;
                    builder.Append(char.ToUpperInvariant(current));
                }
                else
                {
                    builder.Append(current);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// 自动根据命令行参数决定选项的前缀应该是哪一个字符。
        /// </summary>
        /// <param name="args">从一组命令行参数中查找最适合作为命令行 Option 分隔符的字符。</param>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char AutoFindPrefix(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            foreach (var arg in args)
            {
                var prefix = arg[0];
                if (prefix is '-' || prefix is '/')
                {
                    return prefix;
                }
            }

            return '-';
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => string.Join(" ", _optionArgs.ForOptions().Select(pair =>
            $"{(string.IsNullOrEmpty(pair.Key) ? "" : $"-{pair.Key} ")}{string.Join(" ", pair.Value ?? (IEnumerable<string>)new string[0])}"));

        private class CommandLineDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly CommandLine _owner;

            public CommandLineDebugView(CommandLine owner) => _owner = owner;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            [SuppressMessage("VisualStudio", "IDE0051")]
            private string[] Options => _owner._optionArgs.ForOptions().Select(pair =>
                    $"{(string.IsNullOrEmpty(pair.Key) ? "" : $"-{pair.Key} ")}{string.Join(" ", pair.Value ?? (IEnumerable<string>)new string[0])}")
                .ToArray();
        }
    }
}
