#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using dotnetCampus.Cli.Parsers;
using dotnetCampus.Cli.StateMachine;

namespace dotnetCampus.Cli.Utils
{
    /// <summary>
    /// 为命令行参数解析提供纯辅助方法。
    /// </summary>
    internal static class CommandLineHelpers
    {
        /// <summary>
        /// 为指定的命令行参数类型 <typeparamref name="T"/> 查找一个解析器。
        /// </summary>
        /// <typeparam name="T">要解析的命令行参数类型。</typeparam>
        /// <returns>用于解析到 <typeparamref name="T"/> 类型的解析器实例。</returns>
        [Pure]
        internal static ICommandLineOptionParser<T> FindParser<T>()
        {
            ICommandLineOptionParser<T>? parser = null;
            var optionType = typeof(T);
            var parserType = optionType.Assembly.GetType($"{optionType.FullName}Parser", false, false);

            // 尝试从目标程序集中查找一个命令行解析器。
            if (parserType != null)
            {
                parser = Activator.CreateInstance(parserType) as ICommandLineOptionParser<T>;
            }

            // 如果找不到目标命令行解析器，那么就使用内置的运行时解析器。
            return parser ?? RuntimeCommandLineOptionParser<T>.Create();
        }

        /// <summary>
        /// 当第一个参数非选项的时候，取第一个命令行参数，这可能就是我们要找的谓词。
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? FindPossibleVerb(CommandLine commandLine)
        {
            var optionValue = commandLine.FirstOrDefault();
            var option = optionValue.Key;
            var values = optionValue.Value;
#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections. Instead use the collection directly
            var possibleVerb = (string.IsNullOrWhiteSpace(option) ? values : null)?.FirstOrDefault();
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections. Instead use the collection directly
            return possibleVerb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CommandLineFilterMatch MatchFilter<TFilter>(CommandLine commandLine, ICommandLineOptionParser<TFilter>? parser)
            where TFilter : ICommandLineFilter
        {
            return new CommandLineFilterMatch(typeof(TFilter), () => commandLine.As(parser ?? FindParser<TFilter>()));
        }

        /// <summary>
        /// 尝试匹配谓词并调用处理器函数。
        /// 如果匹配成功则执行处理器函数并返回退出代码，否则返回 null。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CommandLineTypeMatchResult<int> MatchWithHandler<TVerb>(CommandLine commandLine, string? possibleVerb,
            Action<TVerb> handler, ICommandLineOptionParser<TVerb>? parser)
        {
            return MatchWithHandler(commandLine, possibleVerb, verb =>
            {
                handler(verb);
                return 0;
            }, parser);
        }

        /// <summary>
        /// 尝试匹配谓词并调用处理器函数。
        /// 如果匹配成功则执行处理器函数并返回退出代码，否则返回 null。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CommandLineTypeMatchResult<int> MatchWithHandler<TVerb>(CommandLine commandLine, string? possibleVerb,
            Func<TVerb, int> handler, ICommandLineOptionParser<TVerb>? parser)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            // 为命令行参数类型寻找解析器。
            parser ??= FindParser<TVerb>();

            // 尝试匹配谓词，并执行处理器代码。
            if (string.Equals(possibleVerb, parser.Verb, StringComparison.InvariantCultureIgnoreCase))
            {
                return new CommandLineTypeMatchResult<int>(VerbMatchingResult.Matched,
                    typeof(TVerb), possibleVerb, () => handler(commandLine.As(parser)));
            }

            if (parser.Verb is null)
            {
                return new CommandLineTypeMatchResult<int>(VerbMatchingResult.FallbackMatched,
                    typeof(TVerb), possibleVerb, () => handler(commandLine.As(parser)));
            }

            return new CommandLineTypeMatchResult<int>(VerbMatchingResult.NotMatch,
                typeof(TVerb), possibleVerb);
        }

        /// <summary>
        /// 尝试匹配谓词并调用处理器函数。
        /// 如果匹配成功则执行处理器函数并返回退出代码，否则返回 null。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CommandLineTypeMatchResult<Task<int>> MatchWithHandler<TVerb>(CommandLine commandLine,
            string? possibleVerb, Func<TVerb, Task> handler, ICommandLineOptionParser<TVerb>? parser)
        {
            return MatchWithHandler(commandLine, possibleVerb, async verb =>
            {
                await handler(verb).ConfigureAwait(false);
                return 0;
            }, parser);
        }

        /// <summary>
        /// 尝试匹配谓词并调用处理器函数。
        /// 如果匹配成功则执行处理器函数并返回退出代码，否则返回 null。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CommandLineTypeMatchResult<Task<int>> MatchWithHandler<TVerb>(CommandLine commandLine,
            string? possibleVerb, Func<TVerb, Task<int>> handler, ICommandLineOptionParser<TVerb>? parser)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            // 为命令行参数类型寻找解析器。
            parser ??= FindParser<TVerb>();

            // 尝试匹配谓词，并执行处理器代码。
            if (string.Equals(possibleVerb, parser.Verb, StringComparison.InvariantCultureIgnoreCase))
            {
                return new CommandLineTypeMatchResult<Task<int>>(VerbMatchingResult.Matched,
                    typeof(TVerb), possibleVerb, () => handler(commandLine.As(parser)));
            }

            if (parser.Verb is null)
            {
                return new CommandLineTypeMatchResult<Task<int>>(VerbMatchingResult.FallbackMatched,
                    typeof(TVerb), possibleVerb, () => handler(commandLine.As(parser)));
            }

            return new CommandLineTypeMatchResult<Task<int>>(VerbMatchingResult.NotMatch,
                typeof(TVerb), possibleVerb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Task<int> Invoke<TVerb>(Action<TVerb> handler, TVerb options)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            handler(options);
            return Task.FromResult(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Task<int> Invoke<TVerb>(Func<TVerb, int> handler, TVerb options)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return Task.FromResult(handler(options));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async Task<int> Invoke<TVerb>(Func<TVerb, Task> handler, TVerb options)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            await handler(options).ConfigureAwait(false);
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async Task<int> Invoke<TVerb>(Func<TVerb, Task<int>> handler, TVerb options)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return await handler(options).ConfigureAwait(false);
        }

        /// <summary>
        /// 如果所有谓词都不匹配，则抛出异常。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ThrowIfVerbNotMatched(string? possibleVerb)
        {
            throw possibleVerb == null
                ? new ArgumentException("传入的命令行参数中没有发现谓词，但此方法规定的所有命令行类型都要求有谓词。", nameof(possibleVerb))
                : new ArgumentException($"没有任何命令行参数类型符合谓词 {possibleVerb}。", nameof(possibleVerb));
        }

        /// <summary>
        /// 如果所有谓词都不匹配，则抛出异常。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ThrowIfVerbNotMatchedAsync(string? possibleVerb)
        {
            throw possibleVerb == null
                ? new ArgumentException("传入的命令行参数中没有发现谓词，但此方法规定的所有命令行类型都要求有谓词。", nameof(possibleVerb))
                : new ArgumentException($"没有任何命令行参数类型符合谓词 {possibleVerb}。", nameof(possibleVerb));
        }
    }
}
