using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli.Utils.Handlers;

internal sealed class DictionaryCommandHandlerCollection : ICommandHandlerCollection
{
    private IVerbCreator<ICommandHandler>? _defaultHandlerCreator;
    private readonly Dictionary<string, IVerbCreator<ICommandHandler>> _verbHandlers = [];

    public void AddHandler(IVerbCreator<ICommandHandler> handlerCreator)
    {
        if (_defaultHandlerCreator is not null)
        {
            throw new InvalidOperationException($"Duplicate default handler creator. Existed: {_defaultHandlerCreator}, new: {handlerCreator}");
        }
        _defaultHandlerCreator = handlerCreator;
    }

    public void AddHandler(string verbName, IVerbCreator<ICommandHandler> handlerCreator)
    {
        _verbHandlers[verbName] = handlerCreator;
    }

    public ICommandHandler? TryMatch(string? verb, CommandLine commandLine)
    {
        return verb is null
            ? _defaultHandlerCreator?.CreateInstance(commandLine)
            : _verbHandlers.TryGetValue(verb, out var handlerCreator)
                ? handlerCreator.CreateInstance(commandLine)
                : null;
    }
}
