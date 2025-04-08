using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli.Tests.Fakes;

[AssemblyCommands]
internal partial class AssemblyCommandHandler;

partial class AssemblyCommandHandler : ICommandHandlerCollection
{
    public ICommandHandler? TryMatch(string? verb, CommandLine commandLine) => verb switch
    {
        // "fake" => new FakeVerbCommandHandler().CreateInstance(commandLine),
        _ => null,
    };
}
