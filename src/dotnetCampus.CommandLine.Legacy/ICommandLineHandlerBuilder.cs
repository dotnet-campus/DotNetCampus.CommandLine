namespace dotnetCampus.Cli
{
    /// <summary>
    /// 对 <see cref="CommandLine"/> 和 <see cref="CommandLineHandlerBuilder"/> 提供抽象。
    /// </summary>
    public interface ICommandLineHandlerBuilder
    {
        /// <summary>
        /// 获取此命令行构造器所关联的命令行。
        /// </summary>
        CommandLine CommandLine { get; }
    }

    /// <summary>
    /// 对 <see cref="CommandLineAsyncHandlerBuilder"/> 提供抽象。
    /// </summary>
    public interface ICommandLineAsyncHandlerBuilder : ICommandLineHandlerBuilder
    {
    }
}
