namespace dotnetCampus.Cli;

public class CommandRunner
{
    private readonly CommandLine _commandLine;
    private readonly Dictionary<string, ICommandHandler> _handlers = [];

    internal CommandRunner(CommandLine commandLine)
    {
        _commandLine = commandLine;
    }

    public void AddHandler<T>()
    {
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
