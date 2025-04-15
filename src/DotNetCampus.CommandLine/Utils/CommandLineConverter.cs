using System.Collections.Immutable;
using DotNetCampus.Cli.Utils.Parsers;

namespace DotNetCampus.Cli.Utils;

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
    internal static IReadOnlyList<string> SingleLineCommandLineArgsToArrayCommandLineArgs(string singleLineCommandLineArgs)
    {
        if (string.IsNullOrWhiteSpace(singleLineCommandLineArgs))
        {
            return ImmutableArray<string>.Empty;
        }

        List<Range> parts = [];

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

    public static (string? MatchedUrlScheme, CommandLineParsedResult Result) ParseCommandLineArguments(
        IReadOnlyList<string> arguments, CommandLineParsingOptions? parsingOptions)
    {
        var matchedUrlScheme = arguments.Count is 1 && parsingOptions?.SchemeNames is { Length: > 0 } schemeNames
            ? schemeNames.FirstOrDefault(x => arguments[0].StartsWith($"{x}://", StringComparison.OrdinalIgnoreCase))
            : null;

        ICommandLineParser parser = (matchUrlScheme: matchedUrlScheme, parsingOptions?.Style) switch
        {
            ({ } scheme, _) => new UrlStyleParser(scheme),
            (_, CommandLineStyle.Flexible) => new FlexibleStyleParser(),
            (_, CommandLineStyle.GNU) => new GnuStyleParser(),
            (_, CommandLineStyle.POSIX) => new PosixStyleParser(),
            (_, CommandLineStyle.DotNet) => new DotNetStyleParser(),
            (_, CommandLineStyle.PowerShell) => new PowerShellStyleParser(),
            _ => new FlexibleStyleParser(),
        };
        return (matchedUrlScheme, parser.Parse(arguments));
    }
}
