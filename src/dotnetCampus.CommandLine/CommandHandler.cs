namespace dotnetCampus.Cli;

public abstract class CommandHandler : ICommandHandler
{
    Task<int> ICommandHandler.RunAsync()
    {
        return Task.FromResult(0);
    }
}
