using System.Collections.Concurrent;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

internal sealed class DictionaryCommandHandlerCollection : ICommandHandlerCollection
{
    private LegacyCommandObjectCreator? _defaultHandlerCreator;
    private readonly ConcurrentDictionary<string, LegacyCommandObjectCreator> _commandHandlerCreators = [];

    public void AddHandler(string? commandNames, LegacyCommandObjectCreator handlerCreator)
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
            if (!_commandHandlerCreators.TryAdd(commandNames, handlerCreator))
            {
                throw new InvalidOperationException($"Duplicate handler with command {commandNames}. Existed: {_commandHandlerCreators}, new: {handlerCreator}");
            }
        }
    }

    public ICommandHandler? TryMatch(string possibleCommandNames, LegacyCommandLine commandLine)
    {
        return commandLine.TryMatch(possibleCommandNames, _defaultHandlerCreator, _commandHandlerCreators);
    }
}
