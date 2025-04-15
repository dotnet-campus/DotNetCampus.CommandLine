using System.Diagnostics;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Tests.Fakes;

namespace DotNetCampus.Cli;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Parse the command line.");
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100000; i++)
        {
            _ = CommandLine.Parse(args).As<Options>();
        }
        stopwatch.Stop();
        Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine("Legacy parse the command line.");
        stopwatch.Restart();
        for (var i = 0; i < 100000; i++)
        {
            _ = dotnetCampus.Cli.CommandLine.Parse(args).As(new OptionsParser());
        }
        stopwatch.Stop();
        Console.WriteLine($"Legacy elapsed time: {stopwatch.ElapsedMilliseconds} ms");
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
