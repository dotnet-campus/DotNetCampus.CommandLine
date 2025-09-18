namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 管理一组命令处理器的集合，在命令匹配的情况下辅助执行对应的命令处理器。
/// </summary>
public interface ICommandHandlerCollection
{
    /// <summary>
    /// 尝试匹配一个命令处理器。
    /// </summary>
    /// <param name="possibleCommandNames">
    /// 可能的命令名称。
    /// <list type="bullet">
    /// <item>可能是空字符串，表示只匹配默认命令。</item>
    /// <item>可能包含无空格的名称，表示只匹配主命令。</item>
    /// <item>可能包含有空格的名称，表示匹配多级命令。</item>
    /// </list>
    /// </param>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <returns>匹配的命令处理器，如果没有匹配的命令处理器，则返回 <see langword="null"/>。</returns>
    ICommandHandler? TryMatch(string possibleCommandNames, CommandLine commandLine);
}

internal static class CommandHandlerCollectionMatcher
{
    /// <summary>
    /// 尝试匹配一个命令处理器。
    /// </summary>
    /// <param name="commandLine">已解析的命令行参数。</param>
    /// <param name="possibleCommandNames">
    /// 这是来自命令行传入的参数，一般来说会多于实际需要的命令层级数。（会多几个位置参数进来，但我们也不知道这位置参数有没有可能是命令啊）
    /// </param>
    /// <param name="defaultHandlerCreator">当没有任何命令匹配时，使用的默认命令处理器创建器。</param>
    /// <param name="commandHandlerCreators">尝试匹配命令时，使用此集合中的命令处理器创建器。</param>
    /// <returns>匹配的命令处理器，如果没有匹配的命令处理器，则返回 <see langword="null"/>。</returns>
    internal static ICommandHandler? TryMatch(
        this CommandLine commandLine,
        string possibleCommandNames,
        CommandObjectCreator? defaultHandlerCreator,
        IReadOnlyDictionary<string, CommandObjectCreator> commandHandlerCreators)
    {
        var caseSensitive = commandLine.ParsingOptions.CaseSensitive;
        if (string.IsNullOrEmpty(possibleCommandNames))
        {
            return (ICommandHandler?)defaultHandlerCreator?.Invoke(commandLine);
        }

        var bestMatchLength = -1;
        var bestMatch = new KeyValuePair<string, CommandObjectCreator?>("", null!);
        foreach (var pair in commandHandlerCreators)
        {
            var names = pair.Key;
            var creator = pair.Value;

            // 检查是否为精确匹配或完整的前缀匹配（后面跟空格或结束）
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            bool isMatch = false;

            if (string.Equals(possibleCommandNames, names, comparison))
            {
                // 完全匹配
                isMatch = true;
            }
            else if (possibleCommandNames.StartsWith(names, comparison))
            {
                // 前缀匹配，但需要确保是完整单词匹配
                // 即命令名称后面必须是空格或字符串结束
                if (possibleCommandNames.Length > names.Length && possibleCommandNames[names.Length] == ' ')
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
