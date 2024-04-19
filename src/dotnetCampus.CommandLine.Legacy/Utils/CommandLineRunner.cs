using System.Collections.Generic;
using System.Threading.Tasks;

using dotnetCampus.Cli.Core;
using dotnetCampus.Cli.StateMachine;

using static dotnetCampus.Cli.Utils.CommandLineHelpers;

namespace dotnetCampus.Cli.Utils
{
    internal static class CommandLineRunner
    {
        /// <summary>
        /// 开始匹配谓词，如果谓词与任何一个已收集的谓词处理方法匹配，则执行此处理方法，然后返回其处理后的退出代码（没有指定退出代码则返回 0）。
        /// 如果已收集的命令行参数类型中包含不带谓词的参数，那么会成为默认谓词并在没有任何谓词匹配时执行其处理方法。
        /// <list type="number">
        /// <item>收集的所有谓词处理方法最多只能有一个不带谓词。</item>
        /// <item>最多只会有一个谓词处理方法被执行，此方法会返回唯一那个处理方法的退出代码。</item>
        /// <item>此方法虽然返回异步的可等待对象，但在已知一定是同步的上下文中，可直接使用 Task.Result 拿到返回值而无需异步转同步。</item>
        /// </list>
        /// </summary>
        /// <returns>谓词处理方法的退出代码。</returns>
        internal static Task<int> RunCoreAsync(CommandLine commandLine)
        {
            // 查询谓词。
            var possibleVerb = FindPossibleVerb(commandLine);

            // 执行预过滤器。
            var filterCache = new Dictionary<CommandLineFilterMatch, ICommandLineFilter>();
            var context = new CommandLineFilterContext(commandLine, possibleVerb);
            foreach (var match in commandLine.FilterMatchList)
            {
                var filter = match.FilterCreator();
                filterCache[match] = filter;
                filter.Filter(context);
                if (context.AreFurtherHandlersSuppressed)
                {
                    return Task.FromResult(context.ExitCode);
                }
            }

            // 执行命令行。
            foreach (var match in new HandleVerbStateMachine<Task<int>>(commandLine.VerbMatchList).Find(possibleVerb))
            {
                var exitCode = match.Handler!();
                return exitCode;
            }

            // 执行后过滤器。
            foreach (var match in commandLine.FilterMatchList)
            {
                var filter = filterCache.TryGetValue(match, out var cache) ? cache : match.FilterCreator();
                filter.PostFilter(context);
                if (context.AreFurtherHandlersSuppressed)
                {
                    return Task.FromResult(context.ExitCode);
                }
            }

            // 如果所有谓词均不匹配，则抛出异常。（如果不希望有异常，请加 AddStandardHandlers。）
            ThrowIfVerbNotMatchedAsync(possibleVerb);
            return Task.FromResult(0);
        }
    }
}
