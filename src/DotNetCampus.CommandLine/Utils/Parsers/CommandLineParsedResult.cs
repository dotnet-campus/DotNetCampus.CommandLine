using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

internal readonly record struct CommandLineParsedResult(
    string? GuessedVerbName,
    IReadOnlyDictionary<string, SingleOptimizedList<string>> LongOptions,
    IReadOnlyDictionary<char, SingleOptimizedList<string>> ShortOptions,
    ReadOnlyListRange<string> Arguments);
