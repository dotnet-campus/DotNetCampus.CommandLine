using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;
using static DotNetCampus.Cli.CommandLineParsingOptions;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse DotNet Args")]
public class ParseDotNetArgs
{
    [Benchmark(Description = "parse [NET] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine.Parse(DotNetArgs, Flexible);
        commandLine.As<BenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [NET] -v=4.1 -p=dotnet")]
    public void Parse41_PowerShell()
    {
        var commandLine = CommandLine.Parse(DotNetArgs, DotNet);
        commandLine.As<BenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [NET] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(DotNetArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [NET] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(DotNetArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }
}
