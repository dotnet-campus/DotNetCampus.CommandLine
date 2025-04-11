using System;
using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli.Tests.Fakes;

public class FakeCommandOptions
{
    [Option("Fake")]
    public string? Fake { get; init; }

    [Option("FakeProperty")]
    public string? FakeProperty { get; init; }

    [Value]
    public string? Argument { get; init; }

    public Func<int>? Runner { get; set; }
}
