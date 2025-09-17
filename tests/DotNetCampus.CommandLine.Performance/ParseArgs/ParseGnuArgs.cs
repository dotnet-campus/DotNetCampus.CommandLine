using System.Collections.Generic;
using System.CommandLine;
using BenchmarkDotNet.Attributes;
using CommandLine;
using ConsoleAppFramework;
using DotNetCampus.Cli.Performance.Fakes;
using static DotNetCampus.Cli.Performance.Fakes.CommandLineArguments;
using static DotNetCampus.Cli.CommandLineParsingOptions;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace DotNetCampus.Cli.Performance.ParseArgs;

[MemoryDiagnoser]
[BenchmarkCategory("Parse GNU Args")]
public class ParseGnuArgs
{
    [Benchmark(Description = "parse [GNU] -v=4.1 -p=flexible")]
    public void Parse41_Flexible()
    {
        var commandLine = CommandLine.Parse(GnuArgs, Flexible);
        commandLine.As<BenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [GNU] -v=4.1 -p=gnu")]
    public void Parse41_PowerShell()
    {
        var commandLine = CommandLine.Parse(GnuArgs, Gnu);
        commandLine.As<BenchmarkOptions4>();
    }

    [Benchmark(Description = "parse [GNU] -v=3.x -p=parser")]
    public void Parse3x_Parser()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(GnuArgs);
        commandLine.As(new BenchmarkOption3Parser());
    }

    [Benchmark(Description = "parse [GNU] -v=3.x -p=runtime")]
    public void Parse3x_Runtime()
    {
        var commandLine = dotnetCampus.Cli.CommandLine.Parse(GnuArgs);
        commandLine.As<RuntimeBenchmarkOptions3>();
    }

    [Benchmark(Description = "NuGet: ConsoleAppFramework")]
    public void ConsoleAppFramework()
    {
        var app = ConsoleApp.Create();
        app.Add<BenchmarkOptionsConsoleAppFramework>();
        app.Run(GnuForConsoleAppFrameworkArgs);
    }

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
}
