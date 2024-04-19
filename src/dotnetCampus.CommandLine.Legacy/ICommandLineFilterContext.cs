namespace dotnetCampus.Cli
{
    /// <summary>
    /// 为 <see cref="ICommandLineFilter"/> 提供执行上下文参数。
    /// </summary>
    public interface ICommandLineFilterContext
    {
        /// <summary>
        /// 获取当前的命令行上下文。
        /// </summary>
        CommandLine CommandLine { get; }

        /// <summary>
        /// 获取当前命令行的谓词。
        /// <para>如需获取其他选项和值，请直接按通用命令行的实现方式标记 <see cref="OptionAttribute"/> 和 <see cref="ValueAttribute"/>。）</para>
        /// </summary>
        string? Verb { get; }

        /// <summary>
        /// 阻止后续命令行处理器对命令行的处理，此命令行在当前方法执行完后即终止。
        /// </summary>
        void SuppressFurtherHandlers(int exitCode = 0);
    }
}
