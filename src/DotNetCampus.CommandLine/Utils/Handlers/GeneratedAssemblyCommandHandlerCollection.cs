using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

/// <summary>
/// 由源生成器继承，用于收集某个特定程序集中所有的命令处理器，然后统一处理。
/// </summary>
public abstract class GeneratedAssemblyCommandHandlerCollection : ICommandHandlerCollection
{
    /// <summary>
    /// 源生成器在构造函数中，为没有命令名称的命令处理器赋值。
    /// </summary>
    protected LegacyCommandObjectCreator? Default { get; init; }

    /// <summary>
    /// 源生成器在构造函数中，为有命令名称的命令处理器赋值。
    /// </summary>
    protected Dictionary<string, LegacyCommandObjectCreator> Creators { get; init; } = [];

    /// <inheritdoc />
    public ICommandHandler? TryMatch(string possibleCommandNames, LegacyCommandLine commandLine)
    {
        return commandLine.TryMatch(possibleCommandNames, Default, Creators);
    }
}
