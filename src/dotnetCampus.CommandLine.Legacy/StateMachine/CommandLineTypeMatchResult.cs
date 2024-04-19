#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Runtime.InteropServices;

namespace dotnetCampus.Cli.StateMachine
{
    /// <summary>
    /// 命令行处理器类型经谓词匹配后的匹配结果，可通过此匹配结果决定是否执行处理器函数。
    /// </summary>
    /// <typeparam name="T">
    /// 对于同步处理器，使用处理器返回值 int；
    /// 对于异步处理器，使用处理器返回值 Task&lt;int&gt;。
    /// </typeparam>
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct CommandLineTypeMatchResult<T> where T : notnull
    {
        /// <summary>
        /// 记录定义此谓词的类型。
        /// </summary>
        public Type VerbType { get; }

        /// <summary>
        /// 记录定义此谓词的类型。
        /// </summary>
        public string? Verb { get; }

        /// <summary>
        /// 记录谓词匹配结果。
        /// </summary>
        public readonly VerbMatchingResult MachingResult;

        /// <summary>
        /// 执行此委托以执行命令行此谓词下的处理器。
        /// 应该仅在 <see cref="MachingResult"/> 得到了匹配的谓词下才调用此委托。
        /// </summary>
        public readonly Func<T>? Handler;

        public CommandLineTypeMatchResult(VerbMatchingResult matchingResult, Type verbType, string? verb)
        {
            if (matchingResult != VerbMatchingResult.NotMatch)
            {
                throw new ArgumentException("当使用此重载时，VerbMatchingResult.NotMatched 是唯一有效的参数值。", nameof(matchingResult));
            }
            MachingResult = matchingResult;
            VerbType = verbType ?? throw new ArgumentNullException(nameof(verbType));
            Verb = verb;
            Handler = null;
        }

        public CommandLineTypeMatchResult(VerbMatchingResult matchingResult, Type verbType, string? verb, Func<T> handler)
        {
            if (matchingResult != VerbMatchingResult.Matched && matchingResult != VerbMatchingResult.FallbackMatched)
            {
                throw new ArgumentException("当使用此重载时，只有 VerbMatched 和 NonVerbMatched 是有效的参数值。", nameof(matchingResult));
            }
            MachingResult = matchingResult;
            VerbType = verbType ?? throw new ArgumentNullException(nameof(verbType));
            Verb = verb;
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }
    }
}
