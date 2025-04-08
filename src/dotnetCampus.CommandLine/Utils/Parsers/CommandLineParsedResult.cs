using System.Collections.Immutable;

namespace dotnetCampus.Cli.Utils.Parsers;

internal readonly ref struct CommandLineParsedResult(
    string? guessedVerbName,
    ImmutableDictionary<string, ImmutableArray<string>> longOptions,
    ImmutableDictionary<char, ImmutableArray<string>> shortOptions,
    ImmutableArray<string> arguments)
{
    public void Deconstruct(
        out string? verbName,
        out ImmutableDictionary<string, ImmutableArray<string>> longOptionValues,
        out ImmutableDictionary<char, ImmutableArray<string>> shortOptionValues,
        out ImmutableArray<string> positionalArguments)
    {
        verbName = guessedVerbName;
        longOptionValues = longOptions;
        shortOptionValues = shortOptions;
        positionalArguments = arguments;
    }
}
