using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

internal readonly record struct CommandLineParsedResult(
    string? GuessedVerbName,
    OptionDictionary LongOptions,
    OptionDictionary ShortOptions,
    ReadOnlyListRange<string> Arguments);
