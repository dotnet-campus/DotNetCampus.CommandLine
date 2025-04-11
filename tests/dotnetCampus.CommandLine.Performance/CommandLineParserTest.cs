﻿using System.CommandLine;
using System.IO;
using BenchmarkDotNet.Attributes;
using CommandLine;
using dotnetCampus.Cli.Performance.Fakes;
using dotnetCampus.Cli.Tests.Fakes;
using static dotnetCampus.Cli.Tests.Fakes.CommandLineArgs;
using static dotnetCampus.Cli.CommandLineParsingOptions;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace dotnetCampus.Cli.Performance;

// [DryJob] // 取消注释以验证测试能否运行。
[MemoryDiagnoser]
[BenchmarkCategory("CommandLine.Parse")]
public class CommandLineParserTest
{
    [Benchmark(Description = "parse  [] --flexible")]
    public void Parse_NoArgs_Flexible()
    {
        var commandLine = CommandLine.Parse(NoArgs, Flexible);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [] --gnu")]
    public void Parse_NoArgs_Gnu()
    {
        var commandLine = CommandLine.Parse(NoArgs, GNU);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [] --posix")]
    public void Parse_NoArgs_Posix()
    {
        var commandLine = CommandLine.Parse(NoArgs, POSIX);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [] --dotnet")]
    public void Parse_NoArgs_DotNet()
    {
        var commandLine = CommandLine.Parse(NoArgs, DotNet);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [] --powershell")]
    public void Parse_NoArgs_PowerShell()
    {
        var commandLine = CommandLine.Parse(NoArgs, PowerShell);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [PS1] --flexible")]
    public void Parse_PowerShell_Flexible()
    {
        var commandLine = CommandLine.Parse(WindowsStyleArgs, Flexible);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [PS1] --powershell")]
    public void Parse_PowerShell_PowerShell()
    {
        var commandLine = CommandLine.Parse(WindowsStyleArgs, PowerShell);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [CMD] --flexible")]
    public void Parse_Cmd_Flexible()
    {
        var commandLine = CommandLine.Parse(CmdStyleArgs, Flexible);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [CMD] --powershell")]
    public void Parse_Cmd_PowerShell()
    {
        var commandLine = CommandLine.Parse(CmdStyleArgs, PowerShell);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [GNU] --flexible")]
    public void Parse_Gnu_Flexible()
    {
        var commandLine = CommandLine.Parse(LinuxStyleArgs, Flexible);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "parse  [GNU] --gnu")]
    public void Parse_Gnu_Gnu()
    {
        var commandLine = CommandLine.Parse(LinuxStyleArgs, GNU);
        commandLine.As<Options>();
    }

    [Benchmark(Description = "handle [Edit,Print] --flexible")]
    public void Handle_Verbs_Flexible()
    {
        CommandLine.Parse(EditVerbArgs)
            .AddHandler<EditOptions>(options => 0)
            .AddHandler<PrintOptions>(options => 0)
            .Run();
    }

    [Benchmark(Description = "parse  [URL]")]
    public void Parse_Url()
    {
        var commandLine = CommandLine.Parse(UrlArgs, new CommandLineParsingOptions { SchemeNames = ["walterlv"] });
        commandLine.As<Options>();
    }

    [Benchmark(Description = "NuGet: CommandLineParser")]
    public void CommandLineParser()
    {
        Parser.Default.ParseArguments<ComparedOptions>(LinuxStyleArgs).WithParsed(options => { });
    }

    [Benchmark(Description = "NuGet: System.CommandLine")]
    public void SystemCommandLine()
    {
        var fileOption = new System.CommandLine.Option<FileInfo?>(
            name: "--file",
            description: "The file to read and display on the console.");

        var rootCommand = new RootCommand("Benchmark for System.CommandLine");
        rootCommand.AddOption(fileOption);
        rootCommand.SetHandler(file => { }, fileOption);

        rootCommand.Invoke(LinuxStyleArgs);
    }
}
