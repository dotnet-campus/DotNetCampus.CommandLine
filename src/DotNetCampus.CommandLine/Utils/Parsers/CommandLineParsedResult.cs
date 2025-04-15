namespace DotNetCampus.Cli.Utils.Parsers;

internal readonly record struct CommandLineParsedResult(
    string? GuessedVerbName,
    IReadOnlyDictionary<string, IReadOnlyList<string>> LongOptions,
    IReadOnlyDictionary<char, IReadOnlyList<string>> ShortOptions,
    ReadOnlyListRange<string> Arguments);
