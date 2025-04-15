using System.Diagnostics;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Tests.Fakes;

namespace DotNetCampus.Cli;

class Program
{
    static void Main(string[] args)
    {
        const int testCount = 100000;
        CommandLineParsingOptions parsingOptions = CommandLineParsingOptions.DotNet;

        Console.WriteLine($"# Test Count: {testCount}");

        Console.WriteLine("| Version | Parse   | As(Parser) | As(Runtime) |");
        Console.WriteLine("| ------- | ------- | ---------- | ----------- |");
        Console.Write("| 4.x     | ");
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < testCount; i++)
        {
            _ = CommandLine.Parse(args, parsingOptions);
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),4} ms | ");
        var newCommandLine = CommandLine.Parse(args, parsingOptions);
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = newCommandLine.As<Options>();
        }
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds.ToString(),7} ms | {stopwatch.ElapsedMilliseconds.ToString(),8} ms |");


        Console.Write("| 3.x     | ");
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = dotnetCampus.Cli.CommandLine.Parse(args);
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),4} ms | ");
        var oldCommandLine = dotnetCampus.Cli.CommandLine.Parse(args);
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = oldCommandLine.As(new OptionsParser());
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),7} ms | ");
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = oldCommandLine.As<Options>();
        }
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds.ToString(),8} ms |");
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
