using dotnetCampus.Cli.Compiler;

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
            .AddHandlers<AssemblyCommandHandler>()
            .Run();
    }
}

[AssemblyCommands]
internal partial class AssemblyCommandHandler;

partial class AssemblyCommandHandler : ICommandHandlerCollection
{
    public ICommandHandler? TryMatch(string verb, CommandLine commandLine)
    {
        return verb switch
        {
            "demo" => new DemoCommandHandler
            {
                Argument = commandLine.EnsureGetValue<string>(),
                Option = commandLine.EnsureGetOption<string>("Option"),
            },
            _ => null,
        };
    }
}

[Verb("demo")]
internal class DemoCommandHandler : CommandHandler
{
    [Option("Option")]
    public required string Option { get; init; }

    [Value]
    public required string Argument { get; init; }
}
