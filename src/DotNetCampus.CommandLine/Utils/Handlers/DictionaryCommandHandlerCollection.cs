using System.Collections.Concurrent;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

internal sealed class DictionaryCommandHandlerCollection : ICommandHandlerCollection
{
    private CommandObjectCreator? _defaultHandlerCreator;
    private readonly ConcurrentDictionary<string, CommandObjectCreator> _verbHandlers = [];

    public void AddHandler(string? verbName, CommandObjectCreator handlerCreator)
    {
        if (verbName is null)
        {
            if (_defaultHandlerCreator is not null)
            {
                throw new InvalidOperationException($"Duplicate default handler creator. Existed: {_defaultHandlerCreator}, new: {handlerCreator}");
            }
            _defaultHandlerCreator = handlerCreator;
        }
        else
        {
            if (!_verbHandlers.TryAdd(verbName, handlerCreator))
            {
                throw new InvalidOperationException($"Duplicate handler with verb {verbName}. Existed: {_verbHandlers}, new: {handlerCreator}");
            }
        }
    }

    public ICommandHandler? TryMatch(string? verb, CommandLine commandLine)
    {
        return verb is null
            ? (ICommandHandler?)_defaultHandlerCreator?.Invoke(commandLine)
            : _verbHandlers.TryGetValue(verb, out var handlerCreator)
                ? (ICommandHandler)handlerCreator.Invoke(commandLine)
                : null;
    }
}
