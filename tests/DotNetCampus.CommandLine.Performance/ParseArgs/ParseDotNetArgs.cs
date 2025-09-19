using System;
using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse DotNet Args")]
public class ParseDotNetArgs
{
    [Benchmark(Description = "parse [NET] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine41.Parse(DotNetArgs, Options41.Flexible);
        commandLine.As<BenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [NET] -v=4.1 -p=dotnet")]
    public void Parse41_Dotnet()
    {
        var commandLine = CommandLine41.Parse(DotNetArgs, Options41.DotNet);
        commandLine.As<BenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [NET] -v=4.0 -p=flexible")]
    public void Parse40_Flexible()
    {
        var commandLine = CommandLine40.Parse(DotNetArgs, Options40.Flexible);
        commandLine.As<BenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [NET] -v=4.0 -p=dotnet")]
    public void Parse40_Dotnet()
    {
        var commandLine = CommandLine40.Parse(DotNetArgs, Options40.DotNet);
        commandLine.As<BenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [NET] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = CommandLine3.Parse(DotNetArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [NET] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = CommandLine3.Parse(DotNetArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }
}
