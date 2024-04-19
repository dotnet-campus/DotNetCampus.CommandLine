#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using dotnetCampus.Cli.StateMachine;

namespace dotnetCampus.Cli.Core
{
    /// <summary>
    /// 为 <see cref="ICommandLineFilterContext"/> 类型提供基本实现。
    /// </summary>
    internal class CommandLineFilterContext : ICommandLineFilterContext
    {
        private volatile int _areFurtherHandlersSuppressed;

        internal CommandLineFilterContext(CommandLine commandLine, string? verb)
        {
            CommandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            Verb = verb;
        }

        /// <inheritdoc />
        public CommandLine CommandLine { get; }

        /// <inheritdoc />
        public string? Verb { get; }

        /// <summary>
        /// 查询本次 <see cref="ICommandLineFilter"/> 执行完成后是否阻止后续处理器的执行。
        /// </summary>
        public bool AreFurtherHandlersSuppressed => _areFurtherHandlersSuppressed == 1;

        /// <summary>
        /// 获取命令行退出时的返回值。
        /// </summary>
        public int ExitCode { get; private set; }

        /// <summary>
        /// 获取所有的谓词列表和对应的执行上下文。
        /// </summary>
        /// <returns>所有的谓词列表和对应的执行上下文。</returns>
        internal IEnumerable<Type> EnumerateRelatedTypes()
        {
            return CommandLine.FilterMatchList.Select(x => x.FilterType).Concat(CommandLine.VerbMatchList.Select(x => x.VerbType));
        }

        /// <summary>
        /// 获取所有的谓词列表和对应的执行上下文。
        /// </summary>
        /// <returns>所有的谓词列表和对应的执行上下文。</returns>
        internal Type? GetVerbType()
        {
            foreach (var match in new HandleVerbStateMachine<Task<int>>(CommandLine.VerbMatchList).Find(Verb))
            {
                return match.VerbType;
            }
            return null;
        }

        /// <inheritdoc />
        public void SuppressFurtherHandlers(int exitCode = 0)
        {
            var hasValue = Interlocked.Increment(ref _areFurtherHandlersSuppressed);
            if (hasValue == 1)
            {
                ExitCode = exitCode;
            }
            else
            {
                throw new InvalidOperationException("在一个命令行执行上下文中，仅允许标记一次拦截后续执行。");
            }
        }
    }
}
