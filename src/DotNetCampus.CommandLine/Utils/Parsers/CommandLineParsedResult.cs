using DotNetCampus.Cli.Temp40.Utils.Collections;

namespace DotNetCampus.Cli.Temp40.Utils.Parsers;

internal readonly record struct CommandLineParsedResult(
    string PossibleCommandNames,
    OptionDictionary LongOptions,
    OptionDictionary ShortOptions,
    ReadOnlyListRange<string> Arguments)
{
    public static string MakePossibleCommandNames(IEnumerable<string> possibleCommandNames, bool isUpperSeparator)
    {
        return string.Join(" ", possibleCommandNames.Select(x => NamingHelper.MakeKebabCase(x, isUpperSeparator, false)));
    }

    public static string MakePossibleCommandNames(IEnumerable<string> commandLineArguments, int possibleCommandNamesLength, bool isUpperSeparator)
    {
        return string.Join(" ", commandLineArguments.Take(possibleCommandNamesLength).Select(x => NamingHelper.MakeKebabCase(x, isUpperSeparator, false)));
    }
}
