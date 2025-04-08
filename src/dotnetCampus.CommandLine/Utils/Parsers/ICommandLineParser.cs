using System.Collections.Immutable;

namespace dotnetCampus.Cli.Utils.Parsers;

internal interface ICommandLineParser
{
    CommandLineParsedResult Parse(ImmutableArray<string> arguments);
}
