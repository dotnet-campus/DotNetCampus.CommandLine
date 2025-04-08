using System;
using System.Threading.Tasks;
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
            // .AddHandler<SampleCommandHandler>("demo", DemoCommandHandlerCreator)
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
        "sample" => new SampleVerbCreator().CreateInstance(commandLine),
        _ => null,
    };
}

[Verb("sample")]
internal class SampleCommandHandler : ICommandHandler
{
    [Option("SampleProperty")]
    public required string Option { get; init; }

    [Value]
    public string? Argument { get; init; }

    public Task<int> RunAsync()
    {
        Console.WriteLine($"Option: {Option}");
        Console.WriteLine($"Argument: {Argument}");
        return Task.FromResult(0);
    }
}
