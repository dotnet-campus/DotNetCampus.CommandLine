using System;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

internal static class CommandLineStyleTestingExtensions
{
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
