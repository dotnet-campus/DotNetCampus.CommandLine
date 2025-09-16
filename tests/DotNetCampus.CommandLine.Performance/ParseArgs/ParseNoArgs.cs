using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;
using static DotNetCampus.Cli.CommandLineParsingOptions;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse No Args")]
public class ParseNoArgs
{
    [Benchmark(Description = "parse [] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine.Parse(NoArgs, Flexible);
        commandLine.As<NullableBenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [] -v=4.1 -p=dotnet")]
    public void Parse41_PowerShell()
    {
        var commandLine = CommandLine.Parse(NoArgs, DotNet);
        commandLine.As<NullableBenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(NoArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(NoArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }
}
