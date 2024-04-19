using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dotnetCampus.Cli.StateMachine
{
    /// <summary>
    /// 包装 <see cref="ICommandLineFilter"/> 的元数据。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay(nameof(CommandLineFilterMatch) + "->{FilterType}")]
    internal readonly struct CommandLineFilterMatch
    {
        /// <summary>
        /// 记录过滤器的类型。
        /// </summary>
        public Type FilterType { get; }

        /// <summary>
        /// 创建过滤器的创建函数。
        /// </summary>
        public readonly Func<ICommandLineFilter> FilterCreator { get; }

        public CommandLineFilterMatch(Type verbType, Func<ICommandLineFilter> filterCreator)
        {
            FilterType = verbType ?? throw new ArgumentNullException(nameof(verbType));
            FilterCreator = filterCreator ?? throw new ArgumentNullException(nameof(filterCreator));
        }
    }
}
