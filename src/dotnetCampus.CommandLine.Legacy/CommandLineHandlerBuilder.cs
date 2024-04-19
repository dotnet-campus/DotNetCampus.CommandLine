using System;

using static dotnetCampus.Cli.Utils.CommandLineRunner;

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 用于收集命令行的谓词处理方法。
    /// </summary>
    public class CommandLineHandlerBuilder : ICommandLineHandlerBuilder
    {
        /// <summary>
        /// 创建一个 <see cref="CommandLineHandlerBuilder"/> 的新实例。
        /// </summary>
        /// <param name="commandLine">命令行参数。</param>
        internal CommandLineHandlerBuilder(CommandLine commandLine)
            => CommandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));

        /// <summary>
        /// 收集所对应的命令行。
        /// </summary>
        public CommandLine CommandLine { get; }

        /// <summary>
        /// 开始匹配谓词，如果谓词与任何一个已收集的谓词处理方法匹配，则执行此处理方法，然后返回其处理后的退出代码（没有指定退出代码则返回 0）。
        /// 如果已收集的命令行参数类型中包含不带谓词的参数，那么会成为默认谓词并在没有任何谓词匹配时执行其处理方法。
        /// </summary>
        /// <remarks>
        /// 1. 收集的所有谓词处理方法最多只能有一个不带谓词。
        /// 2. 最多只会有一个谓词处理方法被执行，此方法会返回唯一那个处理方法的退出代码。
        /// </remarks>
        /// <returns>谓词处理方法的退出代码。</returns>
        public int Run() => RunCoreAsync(CommandLine).Result;
    }
}
