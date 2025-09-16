using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;
using static DotNetCampus.Cli.CommandLineParsingOptions;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse PowerShell Args")]
public class ParsePowerShellArgs
{
    [Benchmark(Description = "parse [PS1] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine.Parse(PowerShellArgs, Flexible);
        commandLine.As<BenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [PS1] -v=4.1 -p=powershell")]
    public void Parse41_PowerShell()
    {
        var commandLine = CommandLine.Parse(PowerShellArgs, PowerShell);
        commandLine.As<BenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [PS1] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(PowerShellArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [PS1] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(PowerShellArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }
}
