using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ConsoleAppFramework;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;

#if IS_NOT_USING_AOT
using System.Collections.Generic;
using System.CommandLine;
using CommandLine;
#endif

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.NativeAot90)]
[MemoryDiagnoser]
[BenchmarkCategory("Parse GNU Args")]
public class ParseGnuArgs
{
    [Benchmark(Description = "parse [GNU] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine41.Parse(GnuArgs, Options41.Flexible);
        commandLine.As<BenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [GNU] -v=4.1 -p=gnu")]
    public void Parse41_PowerShell()
    {
        var commandLine = CommandLine41.Parse(GnuArgs, Options41.Gnu);
        commandLine.As<BenchmarkOptions41>();
    }

    [Benchmark(Description = "parse [GNU] -v=4.0 -p=flexible")]
    public void Parse40_Flexible()
    {
        var commandLine = CommandLine40.Parse(GnuArgs, Options40.Flexible);
        commandLine.As<BenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [GNU] -v=4.0 -p=gnu")]
    public void Parse40_PowerShell()
    {
        var commandLine = CommandLine40.Parse(GnuArgs, Options40.Gnu);
        commandLine.As<BenchmarkOptions40>();
    }

    [Benchmark(Description = "parse [GNU] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = CommandLine3.Parse(GnuArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [GNU] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = CommandLine3.Parse(GnuArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }

    [Benchmark(Description = "NuGet: ConsoleAppFramework")]
    public void ConsoleAppFramework()
    {
        var app = ConsoleApp.Create();
        app.Add<BenchmarkOptionsConsoleAppFramework>();
        app.Run(GnuForConsoleAppFrameworkArgs);
    }

#if IS_NOT_USING_AOT

    [Benchmark(Description = "NuGet: CommandLineParser")]
    public void CommandLineParser()
    {
        Parser.Default.ParseArguments<NullableBenchmarkOptions4>(GnuArgs).WithParsed(options => { });
    }

    [Benchmark(Description = "NuGet: System.CommandLine")]
    public void SystemCommandLine()
    {
        var debug = new Option<bool>("--debug");
        var count = new Option<int>("--count");
        var testName = new Option<string>("--test-name");
        var testCategory = new Option<string>("--test-category");
        var detailLevel = new Option<DetailLevel>("--detail-level");
        var testItems = new Argument<List<string>>();

        var rootCommand = new RootCommand("Benchmark for System.CommandLine");
        rootCommand.AddOption(debug);
        rootCommand.AddOption(count);
        rootCommand.AddOption(testName);
        rootCommand.AddOption(testCategory);
        rootCommand.AddOption(detailLevel);
        rootCommand.AddArgument(testItems);
        rootCommand.SetHandler(file => { });

        rootCommand.Invoke(GnuArgs);
    }

#endif

}
