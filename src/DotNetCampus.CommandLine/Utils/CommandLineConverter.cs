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
    internal static IReadOnlyList<string> SingleLineToList(string singleLineCommandLineArgs)
    {
        if (string.IsNullOrWhiteSpace(singleLineCommandLineArgs))
        {
            return [];
        }

        List<string> arguments = [];

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
                        arguments.Add(singleLineCommandLineArgs[start..(start + length)]);
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
            arguments.Add(singleLineCommandLineArgs[start..(start + length)]);
        }

        return arguments;
    }

    /// <summary>
    /// 尝试将命令行参数中的 URL 转换为普通的命令行参数列表。
    /// </summary>
    /// <param name="originalArguments">原始传入的命令行参数。</param>
    /// <param name="options">命令行解析选项。</param>
    /// <returns>如果传入的命令行参数中包含 URL，则返回转换后的命令行参数列表和 URL 的 Scheme 部分。</returns>
    internal static (string? MatchedUrlScheme, IReadOnlyList<string>? UrlNormalizedArguments) TryNormalizeUrlArguments(
        IReadOnlyList<string> originalArguments, CommandLineParsingOptions options)
    {
        if (originalArguments.Count is not 1 || options.SchemeNames is not { Count: > 0 } schemeNames)
        {
            return (null, null);
        }

        var argument = originalArguments[0];
        foreach (var schemeName in schemeNames)
        {
            if (argument.StartsWith($"{schemeName}://", StringComparison.OrdinalIgnoreCase))
            {
                return (schemeName, NormalizeUrlArguments(schemeName, argument));
            }
        }
        return (null, null);
    }

    /// <summary>
    /// 将 URL 转换为普通的命令行参数列表。<br/>
    /// </summary>
    /// <param name="scheme">URL 的 Scheme 部分。</param>
    /// <param name="argument">URL 字符串。</param>
    /// <returns>普通的命令行参数列表。</returns>
    private static IReadOnlyList<string> NormalizeUrlArguments(string scheme, string argument)
    {
        // scheme://command/subcommand/positional-argument1/positional-argument2?option1=value1&option2=value2

        var span = argument.AsSpan();

        // 1. 跳过 scheme://
        span = span[(scheme.Length + 3)..];

        // 2. 分成三个部分，分别解析。
        var questionMarkIndex = span.IndexOf('?');
        var fragmentIndex = span.IndexOf('#');
        var commandAndPositionalArgumentSpan = questionMarkIndex switch
        {
            -1 when fragmentIndex == -1 => span,
            -1 => span[..fragmentIndex],
            _ when fragmentIndex == -1 => span[..questionMarkIndex],
            _ => span[..Math.Min(questionMarkIndex, fragmentIndex)],
        };
        var optionSpan = questionMarkIndex switch
        {
            -1 => [],
            _ when fragmentIndex == -1 => span[(questionMarkIndex + 1)..],
            _ => span[(questionMarkIndex + 1)..fragmentIndex],
        };
        var fragmentSpan = fragmentIndex switch
        {
            -1 => [],
            _ => span[(fragmentIndex + 1)..],
        };

        // 3. 解析各个部分。
        var commandAndPositionalArgumentList = ParseCommandAndPositionalArguments(commandAndPositionalArgumentSpan);
        var optionList = ParseOptions(optionSpan);
        var fragmentList = ParseFragment(fragmentSpan);

        return [..commandAndPositionalArgumentList, ..optionList, ..fragmentList];
    }

    private static IReadOnlyList<string> ParseCommandAndPositionalArguments(ReadOnlySpan<char> argument)
    {
        if (argument.IsEmpty)
        {
            return [];
        }

#if NET8_0_OR_GREATER
        Span<Range> ranges = stackalloc Range[argument.Count('/') + 1];
        var count = argument.Split(ranges, '/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var part = argument[ranges[i]];
            result.Add(Uri.UnescapeDataString(part.ToString()));
        }
        return result;
#else
        var parts = argument.ToString().Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        var result = parts.Select(Uri.UnescapeDataString).ToList();
        return result;
#endif
    }

    private static IReadOnlyList<string> ParseOptions(ReadOnlySpan<char> argument)
    {
        if (argument.IsEmpty)
        {
            return [];
        }

#if NET8_0_OR_GREATER
        Span<Range> ranges = stackalloc Range[argument.Count('&') + 1];
        var count = argument.Split(ranges, '/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var part = argument[ranges[i]];
            var equalSignIndex = part.IndexOf('=');
            if (equalSignIndex < 0)
            {
                // 只有键，没有值
                result.Add($"--{Uri.UnescapeDataString(part.ToString())}");
            }
            else
            {
                var key = part[..equalSignIndex].ToString();
                var value = part[(equalSignIndex + 1)..].ToString();
                result.Add($"--{Uri.UnescapeDataString(key)}={Uri.UnescapeDataString(value)}");
            }
        }
        return result;
#else
        var parts = argument.ToString().Split(['&'], StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>(parts.Length);
        foreach (var part in parts)
        {
            var equalSignIndex = part.IndexOf('=');
            if (equalSignIndex == -1)
            {
                // 只有键，没有值
                result.Add($"--{Uri.UnescapeDataString(part)}");
            }
            else
            {
                var key = part[..equalSignIndex];
                var value = part[(equalSignIndex + 1)..];
                result.Add($"--{Uri.UnescapeDataString(key)}={Uri.UnescapeDataString(value)}");
            }
        }
        return result;
#endif
    }

    private static IReadOnlyList<string> ParseFragment(ReadOnlySpan<char> argument)
    {
        if (argument.IsEmpty)
        {
            return [];
        }

        // 片段部分直接作为一个位置参数
        return ["--fragment", Uri.UnescapeDataString(argument.ToString())];
    }
}
