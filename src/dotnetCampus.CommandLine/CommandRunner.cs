using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Utils.Handlers;

namespace dotnetCampus.Cli;

public class CommandRunner
{
    private readonly CommandLine _commandLine;
    private readonly DictionaryCommandHandlerCollection _dictionaryVerbHandlers = new();
    private readonly List<ICommandHandlerCollection> _assemblyVerbHandlers = [];

    internal CommandRunner(CommandLine commandLine)
    {
        _commandLine = commandLine;
    }

    internal CommandRunner(CommandRunner commandRunner)
    {
        _commandLine = commandRunner._commandLine;
    }

    public CommandRunner AddHandler(IVerbCreator<ICommandHandler> handlerCreator)
    {
        _dictionaryVerbHandlers.AddHandler(handlerCreator);
        return this;
    }

    public CommandRunner AddHandler(string verbName, IVerbCreator<ICommandHandler> handlerCreator)
    {
        _dictionaryVerbHandlers.AddHandler(verbName, handlerCreator);
        return this;
    }

    public CommandRunner AddHandlers<T>()
        where T : ICommandHandlerCollection, new()
    {
        _assemblyVerbHandlers.Add(new T());
        return this;
    }

    public int Run()
    {
        return RunAsync().Result;
    }

    public Task<int> RunAsync()
    {
        var verbName = _commandLine.GuessedVerbName;

        if (_dictionaryVerbHandlers.TryMatch(verbName, _commandLine) is { } h1)
        {
            return h1.RunAsync();
        }

        foreach (var handler in _assemblyVerbHandlers)
        {
            if (handler.TryMatch(verbName, _commandLine) is { } h2)
            {
                return h2.RunAsync();
            }
        }

        throw new InvalidOperationException($"No command handler found for verb '{verbName}'. Please ensure that the command handler is registered correctly.");
    }
}
