using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dotnetCampus.Cli.StateMachine
{
    /// <summary>
    /// 包装一个命令行处理器类型，这个类型将来可供谓词匹配并决定后续执行。
    /// </summary>
    /// <typeparam name="T">
    /// 对于同步处理器，使用处理器返回值 int；
    /// 对于异步处理器，使用处理器返回值 Task&lt;int&gt;。
    /// </typeparam>
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay(nameof(CommandLineTypeMatcher<T>) + "->{VerbType}")]
    internal readonly struct CommandLineTypeMatcher<T> where T : notnull
    {
        /// <summary>
        /// 记录定义此谓词的类型。
        /// </summary>
        public Type VerbType { get; }

        /// <summary>
        /// 尝试匹配谓词，然后返回匹配结果和执行函数。
        /// </summary>
        public readonly Func<string?, CommandLineTypeMatchResult<T>> Match { get; }

        public CommandLineTypeMatcher(Type verbType, Func<string?, CommandLineTypeMatchResult<T>> handler)
        {
            VerbType = verbType ?? throw new ArgumentNullException(nameof(verbType));
            Match = handler ?? throw new ArgumentNullException(nameof(handler));
        }
    }
}
