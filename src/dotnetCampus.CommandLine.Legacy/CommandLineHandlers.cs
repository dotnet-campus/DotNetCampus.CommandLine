#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using dotnetCampus.Cli.StateMachine;

using static dotnetCampus.Cli.Utils.CommandLineHelpers;

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 包含命令行谓词处理相关的扩展方法。
    /// </summary>
    public static class CommandLineHandlers
    {
        /// <summary>
        /// 添加一个谓词处理方法，用于在命令行匹配谓词的时候执行。
        /// </summary>
        /// <typeparam name="TVerb">命令行参数类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="handler">命令行参数解析后的处理器。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的谓词处理方法，可以继续收集或者开始执行。</returns>
        [Pure]
        public static CommandLineHandlerBuilder AddHandler<TVerb>(
            this ICommandLineHandlerBuilder builder,
            Action<TVerb> handler, ICommandLineOptionParser<TVerb>? parser = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.CommandLine.AddMatch<TVerb>(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb),
                verb => MatchWithHandler(builder.CommandLine, verb, options => Invoke(handler, options), parser)));

            return builder switch
            {
                CommandLineHandlerBuilder commandLineBuilder => commandLineBuilder,
                CommandLine commandLine => new CommandLineHandlerBuilder(commandLine),
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        /// <summary>
        /// 添加一个带有退出代码返回值的谓词处理方法，用于在命令行匹配谓词的时候执行并返回退出代码。
        /// </summary>
        /// <typeparam name="TVerb">命令行参数类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="handler">命令行参数解析后的处理器。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的谓词处理方法，可以继续收集或者开始执行。</returns>
        [Pure]
        public static CommandLineHandlerBuilder AddHandler<TVerb>(
            this ICommandLineHandlerBuilder builder,
            Func<TVerb, int> handler, ICommandLineOptionParser<TVerb>? parser = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.CommandLine.AddMatch<TVerb>(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb),
                verb => MatchWithHandler(builder.CommandLine, verb, options => Invoke(handler, options), parser)));

            return builder switch
            {
                CommandLineHandlerBuilder commandLineBuilder => commandLineBuilder,
                CommandLine commandLine => new CommandLineHandlerBuilder(commandLine),
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        /// <summary>
        /// 添加一个异步的谓词处理方法，用于在命令行匹配谓词的时候异步执行。
        /// </summary>
        /// <typeparam name="TVerb">命令行参数类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="handler">命令行参数解析后的处理器。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的谓词处理方法，可以继续收集或者开始异步执行。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddHandler<TVerb>(
            this ICommandLineHandlerBuilder builder,
            Func<TVerb, Task> handler, ICommandLineOptionParser<TVerb>? parser = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var commandLine = builder.CommandLine;
            var newBuilder = new CommandLineAsyncHandlerBuilder(commandLine);

            builder.CommandLine.AddMatch<TVerb>(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb),
                verb => MatchWithHandler(commandLine, verb, options => Invoke(handler, options), parser)));

            return builder switch
            {
                CommandLineHandlerBuilder _ => new CommandLineAsyncHandlerBuilder(commandLine),
                CommandLine _ => new CommandLineAsyncHandlerBuilder(commandLine),
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        /// <summary>
        /// 添加一个带有退出代码返回值的异步谓词处理方法，用于在命令行匹配谓词的时候异步执行并返回退出代码。
        /// </summary>
        /// <typeparam name="TVerb">命令行参数类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="handler">命令行参数解析后的处理器。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的谓词处理方法，可以继续收集或者开始异步执行。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddHandler<TVerb>(
            this ICommandLineHandlerBuilder builder,
            Func<TVerb, Task<int>> handler, ICommandLineOptionParser<TVerb>? parser = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var commandLine = builder.CommandLine;
            var newBuilder = new CommandLineAsyncHandlerBuilder(commandLine);

            builder.CommandLine.AddMatch<TVerb>(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb),
                verb => MatchWithHandler(commandLine, verb, options => Invoke(handler, options), parser)));

            return builder switch
            {
                CommandLineHandlerBuilder _ => new CommandLineAsyncHandlerBuilder(commandLine),
                CommandLine _ => new CommandLineAsyncHandlerBuilder(commandLine),
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        /// <summary>
        /// 添加一个谓词处理方法，用于在命令行匹配谓词的时候执行。
        /// </summary>
        /// <typeparam name="TVerb">命令行参数类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="handler">命令行参数解析后的处理器。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的谓词处理方法，可以继续收集或者开始异步执行。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddHandler<TVerb>(
            this ICommandLineAsyncHandlerBuilder builder,
            Action<TVerb> handler, ICommandLineOptionParser<TVerb>? parser = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.CommandLine.AddMatch<TVerb>(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb),
                verb => MatchWithHandler(builder.CommandLine, verb, options => Invoke(handler, options), parser)));

            return builder switch
            {
                CommandLineAsyncHandlerBuilder commandLineBuilder => commandLineBuilder,
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        /// <summary>
        /// 添加一个带有退出代码返回值的谓词处理方法，用于在命令行匹配谓词的时候执行并返回退出代码。
        /// </summary>
        /// <typeparam name="TVerb">命令行参数类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="handler">命令行参数解析后的处理器。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的谓词处理方法，可以继续收集或者开始异步执行。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddHandler<TVerb>(
            this ICommandLineAsyncHandlerBuilder builder,
            Func<TVerb, int> handler, ICommandLineOptionParser<TVerb>? parser = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.CommandLine.AddMatch<TVerb>(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb),
                verb => MatchWithHandler(builder.CommandLine, verb, options => Invoke(handler, options), parser)));

            return builder switch
            {
                CommandLineAsyncHandlerBuilder commandLineBuilder => commandLineBuilder,
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        /// <summary>
        /// 添加一个异步的谓词处理方法，用于在命令行匹配谓词的时候异步执行。
        /// </summary>
        /// <typeparam name="TVerb">命令行参数类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="handler">命令行参数解析后的处理器。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的谓词处理方法，可以继续收集或者开始异步执行。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddHandler<TVerb>(
            this ICommandLineAsyncHandlerBuilder builder,
            Func<TVerb, Task> handler, ICommandLineOptionParser<TVerb>? parser = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.CommandLine.AddMatch<TVerb>(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb),
                verb => MatchWithHandler(builder.CommandLine, verb, options => Invoke(handler, options), parser)));

            return builder switch
            {
                CommandLineAsyncHandlerBuilder commandLineBuilder => commandLineBuilder,
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }

        /// <summary>
        /// 添加一个带有退出代码返回值的异步谓词处理方法，用于在命令行匹配谓词的时候异步执行并返回退出代码。
        /// </summary>
        /// <typeparam name="TVerb">命令行参数类型。</typeparam>
        /// <param name="builder">构造器模式。</param>
        /// <param name="handler">命令行参数解析后的处理器。</param>
        /// <param name="parser">命令行解析器。</param>
        /// <returns>已收集的谓词处理方法，可以继续收集或者开始异步执行。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddHandler<TVerb>(
            this ICommandLineAsyncHandlerBuilder builder,
            Func<TVerb, Task<int>> handler, ICommandLineOptionParser<TVerb>? parser = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.CommandLine.AddMatch<TVerb>(new CommandLineTypeMatcher<Task<int>>(typeof(TVerb),
                verb => MatchWithHandler(builder.CommandLine, verb, options => Invoke(handler, options), parser)));

            return builder switch
            {
                CommandLineAsyncHandlerBuilder commandLineBuilder => commandLineBuilder,
                _ => throw new NotSupportedException($"接口 {nameof(ICommandLineHandlerBuilder)} 不支持第三方实现。"),
            };
        }
    }
}
