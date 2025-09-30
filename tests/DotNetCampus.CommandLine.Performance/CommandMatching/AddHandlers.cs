using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Performance.Fakes;

namespace DotNetCampus.Cli.Performance.CommandMatching;

[MemoryDiagnoser]
[BenchmarkCategory("Add handlers")]
public class AddHandlers
{
    [Benchmark(Description = "add-handler -v=4.1 -p=dotnet")]
    public void AddHandler()
    {
        // Arrange
        var commandLine = CommandLine.Parse(CommandLineArguments.CommandArgs, Options41.DotNet);

        // Act
        var result = commandLine
            .AddHandler<DefaultHandler>()
            .AddHandler<FooHandler>()
            .RunAsync();
        var exitCode = result.Result.ExitCode;
    }

    public record DefaultHandler : ICommandHandler
    {
        [Value(0)]
        public string? Value { get; set; } = "DefaultHandler";

        public Task<int> RunAsync()
        {
            return Task.FromResult(1);
        }
    }

    [Command("foo")]
    public record FooHandler : ICommandHandler
    {
        [Value(0)]
        public string? Value { get; set; } = "FooHandler";

        public Task<int> RunAsync()
        {
            return Task.FromResult(2);
        }
    }
}
