using System.Collections.Concurrent;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

internal sealed class DictionaryCommandHandlerCollection : ICommandHandlerCollection
{
    private CommandObjectCreator? _defaultHandlerCreator;
    private readonly ConcurrentDictionary<string, CommandObjectCreator> _commandHandlers = [];

    public void AddHandler(string? commandNames, CommandObjectCreator handlerCreator)
    {
        if (
#if !NETCOREAPP3_1_OR_GREATER
            commandNames is null ||
#endif
            string.IsNullOrEmpty(commandNames))
        {
            if (_defaultHandlerCreator is not null)
            {
                throw new InvalidOperationException($"Duplicate default handler creator. Existed: {_defaultHandlerCreator}, new: {handlerCreator}");
            }
            _defaultHandlerCreator = handlerCreator;
        }
        else
        {
            if (!_commandHandlers.TryAdd(commandNames, handlerCreator))
            {
                throw new InvalidOperationException($"Duplicate handler with verb {commandNames}. Existed: {_commandHandlers}, new: {handlerCreator}");
            }
        }
    }

    public ICommandHandler? TryMatch(string commandNames, CommandLine commandLine)
    {
        return commandLine.TryMatch(commandNames, _defaultHandlerCreator, _commandHandlers);
    }
}
