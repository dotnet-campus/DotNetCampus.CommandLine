using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Help;

/// <summary>
/// 处理命令行帮助请求的处理器。
/// </summary>
public interface IHelpHandler
{
    /// <summary>
    /// 处理帮助请求。
    /// </summary>
    /// <param name="matchedCommand">本次用户输入的命令所匹配到的命令信息。</param>
    /// <param name="defaultCommandMetadata">默认命令的元数据，可通过 <see cref="ICommandObjectMetadata.GetHelp"/> 获取其帮助信息。</param>
    /// <param name="subCommandMetadataList">所有子命令的元数据，可分别通过 <see cref="ICommandObjectMetadata.GetHelp"/> 获取其帮助信息。</param>
    void Handle(
        MatchedCommand matchedCommand,
        ICommandObjectMetadata? defaultCommandMetadata,
        IReadOnlyList<ICommandObjectMetadata> subCommandMetadataList);
}
