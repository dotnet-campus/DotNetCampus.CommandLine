#define Benchmark
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Tests.Fakes;

namespace DotNetCampus.Cli;

class Program
{
    static void Main(string[] args)
    {
#if Benchmark
        // 第一次运行，排除类型初始化的影响，只测试代码执行性能。
        // 注释掉这句话，可以：
        // 1. 测试带类型初始化的性能
        // 2. 测试 AOT 性能 dotnet publish --self-contained -r win-x64 -c release -tl:off .\src\DotNetCampus.CommandLine.Sample\DotNetCampus.CommandLine.Sample.csproj
        Run(args);
        var stopwatch = Stopwatch.StartNew();
        Run(args);
        stopwatch.Stop();
        Console.WriteLine($"[# Elapsed: {stopwatch.Elapsed.TotalMicroseconds} us #]");
#else
        const int testCount = 1000000;
        CommandLineParsingOptions parsingOptions = CommandLineParsingOptions.DotNet;

        for (var i = 0; i < testCount; i++)
        {
            dotnetCampus.Cli.CommandLine.Parse(args).As(new OptionsParser());
            dotnetCampus.Cli.CommandLine.Parse(args).As<Options>();
            _ = CommandLine.Parse(args, parsingOptions).As<Options>();
        }

        var stopwatch = new Stopwatch();

        Console.WriteLine($"Run {testCount} times for: {string.Join(" ", args)}");

        Console.WriteLine("| Version | Parse   | As(Parser) | As(Runtime) |");
        Console.WriteLine("| ------- | ------- | ---------- | ----------- |");

        Console.Write("| 3.x     | ");
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = dotnetCampus.Cli.CommandLine.Parse(args);
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),4} ms | ");
        var oldCommandLine = dotnetCampus.Cli.CommandLine.Parse(args);
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = oldCommandLine.As(new OptionsParser());
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),7} ms | ");
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = oldCommandLine.As<Options>();
        }
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds.ToString(),8} ms |");

        Console.Write("| 4.x     | ");
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = CommandLine.Parse(args, parsingOptions);
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),4} ms | ");
        var newCommandLine = CommandLine.Parse(args, parsingOptions);
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = newCommandLine.As<Options>(OptionsBuilder.CreateInstance);
        }
        stopwatch.Stop();
        Console.Write($"{stopwatch.ElapsedMilliseconds.ToString(),7} ms | ");
        stopwatch.Restart();
        for (var i = 0; i < testCount; i++)
        {
            _ = newCommandLine.As<Options>();
        }
        stopwatch.Stop();
        Console.WriteLine($"{stopwatch.ElapsedMilliseconds.ToString(),8} ms |");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Run(string[] args)
    {
        if (args.Length is 0)
        {
        }
        else if (args[0] == "3.x-parser")
        {
            Run3xParser(args);
        }
        else if (args[0] == "3.x-runtime")
        {
            Run3xRuntime(args);
        }
        else if (args[0] == "4.x-interceptor")
        {
            Run4xInterceptor(args);
        }
        else if (args[0] == "4.x-module")
        {
            Run4xModule(args);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run3xParser(string[] args)
    {
        _ = dotnetCampus.Cli.CommandLine.Parse(args).As(new OptionsParser());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run3xRuntime(string[] args)
    {
        _ = dotnetCampus.Cli.CommandLine.Parse(args).As<Options>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run4xInterceptor(string[] args)
    {
        _ = CommandLine.Parse(args, CommandLineParsingOptions.DotNet).As<Options>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run4xModule(string[] args)
    {
        Initialize();
        _ = CommandLine.Parse(args, CommandLineParsingOptions.DotNet).As<Options>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Initialize()
    {
        // DefaultOptions { CommandName = null }
        global::DotNetCampus.Cli.CommandRunner.Register<global::DotNetCampus.Cli.DefaultOptions>(
            null,
            global::DotNetCampus.Cli.DefaultOptionsBuilder.CreateInstance);

        // EditOptions { CommandName = "Edit" }
        global::DotNetCampus.Cli.CommandRunner.Register<global::DotNetCampus.Cli.Tests.Fakes.EditOptions>(
            "Edit",
            global::DotNetCampus.Cli.Tests.Fakes.EditOptionsBuilder.CreateInstance);

        // Options { CommandName = null }
        global::DotNetCampus.Cli.CommandRunner.Register<global::DotNetCampus.Cli.Tests.Fakes.Options>(
            null,
            global::DotNetCampus.Cli.Tests.Fakes.OptionsBuilder.CreateInstance);

        // PrintOptions { CommandName = "Print" }
        global::DotNetCampus.Cli.CommandRunner.Register<global::DotNetCampus.Cli.Tests.Fakes.PrintOptions>(
            "Print",
            global::DotNetCampus.Cli.Tests.Fakes.PrintOptionsBuilder.CreateInstance);

        // SampleCommandHandler { CommandName = "sample" }
        global::DotNetCampus.Cli.CommandRunner.Register<global::DotNetCampus.Cli.SampleCommandHandler>(
            "sample",
            global::DotNetCampus.Cli.SampleCommandHandlerBuilder.CreateInstance);

        // SampleOptions { CommandName = "sample-options" }
        global::DotNetCampus.Cli.CommandRunner.Register<global::DotNetCampus.Cli.SampleOptions>(
            "sample-options",
            global::DotNetCampus.Cli.SampleOptionsBuilder.CreateInstance);

        // ShareOptions { CommandName = "Share" }
        global::DotNetCampus.Cli.CommandRunner.Register<global::DotNetCampus.Cli.Tests.Fakes.ShareOptions>(
            "Share",
            global::DotNetCampus.Cli.Tests.Fakes.ShareOptionsBuilder.CreateInstance);
    }
}

// [CollectCommandHandlersFromThisAssembly]
// internal partial class AssemblyCommandHandler;

[Command("sample")]
internal class SampleCommandHandler : ICommandHandler
{
    [Option("SampleProperty")]
    public required string Option { get; init; }

    [Value(Length = int.MaxValue)]
    public string? Argument { get; init; }

    public Task<int> RunAsync()
    {
        Console.WriteLine($"Option: {Option}");
        Console.WriteLine($"Argument: {Argument}");
        return Task.FromResult(0);
    }
}
