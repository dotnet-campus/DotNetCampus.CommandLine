using System;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

internal static class CommandLineStyleTestingExtensions
{
    public static CommandLineParsingOptions ToParsingOptions(this CommandLineStyle style) => style switch
    {
        CommandLineStyle.Flexible => CommandLineParsingOptions.Flexible,
        CommandLineStyle.DotNet => CommandLineParsingOptions.DotNet,
        CommandLineStyle.Gnu => CommandLineParsingOptions.Gnu,
        CommandLineStyle.Posix => CommandLineParsingOptions.Posix,
        CommandLineStyle.PowerShell => CommandLineParsingOptions.PowerShell,
        _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
    };

    public static CommandLineParsingOptions ToParsingOptions(this TestCommandLineStyle style) => style switch
    {
        TestCommandLineStyle.Flexible => CommandLineParsingOptions.Flexible,
        TestCommandLineStyle.DotNet => CommandLineParsingOptions.DotNet,
        TestCommandLineStyle.Gnu => CommandLineParsingOptions.Gnu,
        TestCommandLineStyle.Posix => CommandLineParsingOptions.Posix,
        TestCommandLineStyle.PowerShell => CommandLineParsingOptions.PowerShell,
        TestCommandLineStyle.Url => CommandLineParsingOptions.Flexible with { SchemeNames = ["test"] },
        _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
    };
}

public enum TestCommandLineStyle
{
    Flexible,
    DotNet,
    Gnu,
    Posix,
    PowerShell,
    Url,
}
