namespace dotnetCampus.Cli.Tests.Fakes;

internal static class CommandLineArgs
{
    internal const string UrlProtocol = "walterlv";
    internal const string FileValue = @"C:\Users\lvyi\Desktop\文件.txt";
    internal const bool CloudValue = true;
    internal const bool IwbValue = true;
    internal const string ModeValue = "Display";
    internal const bool SilenceValue = true;
    internal const string PlacementValue = "Outside";
    internal const string StartupSessionValue = "89EA9D26-6464-4E71-BD04-AA6516063D83";

    internal static readonly string[] NoArgs = new string[0];

    internal static readonly string[] WindowsStyleArgs =
    {
        FileValue,
        "-Cloud",
        "-Iwb",
        "-m",
        ModeValue,
        "-s",
        "-p",
        PlacementValue,
        "-StartupSession",
        StartupSessionValue,
    };

    internal static readonly string[] CmdStyleArgs =
    {
        FileValue,
        "/Cloud",
        "/Iwb",
        "/m",
        ModeValue,
        "/s",
        "/p",
        PlacementValue,
        "/StartupSession",
        StartupSessionValue,
    };

    internal static readonly string[] Cmd2StyleArgs =
    {
        FileValue,
        "/Cloud",
        "/Iwb",
        $"/m:{ModeValue}",
        "/s",
        $"/p:{PlacementValue}",
        $"/StartupSession:{StartupSessionValue}",
    };

    internal static readonly string[] LinuxStyleArgs =
    {
        FileValue,
        "--cloud",
        "--iwb",
        "-m",
        ModeValue,
        "-s",
        "-p",
        PlacementValue,
        "--startup-session",
        StartupSessionValue,
    };

    internal static readonly string[] UrlArgs =
    {
        @"walterlv://open/?file=C:\Users\lvyi\Desktop\%E6%96%87%E4%BB%B6.txt&cloud=true&iwb=true&mode=Display&silence=true&placement=Outside&startupSession=89EA9D26-6464-4E71-BD04-AA6516063D83",
    };

    internal static readonly string[] EditVerbArgs =
    {
        "Edit", "XXX",
    };
}
