using System;
using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Tests.Fakes;

[Verb("Fake")]
public class FakeVerbCommandHandler : ICommandHandler
{
    [Option("Fake")]
    public string? Fake { get; init; }

    [Option("FakeProperty")]
    public string? FakeProperty { get; init; }

    [Value]
    public string? Argument { get; init; }

    public Func<int>? Runner { get; set; }

    public Task<int> RunAsync()
    {
        if (Runner is not { } runner)
        {
            throw new InvalidOperationException("No runner is set.");
        }

        return Task.FromResult(runner());
    }
}
