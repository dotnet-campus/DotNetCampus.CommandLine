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

    public static readonly string[] PowerShellArgs =
    [
        "DotNetCampus.CommandLine.Performance.dll",
        "DotNetCampus.CommandLine.Sample.dll",
        "DotNetCampus.CommandLine.Test.dll",
        "-c", "20",
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
}
