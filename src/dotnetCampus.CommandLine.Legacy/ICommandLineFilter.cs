namespace dotnetCampus.Cli
{
    /// <summary>
    /// 为命令行提供预处理过程（允许拦截执行），或后处理过程。
    /// </summary>
    public interface ICommandLineFilter
    {
        /// <summary>
        /// 开始执行预处理过程或开始拦截。
        /// </summary>
        /// <param name="context">预处理过程相关的上下文信息。</param>
        void Filter(ICommandLineFilterContext context);

        /// <summary>
        /// 开始执行后处理过程。
        /// </summary>
        /// <param name="context">后处理过程相关的上下文信息。</param>
        void PostFilter(ICommandLineFilterContext context);
    }
}
