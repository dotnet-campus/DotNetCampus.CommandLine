#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Diagnostics.Contracts;

using static dotnetCampus.Cli.Utils.CommandLineHelpers;

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 包含命令行过滤器与拦截相关的扩展方法。
    /// </summary>
    public static class CommandLineFilters
    {
        /// <summary>
        /// 添加一个命令行过滤器，用于在命令行执行前后处理和拦截一些通用的命令。
        /// 与 AddHandler 不同的是，AddFilter 与顺序相关，先添加的 Filter 先执行。
        /// </summary>
        /// <typeparam name="TFilter">过滤器的类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <returns>已收集的过滤器，可以继续收集或者开始执行。</returns>
        [Pure]
        public static CommandLineHandlerBuilder AddFilter<TFilter>(
            this ICommandLineHandlerBuilder builder)
            where TFilter : ICommandLineFilter, new()
            => AddFilterCore<TFilter>(
                builder ?? throw new ArgumentNullException(nameof(builder)),
                null);

        /// <summary>
        /// 添加一个命令行过滤器，用于在命令行执行前后处理和拦截一些通用的命令。
        /// 与 AddHandler 不同的是，AddFilter 与顺序相关，先添加的 Filter 先执行。
        /// </summary>
        /// <typeparam name="TFilter">过滤器的类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的过滤器，可以继续收集或者开始执行。</returns>
        [Pure]
        public static CommandLineHandlerBuilder AddFilter<TFilter>(
            this ICommandLineHandlerBuilder builder,
            ICommandLineOptionParser<TFilter> parser)
            where TFilter : ICommandLineFilter
            => AddFilterCore(
                builder ?? throw new ArgumentNullException(nameof(builder)),
                parser ?? throw new ArgumentNullException(nameof(parser)));

        /// <summary>
        /// 添加一个命令行过滤器，用于在命令行执行前后处理和拦截一些通用的命令。
        /// 与 AddHandler 不同的是，AddFilter 与顺序相关，先添加的 Filter 先执行。
        /// </summary>
        /// <typeparam name="TFilter">过滤器的类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <returns>已收集的过滤器，可以继续收集或者开始异步执行。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddFilter<TFilter>(
            this ICommandLineAsyncHandlerBuilder builder)
            where TFilter : ICommandLineFilter, new()
            => AddFilterCore<TFilter>(
                builder ?? throw new ArgumentNullException(nameof(builder)),
                null);

        /// <summary>
        /// 添加一个命令行过滤器，用于在命令行执行前后处理和拦截一些通用的命令。
        /// 与 AddHandler 不同的是，AddFilter 与顺序相关，先添加的 Filter 先执行。
        /// </summary>
        /// <typeparam name="TFilter">过滤器的类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的过滤器，可以继续收集或者开始异步执行。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddFilter<TFilter>(
            this ICommandLineAsyncHandlerBuilder builder,
            ICommandLineOptionParser<TFilter>? parser = null)
            where TFilter : ICommandLineFilter
            => AddFilterCore(
                builder ?? throw new ArgumentNullException(nameof(builder)),
                parser ?? throw new ArgumentNullException(nameof(parser)));

        [Pure]
        private static CommandLineHandlerBuilder AddFilterCore<TFilter>(
            this ICommandLineHandlerBuilder builder,
            ICommandLineOptionParser<TFilter>? parser)
            where TFilter : ICommandLineFilter
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AddFilterCore(builder.CommandLine, parser);

            return builder switch
            {
                CommandLineHandlerBuilder commandLineBuilder => commandLineBuilder,
                CommandLine commandLine => new CommandLineHandlerBuilder(commandLine),
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        [Pure]
        private static CommandLineAsyncHandlerBuilder AddFilterCore<TFilter>(
            this ICommandLineAsyncHandlerBuilder builder,
            ICommandLineOptionParser<TFilter>? parser)
            where TFilter : ICommandLineFilter
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AddFilterCore(builder.CommandLine, parser);

            return builder switch
            {
                CommandLineAsyncHandlerBuilder commandLineBuilder => commandLineBuilder,
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        [Pure]
        private static void AddFilterCore<TFilter>(CommandLine commandLine, ICommandLineOptionParser<TFilter>? parser)
            where TFilter : ICommandLineFilter
        {
            commandLine.AddMatch(MatchFilter(commandLine, parser));
        }
    }
}
