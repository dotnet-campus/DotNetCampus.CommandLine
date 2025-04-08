using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Generated;

namespace dotnetCampus.Cli;

class Program
{
    static void Main(string[] args)
    {
        // CommandLine.Parse(args, LocalizableStrings.ResourceManager)
        //     .AddStandardHandlers()
        //     .AddHandler<DefaultOptions>(o => o.Run())
        //     .AddHandler<SampleOptions>(o => o.Run())
        //     .Run();

        CommandLine.Parse(args)
            // .AddHandler<DemoCommandHandler>("demo", DemoCommandHandlerCreator)
            .AddHandlers<AssemblyCommandHandler>()
            .Run();
    }
}

[AssemblyCommands]
internal partial class AssemblyCommandHandler;

partial class AssemblyCommandHandler : ICommandHandlerCollection
{
    public ICommandHandler? TryMatch(string? verb, CommandLine commandLine) => verb switch
    {
        "demo" => new DemoVerbCreator().CreateInstance(commandLine),
        _ => null,
    };
}

[Verb("demo")]
internal class DemoCommandHandler : CommandHandler
{
    [Option("Option")]
    public required string Option { get; init; }

    [Value]
    public required string Argument { get; init; }
}
