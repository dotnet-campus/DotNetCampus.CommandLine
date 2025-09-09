namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 管理一组命令处理器的集合，在命令匹配的情况下辅助执行对应的命令处理器。
/// </summary>
public interface ICommandHandlerCollection
{
    /// <summary>
    /// 尝试匹配一个命令处理器。
    /// </summary>
    /// <param name="commandNames">
    /// 可能的命令名称。
    /// <list type="bullet">
    /// <item>可能是空字符串，表示只匹配默认命令。</item>
    /// <item>可能包含无空格的名称，表示只匹配主命令。</item>
    /// <item>可能包含有空格的名称，表示匹配多级命令。</item>
    /// </list>
    /// </param>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <returns>匹配的命令处理器，如果没有匹配的命令处理器，则返回 <see langword="null"/>。</returns>
    ICommandHandler? TryMatch(string commandNames, CommandLine commandLine);
}

internal static class CommandHandlerCollectionMatcher
{
    internal static ICommandHandler? TryMatch(
        this CommandLine commandLine,
        string commandNames,
        CommandObjectCreator? defaultHandlerCreator,
        IReadOnlyDictionary<string, CommandObjectCreator> commandHandlers)
    {
        var caseSensitive = commandLine.ParsingOptions.CaseSensitive;
        if (string.IsNullOrEmpty(commandNames))
        {
            return (ICommandHandler?)defaultHandlerCreator?.Invoke(commandLine);
        }

        var bestMatchLength = -1;
        var bestMatch = new KeyValuePair<string, CommandObjectCreator?>("", null!);
        foreach (var pair in commandHandlers)
        {
            var names = pair.Key;
            var creator = pair.Value;
            
            // 检查是否为精确匹配或完整的前缀匹配（后面跟空格或结束）
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            bool isMatch = false;
            
            if (string.Equals(commandNames, names, comparison))
            {
                // 完全匹配
                isMatch = true;
            }
            else if (commandNames.StartsWith(names, comparison))
            {
                // 前缀匹配，但需要确保是完整单词匹配
                // 即命令名称后面必须是空格或字符串结束
                if (commandNames.Length > names.Length && commandNames[names.Length] == ' ')
                {
                    isMatch = true;
                }
            }
            
            if (isMatch && names.Length > bestMatchLength)
            {
                bestMatchLength = names.Length;
                bestMatch = new KeyValuePair<string, CommandObjectCreator?>(names, creator);
            }
        }
        return bestMatch.Value is { } handlerCreator
            ? (ICommandHandler)handlerCreator.Invoke(commandLine)
            : null;
    }
}
