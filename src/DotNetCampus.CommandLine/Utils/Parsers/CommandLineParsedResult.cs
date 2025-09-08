using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

internal readonly record struct CommandLineParsedResult(
    string PossibleCommandNames,
    OptionDictionary LongOptions,
    OptionDictionary ShortOptions,
    ReadOnlyListRange<string> Arguments);
