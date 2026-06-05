using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Legacy;

[Command("benchmark", Description = "Run performance benchmarks comparing 3.x and 4.x APIs.")]
internal class BenchmarkHandler : ICommandHandler
{
    [Option('n', "count", Description = "Number of iterations for the benchmark.")]
    public int Count { get; init; } = 10_000_000;

    [Option('w', "warmup", Description = "Number of warmup iterations.")]
    public int Warmup { get; init; } = 10_000;

    public Task<int> RunAsync()
    {
        var args = new[] { "--file", "test.txt", "--mode", "edit", "--silence" };
        CommandLineParsingOptions parsingOptions = CommandLineParsingOptions.DotNet;

        for (var i = 0; i < Warmup; i++)
        {
            dotnetCampus.Cli.CommandLine.Parse(args).As(new LegacyOptionsParser());
            dotnetCampus.Cli.CommandLine.Parse(args).As<LegacyOptions>();
            _ = CommandLine.Parse(args, parsingOptions).As<LegacyOptions>();
        }

        var stopwatch = new Stopwatch();

        Console.WriteLine($"Run {Count} times for: {string.Join(" ", args)}");
        Console.WriteLine("| Version | Parse   | As(Parser) | As(Runtime) |");
        Console.WriteLine("| ------- | ------- | ---------- | ----------- |");

        RunLegacy(stopwatch, args);
        RunNew(stopwatch, args, parsingOptions);

        return Task.FromResult(0);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RunLegacy(Stopwatch stopwatch, string[] args)
    {
        Console.Write("| 3.x     | ");
        stopwatch.Restart();
        for (var i = 0; i < Count; i++)
        {
            _ = dotnetCampus.Cli.CommandLine.Parse(args);
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),4} ms | ");

        var oldCommandLine = dotnetCampus.Cli.CommandLine.Parse(args);
        stopwatch.Restart();
        for (var i = 0; i < Count; i++)
        {
            _ = oldCommandLine.As(new LegacyOptionsParser());
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),7} ms | ");

        stopwatch.Restart();
        for (var i = 0; i < Count; i++)
        {
            _ = oldCommandLine.As<LegacyOptions>();
        }
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds.ToString(),8} ms |");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RunNew(Stopwatch stopwatch, string[] args, CommandLineParsingOptions parsingOptions)
    {
        Console.Write("| 4.x     | ");
        stopwatch.Restart();
        for (var i = 0; i < Count; i++)
        {
            _ = CommandLine.Parse(args, parsingOptions);
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),4} ms | ");

        var newCommandLine = CommandLine.Parse(args, parsingOptions);
        stopwatch.Restart();
        for (var i = 0; i < Count; i++)
        {
            var context = new CommandRunningContext { CommandLine = newCommandLine };
            _ = new LegacyOptionsBuilder().Build(context);
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),7} ms | ");

        stopwatch.Restart();
        for (var i = 0; i < Count; i++)
        {
            _ = newCommandLine.As<LegacyOptions>();
        }
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds.ToString(),8} ms |");
    }
}
