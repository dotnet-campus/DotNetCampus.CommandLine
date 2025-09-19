using System.Collections.Generic;
using DotNetCampus.Cli.Temp40.Compiler;

namespace DotNetCampus.Cli.Tests.Fakes;

public class AmbiguousOptions
{
    /// <summary>
    /// 命令行中传入 --boolean 也可，传入 --boolean true 也可。
    /// </summary>
    [Option("Boolean")]
    public bool Boolean { get; set; }

    /// <summary>
    /// 命令行中传入 --string-boolean true 也可，会使得值为 true。
    /// </summary>
    [Option("StringBoolean")]
    public string? StringBoolean { get; set; }

    /// <summary>
    /// 命令行中传入 --string-array a 也可。
    /// </summary>
    [Option("StringArray")]
    public string? StringArray { get; set; }

    /// <summary>
    /// 命令行中传入 --string-array a 也可，传入 --string-array a b 也可。
    /// </summary>
    [Option("Array")]
    public IReadOnlyList<string>? Array { get; set; }
}
