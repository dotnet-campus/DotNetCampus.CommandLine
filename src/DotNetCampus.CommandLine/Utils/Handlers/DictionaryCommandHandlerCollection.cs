using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Utils.Handlers;

internal sealed class DictionaryCommandHandlerCollection : ICommandHandlerCollection
{
    private Func<CommandLine, ICommandHandler>? _defaultHandlerCreator;
    private readonly Dictionary<string, Func<CommandLine, ICommandHandler>> _verbHandlers = [];

    public void AddHandler(string? verbName, Func<CommandLine, ICommandHandler> handlerCreator)
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
            _verbHandlers[verbName] = handlerCreator;
        }
    }

    public ICommandHandler? TryMatch(string? verb, CommandLine commandLine)
    {
        return verb is null
            ? _defaultHandlerCreator?.Invoke(commandLine)
            : _verbHandlers.TryGetValue(verb, out var handlerCreator)
                ? handlerCreator.Invoke(commandLine)
                : null;
    }
}
