using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse PowerShell Args")]
public class ParsePowerShellArgs
{
    [Benchmark(Description = "parse [PS1] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine41.Parse(PowerShellArgs, Options41.Flexible);
        commandLine.As<BenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [PS1] -v=4.1 -p=powershell")]
    public void Parse41_PowerShell()
    {
        var commandLine = CommandLine41.Parse(PowerShellArgs, Options41.PowerShell);
        commandLine.As<BenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [PS1] -v=4.0 -p=flexible")]
    public void Parse40_Flexible()
    {
        var commandLine = CommandLine40.Parse(PowerShellArgs, Options40.Flexible);
        commandLine.As<BenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [PS1] -v=4.0 -p=powershell")]
    public void Parse40_PowerShell()
    {
        var commandLine = CommandLine40.Parse(PowerShellArgs, Options40.PowerShell);
        commandLine.As<BenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [PS1] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = CommandLine3.Parse(PowerShellArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [PS1] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = CommandLine3.Parse(PowerShellArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }
}
