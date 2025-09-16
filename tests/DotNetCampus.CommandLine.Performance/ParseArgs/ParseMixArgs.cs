using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;
using static DotNetCampus.Cli.CommandLineParsingOptions;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse MIX Args")]
public class ParseMixArgs
{
    [Benchmark(Description = "parse [MIX] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine.Parse(MixArgs, Flexible);
        commandLine.As<BenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [MIX] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(MixArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [MIX] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(MixArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }
}
