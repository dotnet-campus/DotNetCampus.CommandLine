namespace DotNetCampus.Cli.Utils.Parsers;

internal interface ICommandLineParser
{
    LegacyCommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments);
}
