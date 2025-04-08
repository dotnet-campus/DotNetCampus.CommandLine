namespace dotnetCampus.Cli.Compiler;

public interface ICommandHandlerCollection
{
    ICommandHandler? TryMatch(string? verb, CommandLine commandLine);
}
