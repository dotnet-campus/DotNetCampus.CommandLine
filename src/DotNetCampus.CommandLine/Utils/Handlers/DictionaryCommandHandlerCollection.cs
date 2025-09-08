using System.Collections.Concurrent;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

internal sealed class DictionaryCommandHandlerCollection : ICommandHandlerCollection
{
    private CommandObjectCreator? _defaultHandlerCreator;
    private readonly ConcurrentDictionary<string, CommandObjectCreator> _verbHandlers = [];

    public void AddHandler(string commandNames, CommandObjectCreator handlerCreator)
    {
        if (string.IsNullOrEmpty(commandNames))
        {
            if (_defaultHandlerCreator is not null)
            {
                throw new InvalidOperationException($"Duplicate default handler creator. Existed: {_defaultHandlerCreator}, new: {handlerCreator}");
            }
            _defaultHandlerCreator = handlerCreator;
        }
        else
        {
            if (!_verbHandlers.TryAdd(commandNames, handlerCreator))
            {
                throw new InvalidOperationException($"Duplicate handler with verb {commandNames}. Existed: {_verbHandlers}, new: {handlerCreator}");
            }
        }
    }

    public ICommandHandler? TryMatch(string commandNames, CommandLine commandLine)
    {
        var caseSensitive = commandLine.ParsingOptions.CaseSensitive;
        if (string.IsNullOrEmpty(commandNames))
        {
            return (ICommandHandler?)_defaultHandlerCreator?.Invoke(commandLine);
        }

        var bestMatchLength = -1;
        var bestMatch = new KeyValuePair<string, CommandObjectCreator?>("", null!);
        foreach (var pair in _verbHandlers)
        {
            var names = pair.Key;
            var creator = pair.Value;
            if (names.Length > bestMatchLength
                && commandNames.StartsWith(names, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
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
