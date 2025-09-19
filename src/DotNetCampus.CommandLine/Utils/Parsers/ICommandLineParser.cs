namespace DotNetCampus.Cli.Temp40.Utils.Parsers;

internal interface ICommandLineParser
{
    CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments);
}
