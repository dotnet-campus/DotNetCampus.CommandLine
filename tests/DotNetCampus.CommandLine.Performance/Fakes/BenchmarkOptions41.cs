using System.Collections.Generic;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Performance.Fakes;

[Command("", ExperimentalUseFullStackParser = true)]
public readonly record struct FullStackBenchmarkOptions41()
{
    [Option("debug")]
    public required bool IsDebugMode { get; init; }

    [Option('c', "count")]
    public required int TestCount { get; init; }

    [Option('n', "test-name")]
    public string? TestName { get; init; }

    [Option("test-category")]
    public string? TestCategory { get; init; }

    [Option('d', "detail-level")]
    public DetailLevel DetailLevel { get; init; }

    [Value(0, int.MaxValue)]
    public IReadOnlyList<string> TestItems { get; init; } = null!;
}

public class BenchmarkOptions41
{
    [Option("debug")]
    public required bool IsDebugMode { get; init; }

    [Option('c', "count")]
    public required int TestCount { get; init; }

    [Option('n', "test-name")]
    public string? TestName { get; set; }

    [Option("test-category")]
    public string? TestCategory { get; set; }

    [Option('d', "detail-level")]
    public DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

    [Value(0, int.MaxValue)]
    public IReadOnlyList<string> TestItems { get; init; } = null!;
}

public class NullableBenchmarkOptions41
{
    [Option("debug")]
    public bool IsDebugMode { get; set; }

    [Option('c', "count")]
    public int TestCount { get; set; }

    [Option('n', "test-name")]
    public string? TestName { get; set; }

    [Option("test-category")]
    public string? TestCategory { get; set; }

    [Option('d', "detail-level")]
    public DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

    [Value(0, int.MaxValue)]
    public IList<string> TestItems { get; set; } = null!;
}
