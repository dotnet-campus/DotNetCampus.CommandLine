using System.Collections.Generic;
using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Performance.Fakes;

public class BenchmarkOptions4
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

public class NullableBenchmarkOptions4
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
