using System.Diagnostics.Contracts;

namespace dotnetCampus.Cli.Standard
{
    /// <summary>
    /// 为命令行提供 GNU 标准支持。
    /// </summary>
    public static class GnuExtensions
    {
        /// <summary>
        /// 添加支持 GNU 标准的命令行通用参数。这将在无参数，带 --help 参数和带 --version 参数时得到通用的响应。
        /// </summary>
        /// <param name="builder">构造器模式。</param>
        /// <returns>构造器模式。</returns>
        [Pure]
        public static CommandLineHandlerBuilder AddStandardHandlers(
            this ICommandLineHandlerBuilder builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            return builder.AddFilter(new GnuFilter(builder.CommandLine));
        }

        /// <summary>
        /// 添加支持 GNU 标准的命令行通用参数。这将在无参数，带 --help 参数和带 --version 参数时得到通用的响应。
        /// </summary>
        /// <param name="builder">构造器模式。</param>
        /// <returns>构造器模式。</returns>
        [Pure]
        public static CommandLineAsyncHandlerBuilder AddStandardHandlers(
            this ICommandLineAsyncHandlerBuilder builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            return builder.AddFilter(new GnuFilter(builder.CommandLine));
        }
    }
}
