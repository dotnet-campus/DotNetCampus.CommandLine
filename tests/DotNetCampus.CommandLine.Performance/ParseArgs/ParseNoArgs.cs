using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse No Args")]
public class ParseNoArgs
{
    [Benchmark(Description = "parse [] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine41.Parse(NoArgs, Options41.Flexible);
        commandLine.As<NullableBenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [] -v=4.1 -p=dotnet")]
    public void Parse41_DotNet()
    {
        var commandLine = CommandLine41.Parse(NoArgs, Options41.DotNet);
        commandLine.As<NullableBenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [] -v=4.0 -p=flexible")]
    public void Parse40_Flexible()
    {
        var commandLine = CommandLine40.Parse(NoArgs, Options40.Flexible);
        commandLine.As<NullableBenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [] -v=4.0 -p=dotnet")]
    public void Parse40_DotNet()
    {
        var commandLine = CommandLine40.Parse(NoArgs, Options40.DotNet);
        commandLine.As<NullableBenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = CommandLine3.Parse(NoArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = CommandLine3.Parse(NoArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }
}
