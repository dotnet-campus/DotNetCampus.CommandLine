using System.Collections.Immutable;

namespace DotNetCampus.Cli.Utils.Parsers;

internal interface ICommandLineParser
{
    CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments);
}
