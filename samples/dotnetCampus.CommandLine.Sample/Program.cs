using System;
using System.Threading.Tasks;
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
            .AddHandler<SampleCommandHandler>(o => 0)
            .AddHandlers<AssemblyCommandHandler>()
            .Run();
    }
}

[CollectCommandHandlersFromThisAssembly]
internal partial class AssemblyCommandHandler;

[Verb("sample")]
internal class SampleCommandHandler : ICommandHandler
{
    [Option("SampleProperty")]
    public required string Option { get; init; }

    [Value(Length = int.MaxValue)]
    public required string Argument { get; init; }

    public Task<int> RunAsync()
    {
        Console.WriteLine($"Option: {Option}");
        Console.WriteLine($"Argument: {Argument}");
        return Task.FromResult(0);
    }
}
