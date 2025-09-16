namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// 如果命令行参数中传入的是 URL，则尝试将其转换为普通的命令行参数列表。
/// </summary>
internal static class CommandUrlParser
{
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
    /// <param name="schema">URL 的 Scheme 部分。</param>
    /// <param name="argument">URL 字符串。</param>
    /// <returns>普通的命令行参数列表。</returns>
    private static IReadOnlyList<string> NormalizeUrlArguments(string schema, string argument)
    {
        // schema://command/subcommand/positional-argument1/positional-argument2?option1=value1&option2=value2

        var span = argument.AsSpan();

        // 1. 跳过 schema://
        span = span[(schema.Length + 3)..];

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

        var parts = argument.ToString().Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>(parts.Length);
        foreach (var part in parts)
        {
            result.Add(Uri.UnescapeDataString(part));
        }
        return result;
    }

    private static IReadOnlyList<string> ParseOptions(ReadOnlySpan<char> argument)
    {
        if (argument.IsEmpty)
        {
            return [];
        }

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
