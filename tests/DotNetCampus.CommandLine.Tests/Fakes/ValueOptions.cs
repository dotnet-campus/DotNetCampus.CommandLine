using System.Collections.Generic;
using DotNetCampus.Cli.Temp40.Compiler;

namespace DotNetCampus.Cli.Tests.Fakes;

public class ValueOptions
{
    [Option('f', nameof(Foo))]
    public string? Foo { get; set; }

    [Value(0)]
    public long LongValue { get; set; }

    [Value(1, 2)]
    public IReadOnlyList<string>? Values { get; set; }

    [Value(2)]
    public int Int32Value { get; set; }
}
