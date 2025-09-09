using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Utils.Parsers;

internal readonly record struct CommandLineParsedResult(
    string PossibleCommandNames,
    OptionDictionary LongOptions,
    OptionDictionary ShortOptions,
    ReadOnlyListRange<string> Arguments)
{
    public static string MakePossibleCommandNames(IEnumerable<string> possibleCommandNames)
    {
        return string.Join(" ", possibleCommandNames.Select(x => NamingHelper.MakeKebabCase(x, false, false)));
    }

    public static string MakePossibleCommandNames(IEnumerable<string> commandLineArguments, int possibleCommandNamesLength)
    {
        return string.Join(" ", commandLineArguments.Take(possibleCommandNamesLength).Select(x => NamingHelper.MakeKebabCase(x, false, false)));
    }
}
