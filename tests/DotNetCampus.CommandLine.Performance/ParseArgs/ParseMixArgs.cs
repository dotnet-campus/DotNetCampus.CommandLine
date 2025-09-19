using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse MIX Args")]
public class ParseMixArgs
{
    [Benchmark(Description = "parse [MIX] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine41.Parse(MixArgs, Options41.Flexible);
        commandLine.As<BenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [MIX] -v=4.0 -p=flexible")]
    public void Parse40_Flexible()
    {
        var commandLine = CommandLine40.Parse(MixArgs, Options40.Flexible);
        commandLine.As<BenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [MIX] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = CommandLine3.Parse(MixArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [MIX] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = CommandLine3.Parse(MixArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }
}
