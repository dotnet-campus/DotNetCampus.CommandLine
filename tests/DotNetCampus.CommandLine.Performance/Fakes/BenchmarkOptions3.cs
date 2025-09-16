using System;
using System.Collections.Generic;
using dotnetCampus.Cli;

namespace DotNetCampus.Cli.Performance.Fakes;

public class BenchmarkOptions3
{
    [Option("Debug")]
    public required bool IsDebugMode { get; init; }

    [Option('c', "Count")]
    public required int TestCount { get; init; }

    [Option('n', "TestName")]
    public string? TestName { get; set; }

    [Option("TestCategory")]
    public string? TestCategory { get; set; }

    [Option('d', "DetailLevel")]
    public DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

    [Value(0, int.MaxValue)]
    public IReadOnlyList<string> TestItems { get; init; } = null!;
}

public class RuntimeBenchmarkOptions3
{
    [Option("Debug")]
    public required bool IsDebugMode { get; init; }

    [Option('c', "Count")]
    public required int TestCount { get; init; }

    [Option('n', "TestName")]
    public string? TestName { get; set; }

    [Option("TestCategory")]
    public string? TestCategory { get; set; }

    [Option('d', "DetailLevel")]
    public string DetailLevel { get; set; } = nameof(DotNetCampus.Cli.Performance.Fakes.DetailLevel.Medium);

    [Value(0, int.MaxValue)]
    public IReadOnlyList<string> TestItems { get; init; } = null!;
}

internal sealed class BenchmarkOption3Parser : ICommandLineOptionParser<BenchmarkOptions3>
{
    private bool _isDebugMode;
    private int _testCount;
    private string? _testName;
    private string? _testCategory;
    private DetailLevel _detailLevel = DetailLevel.Medium;
    private List<string> _testItems = new();

    public string? Verb => null;

    public void SetValue(IReadOnlyList<string> values)
    {
        _testItems = new List<string>(values);
    }

    public void SetValue(char shortName, bool value)
    {
        switch (shortName)
        {
            case 'd':
                _isDebugMode = value;
                break;
        }
    }

    public void SetValue(char shortName, string value)
    {
        switch (shortName)
        {
            case 'n':
                _testName = value;
                break;
            case 'c':
                _testCount = int.Parse(value);
                break;
        }
    }

    public void SetValue(char shortName, IReadOnlyList<string> values)
    {
    }

    public void SetValue(string longName, bool value)
    {
        switch (longName)
        {
            case "Debug":
                _isDebugMode = value;
                break;
        }
    }

    public void SetValue(string longName, string value)
    {
        switch (longName)
        {
            case "Count":
                _testCount = int.Parse(value);
                break;
            case "TestName":
                _testName = value;
                break;
            case "TestCategory":
                _testCategory = value;
                break;
            case "DetailLevel":
                _detailLevel = Enum.Parse<DetailLevel>(value);
                break;
        }
    }

    public void SetValue(string longName, IReadOnlyList<string> values)
    {
    }

    public BenchmarkOptions3 Commit() => new()
    {
        IsDebugMode = _isDebugMode,
        TestCount = _testCount,
        TestName = _testName,
        TestCategory = _testCategory,
        DetailLevel = _detailLevel,
        TestItems = _testItems,
    };
}
