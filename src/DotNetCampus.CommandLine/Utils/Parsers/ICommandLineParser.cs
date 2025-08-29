namespace DotNetCampus.Cli.Utils.Parsers;

internal interface ICommandLineParser
{
    CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments);
}
