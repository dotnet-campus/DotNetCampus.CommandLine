using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Tests.Fakes;

namespace DotNetCampus.Cli;

class Program
{
    static void Main(string[] args)
    {
        Thread.Sleep(5000);
        CommandLine.Parse(args /* , LocalizableStrings.ResourceManager */)
            // .AddStandardHandlers()
            // .AddHandler<DefaultOptions>(o => o.Run())
            // .AddHandler<SampleOptions>(o => o.Run())
            .AddHandler<Options>(Run)
            .Run();
        Thread.Sleep(5000);
        // .AddHandlers<AssemblyCommandHandler>()
        // .RunAsync();
    }

    private static int Run(Options options)
    {
        return 0;
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
    public string? Argument { get; init; }

    public Task<int> RunAsync()
    {
        Console.WriteLine($"Option: {Option}");
        Console.WriteLine($"Argument: {Argument}");
        return Task.FromResult(0);
    }
}
