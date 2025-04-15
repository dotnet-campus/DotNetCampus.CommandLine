using System.Collections.ObjectModel;
using System.Web;
using DotNetCampus.Cli.Exceptions;

namespace DotNetCampus.Cli.Utils.Parsers;

/// <summary>
/// URL风格的命令行参数解析器
/// 用于解析来自Web的URL格式命令行参数
/// </summary>
internal sealed class UrlStyleParser : ICommandLineParser
{
    private readonly string _scheme;

    /// <summary>
    /// 创建URL风格解析器
    /// </summary>
    /// <param name="scheme">URL方案名(scheme)</param>
    public UrlStyleParser(string scheme)
    {
        _scheme = scheme;
    }

    public CommandLineParsedResult Parse(IReadOnlyList<string> commandLineArguments)
    {
        if (commandLineArguments.Count != 1)
        {
            throw new CommandLineParseException("URL style parser expects exactly one argument.");
        }

        var url = commandLineArguments[0];

        // 验证URL格式：scheme://[path][?query][#fragment]
        if (!url.StartsWith($"{_scheme}://", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandLineParseException($"URL must start with '{_scheme}://'");
        }

        Dictionary<string, List<string>> longOptions = [];
        List<string> arguments = [];
        string? guessedVerbName = null;

        // 移除scheme://前缀
        string urlWithoutScheme = url.Substring(_scheme.Length + 3);

        // 分离fragment
        string urlWithoutFragment = urlWithoutScheme;

        int fragmentIndex = urlWithoutScheme.IndexOf('#');
        if (fragmentIndex >= 0)
        {
            urlWithoutFragment = urlWithoutScheme.Substring(0, fragmentIndex);
            var fragment = urlWithoutScheme.Substring(fragmentIndex + 1);

            // 添加fragment作为选项
            longOptions["fragment"] = [fragment];
        }

        // 分离查询参数和路径
        string path = urlWithoutFragment;
        int queryIndex = urlWithoutFragment.IndexOf('?');

        if (queryIndex >= 0)
        {
            path = urlWithoutFragment.Substring(0, queryIndex);
            string queryString = urlWithoutFragment.Substring(queryIndex + 1);

            // 解析查询字符串参数
            ParseQueryString(queryString, longOptions);
        }

        // 如果路径不为空，将其添加为位置参数
        if (!string.IsNullOrEmpty(path))
        {
            // URL解码路径
            string decodedPath = HttpUtility.UrlDecode(path);
            string[] pathSegments = decodedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            arguments.AddRange(pathSegments);

            // 猜测动词名称
            if (pathSegments.Length > 0)
            {
                guessedVerbName = pathSegments[0];
            }
        }

        return new CommandLineParsedResult(guessedVerbName,
            longOptions.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value.ToReadOnlyList()),
            // URL 不支持短选项，所以直接使用空字典。
#if NET8_0_OR_GREATER
            ReadOnlyDictionary<char, IReadOnlyList<string>>.Empty,
#else
            new Dictionary<char, IReadOnlyList<string>>(),
#endif
            arguments.ToReadOnlyList());
    }

    private static void ParseQueryString(string queryString, Dictionary<string, List<string>> options)
    {
        if (string.IsNullOrEmpty(queryString))
        {
            return;
        }

        string[] queryParams = queryString.Split('&');

        foreach (var param in queryParams)
        {
            // 处理无值参数 (如 ?debug)
            if (!param.Contains('='))
            {
                string decodedName1 = HttpUtility.UrlDecode(param);
                decodedName1 = NamingHelper.MakeKebabCase(decodedName1);
                options.TryAdd(decodedName1, []);
                options[decodedName1].Add("true");
                continue;
            }

            // 处理有值参数 (如 ?name=value)
            int equalIndex = param.IndexOf('=');
            string name = param.Substring(0, equalIndex);
            string value = equalIndex + 1 < param.Length ? param.Substring(equalIndex + 1) : string.Empty;

            // URL解码参数名和值
            string decodedName = HttpUtility.UrlDecode(name);
            string decodedValue = HttpUtility.UrlDecode(value);
            decodedName = NamingHelper.MakeKebabCase(decodedName);

            options.TryAdd(decodedName, []);
            options[decodedName].Add(decodedValue);
        }
    }
}
