using System.Collections.Immutable;

namespace DotNetCampus.Cli.Utils.Parsers;

internal readonly record struct CommandLineParsedResult(
    string? GuessedVerbName,
    ImmutableDictionary<string, ImmutableArray<string>> LongOptions,
    ImmutableDictionary<char, ImmutableArray<string>> ShortOptions,
    ImmutableArray<string> Arguments);
