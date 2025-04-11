using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        await CommandLine.Parse(args /* , LocalizableStrings.ResourceManager */)
            // .AddStandardHandlers()
            .AddHandler<DefaultOptions>(o => o.Run())
            .AddHandler<SampleOptions>(o => o.Run())
            .AddHandlers<AssemblyCommandHandler>()
            .RunAsync();
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
