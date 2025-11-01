using System;

namespace DotNetCampus.Cli.Tests;

using UH = UnknownCommandArgumentHandling;

internal static class CommandLineStyleTestingExtensions
{
    public static CommandLineParsingOptions ToParsingOptions(this TestCommandLineStyle style) => style switch
    {
        TestCommandLineStyle.Flexible => CommandLineParsingOptions.Flexible with { UnknownArgumentsHandling = UH.AllArgumentsMustBeRecognized },
        TestCommandLineStyle.DotNet => CommandLineParsingOptions.DotNet with { UnknownArgumentsHandling = UH.AllArgumentsMustBeRecognized },
        TestCommandLineStyle.Gnu => CommandLineParsingOptions.Gnu with { UnknownArgumentsHandling = UH.AllArgumentsMustBeRecognized },
        TestCommandLineStyle.Posix => CommandLineParsingOptions.Posix with { UnknownArgumentsHandling = UH.AllArgumentsMustBeRecognized },
        TestCommandLineStyle.Windows => CommandLineParsingOptions.Windows with { UnknownArgumentsHandling = UH.AllArgumentsMustBeRecognized },
        TestCommandLineStyle.Url => CommandLineParsingOptions.Flexible with
        {
            SchemeNames = ["test"],
            UnknownArgumentsHandling = UH.AllArgumentsMustBeRecognized,
        },
        _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
    };
}

public enum TestCommandLineStyle
{
    Flexible,
    DotNet,
    Gnu,
    Posix,
    Windows,
    Url,
}
