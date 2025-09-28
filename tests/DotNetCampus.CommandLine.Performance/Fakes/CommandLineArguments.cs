namespace DotNetCampus.Cli.Performance.Fakes;

internal static class CommandLineArguments
{
    public static readonly string[] NoArgs = [];

    public static readonly string[] DotNetArgs =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "-c:20",
        "--test-name:BenchmarkTest",
        "--detail-level=High",
        "--debug",
    ];

    public static readonly string[] DotNetArgsFor40 =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "--count:20",
        "--test-name:BenchmarkTest",
        "--detail-level=High",
        "--debug",
    ];

    public static readonly string[] WindowsArgs =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "-c", "20",
        "-TestName", "BenchmarkTest",
        "-DetailLevel", "High",
        "-Debug",
    ];

    public static readonly string[] WindowsArgsFor40 =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "-Count", "20",
        "-TestName", "BenchmarkTest",
        "-DetailLevel", "High",
        "-Debug",
    ];

    public static readonly string[] CmdArgs =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "/c", "20",
        "/TestName", "BenchmarkTest",
        "/DetailLevel", "High",
        "/Debug",
    ];

    public static readonly string[] GnuArgs =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "-c", "20",
        "--test-name", "BenchmarkTest",
        "--detail-level", "High",
        "--debug",
    ];

    public static readonly string[] GnuForConsoleAppFrameworkArgs =
    [
        "DotNetCampus.CommandLine.Performance.dll,DotNetCampus.CommandLine.Sample.dll,DotNetCampus.CommandLine.Test.dll",
        "-c", "20",
        "--test-name", "BenchmarkTest",
        "--detail-level", "High",
        "--debug",
    ];

    public static readonly string[] MixArgs =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "-c:20",
        "/TestName", "BenchmarkTest",
        "--detail-level=High",
        "-Debug",
    ];

    public static readonly string[] MixArgsFor40 =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "--count:20",
        "/TestName", "BenchmarkTest",
        "--detail-level=High",
        "-Debug",
    ];
}
