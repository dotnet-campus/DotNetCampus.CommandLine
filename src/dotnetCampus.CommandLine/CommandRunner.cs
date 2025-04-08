using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli;

public class CommandRunner
{
    private readonly CommandLine _commandLine;
    private readonly Dictionary<string, IVerbCreator<ICommandHandler>> _verbHandlers = [];

    internal CommandRunner(CommandLine commandLine)
    {
        _commandLine = commandLine;
    }

    internal CommandRunner(CommandRunner commandRunner)
    {
        _commandLine = commandRunner._commandLine;
    }

    public CommandRunner AddHandler(string verbName, IVerbCreator<ICommandHandler> handlerCreator)
    {
        _verbHandlers[verbName] = handlerCreator;
        return this;
    }

    public CommandRunner AddHandlers<T>()
        where T : ICommandHandlerCollection, new()
    {
        throw new NotImplementedException();
    }

    public int Run()
    {
        return 0;
    }

    public async Task<int> RunAsync()
    {
        return 0;
    }
}
