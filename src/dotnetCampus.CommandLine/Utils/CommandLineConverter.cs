using System.Collections.Immutable;
using dotnetCampus.Cli.Utils.Parsers;

namespace dotnetCampus.Cli.Utils;

/// <summary>
/// 命令行参数转换器。
/// </summary>
internal static class CommandLineConverter
{
    /// <summary>
    /// 将一整行命令转换为命令行参数数组。
    /// </summary>
    /// <param name="singleLineCommandLineArgs">一整行命令。</param>
    /// <returns>命令行参数数组。</returns>
    internal static ImmutableArray<string> SingleLineCommandLineArgsToArrayCommandLineArgs(string singleLineCommandLineArgs)
    {
        if (string.IsNullOrWhiteSpace(singleLineCommandLineArgs))
        {
            return ImmutableArray<string>.Empty;
        }

        var parts = ImmutableArray.CreateBuilder<Range>();

        var start = 0;
        var length = 0;
        var quoteDepth = 0;
        for (var i = 0; i < singleLineCommandLineArgs.Length; i++)
        {
            var c = singleLineCommandLineArgs[i];
            if (quoteDepth == 0)
            {
                if (c == ' ')
                {
                    if (length > 0)
                    {
                        parts.Add(new Range(start, start + length));
                    }

                    start = i + 1;
                    length = 0;
                }
                else if (c == '"')
                {
                    quoteDepth++;
                }
                else
                {
                    length++;
                }
            }
            else
            {
                if (c == '"')
                {
                    quoteDepth--;
                }
            }
        }

        if (length > 0)
        {
            parts.Add(new Range(start, start + length));
        }

        return [..parts.Select(part => singleLineCommandLineArgs[part])];
    }

    public static CommandLineParsedResult ParseCommandLineArguments(ImmutableArray<string> arguments, CommandLineParsingOptions? parsingOptions)
    {
        var style = parsingOptions?.Style ?? CommandLineStyle.Auto;
        var parser = new GnuStyleParser();
        return parser.Parse(arguments);
    }
}
