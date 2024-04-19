using System.Collections.Generic;

namespace dotnetCampus.Cli.StateMachine
{
    /// <summary>
    /// 一个命令行谓词执行状态机。其执行原理是：
    /// 1. 依次尝试匹配谓词，直到找到第一个匹配的谓词后执行然后结束状态机；
    /// 2. 如果所有谓词匹配结束依然没有匹配，那么从之前匹配过的所有处理器中找到一个没有谓词的重新执行，然后结束状态机。
    /// 3. 如果连没有谓词的处理器都没有找到，那么直接结束状态机。
    /// 也就是说，构造函数参数中会传入很多用于执行的委托，但永远只会执行一个。
    /// 对应到 foreach 语句，最多只会进入 foreach 语句内部一次。
    /// </summary>
    /// <example>
    /// 说明：
    /// 下面用记号 Options[verb] 来表示 Options 参数类型使用谓词 verb。对应到代码就是 Options 类型上标记了 VerbAttribute。
    /// 此类型用于 <see cref="CommandLineHandlers"/> 匹配 Options 然后执行 Options 对应的回调函数（这里称之为处理器）。
    /// 示例：
    /// 例如有三个谓词 A[a]、B[b]、C[c]，用户输入的命令行参数是 b。那么此状态机会依次执行 ABC，发现 B 匹配，于是执行 B 处理器并结束。
    /// 例如有三个谓词 A[a]、B[b]、C[c]，用户输入的命令行参数是 d。那么此状态机会依次执行 ABC，发现没有匹配，也没有默认谓词，于是直接结束。
    /// 例如有三个谓词 A[a]、B[null]、C，用户输入的命令行参数是 x。那么此状态机会依次执行 ABC，发现没有匹配，但是 B 有默认谓词，于是执行 B 处理器并结束。
    /// 值得注意的是，上面用户输入的 x 并不一定是谓词，有可能是文件名等真实的命令行参数。x 对 A[a] 和 C[c] 来说是谓词，但对 B[null] 来说是参数。
    /// </example>
    /// <typeparam name="T">
    /// 对于同步处理器，使用处理器返回值 int；
    /// 对于异步处理器，使用处理器返回值 Task&lt;int&gt;。
    /// </typeparam>
    internal class HandleVerbStateMachine<T> where T : notnull
    {
        /// <summary>
        /// 记录所有的处理器。会依次执行，如果执行结束依然没有返回，那么会从中挑选出默认处理器再执行一次。
        /// </summary>
        private readonly IEnumerable<CommandLineTypeMatcher<T>> _verbMatches;

        /// <summary>
        /// 传入一组处理器，用于匹配以后续执行。
        /// </summary>
        public HandleVerbStateMachine(params CommandLineTypeMatcher<T>[] handlers)
        {
            _verbMatches = handlers;
        }

        /// <summary>
        /// 传入一组处理器，用于匹配以后续执行。
        /// </summary>
        public HandleVerbStateMachine(IEnumerable<CommandLineTypeMatcher<T>> handlers)
        {
            _verbMatches = handlers;
        }

        /// <summary>
        /// 使用 foreach 语法执行此状态机，以查找最匹配的处理器。
        /// 在 foreach 语法中，只会返回最匹配的那一个，随后遍历便会结束。
        /// 需要额外注意的是即使是最匹配的那一个，也可能有无法完全匹配的理由，可能需要后续进行校验（<see cref="ValueAttribute"/> 中的必要参数满足）后才执行。
        /// </summary>
        public IEnumerable<CommandLineTypeMatchResult<T>> Find(string? verb)
        {
            // 留一个空位，保存默认处理器。
            CommandLineTypeMatchResult<T> @default = default;

            // 现在，开始依次匹配。
            foreach (var match in _verbMatches)
            {
                // 尝试匹配。
                var result = match.Match(verb);

                // 检查匹配结果。
                switch (result.MachingResult)
                {
                    case VerbMatchingResult.NotMatch:
                        // 如果没有匹配上，且这个不是默认谓词，那么继续匹配下一个。
                        continue;
                    case VerbMatchingResult.FallbackMatched:
                        // 如果没有匹配上，但这个是默认谓词，那么保存默认处理器，并继续匹配下一个。
                        @default = result;
                        continue;
                    case VerbMatchingResult.Matched:
                        // 如果匹配成功，那么进入 foreach 区域内部。
                        yield return result;
                        // 但是，进入 foreach 之后仅执行一次代码后就立即退出 foreach 区域。
                        yield break;
                }
            }

            // 如果遍历完成依然没有匹配，那么检查是否曾经遇到过带有默认谓词的处理器。
            if (@default.MachingResult == VerbMatchingResult.FallbackMatched)
            {
                // 如果有默认谓词，那么直接执行处理器而无视匹配。
                yield return @default;
            }
        }
    }
}
