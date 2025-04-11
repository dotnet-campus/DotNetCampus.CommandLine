using BenchmarkDotNet.Attributes;
using CommandLine;
using dotnetCampus.Cli.Performance.Fakes;
using dotnetCampus.Cli.Tests.Fakes;
using static dotnetCampus.Cli.Tests.Fakes.CommandLineArgs;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace dotnetCampus.Cli.Performance
{
    public class CommandLineParserTest
    {
        [Benchmark]
        public void ParseNoArgsAuto()
        {
            var commandLine = CommandLine.Parse(NoArgs);
            commandLine.As<Options>();
        }

        [Benchmark]
        public void ParseWindowsAuto()
        {
            var commandLine = CommandLine.Parse(WindowsStyleArgs);
            commandLine.As<Options>();
        }

        [Benchmark]
        public void HandleVerbs()
        {
            CommandLine.Parse(EditVerbArgs)
                .AddHandler<EditOptions>(options => 0)
                .AddHandler<PrintOptions>(options => 0)
                .Run();
        }

        [Benchmark]
        public void HandleVerbsRuntime()
        {
            var commandLine = CommandLine.Parse(EditVerbArgs);
            commandLine.AddHandler<EditOptions>(options => 0)
                .AddHandler<PrintOptions>(options => 0).Run();
        }

        [Benchmark]
        public void ParseCmdAuto()
        {
            var commandLine = CommandLine.Parse(CmdStyleArgs);
            commandLine.As<Options>();
        }

        [Benchmark]
        public void ParseLinuxAuto()
        {
            var commandLine = CommandLine.Parse(LinuxStyleArgs);
            commandLine.As<Options>();
        }

        [Benchmark]
        public void ParseUrlAuto()
        {
            var commandLine = CommandLine.Parse(UrlArgs, new CommandLineParsingOptions { SchemeNames = ["walterlv"] });
            commandLine.As<Options>();
        }

        [Benchmark]
        public void CommandLineParser()
        {
            Parser.Default.ParseArguments<ComparedOptions>(LinuxStyleArgs).WithParsed(options => { });
        }
    }
}
