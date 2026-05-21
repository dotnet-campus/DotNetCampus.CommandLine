using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli;

/// <summary>
/// 用户输入的命令匹配到的已注册的命令。
/// </summary>
/// <param name="PossibleCommandNames">猜测的子命令。在匹配成功时，这就是已匹配到的子命令；匹配失败时，这是命令行第一个参数（可能是第一个子命令）。</param>
/// <param name="Metadata">如果已匹配成功，则此属性为已匹配的命令对象的元数据。</param>
/// <param name="Type">匹配到的命令类型。</param>
public readonly record struct MatchedCommand(string PossibleCommandNames, ICommandObjectMetadata? Metadata, MatchedCommandType Type);

/// <summary>
/// 匹配到的命令类型。
/// </summary>
public enum MatchedCommandType
{
    /// <summary>
    /// 未知（未匹配到）。
    /// </summary>
    Unknown,

    /// <summary>
    /// 匹配到了默认命令。即没有匹配到任何子命令对象，且已注册了默认命令。
    /// </summary>
    Default,

    /// <summary>
    /// 匹配到了唯一的子命令对象。
    /// </summary>
    Command,
}
