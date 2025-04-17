using System.Web;
using DotNetCampus.Cli.Exceptions;
using DotNetCampus.Cli.Utils.Collections;

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
        if (commandLineArguments.Count is not 1)
        {
            throw new CommandLineParseException($"URL style parser expects exactly one argument, but got {commandLineArguments.Count}.");
        }

        var url = commandLineArguments[0];

        // 验证URL格式：scheme://[path][?query][#fragment]
        if (!url.StartsWith($"{_scheme}://", StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandLineParseException($"URL must start with '{_scheme}://'");
        }

        var longOptions = new OptionDictionary(true);
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
            longOptions.AddValue("fragment", fragment);
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
            longOptions,
            // URL 不支持短选项，所以直接使用空字典。
            OptionDictionary.Empty,
            arguments.ToReadOnlyList());
    }

    private static void ParseQueryString(string queryString, OptionDictionary options)
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
                options.AddOption(OptionName.MakeKebabCase(decodedName1.AsSpan()));
                continue;
            }

            // 处理有值参数 (如 ?name=value)
            int equalIndex = param.IndexOf('=');
            string name = param.Substring(0, equalIndex);
            string value = equalIndex + 1 < param.Length ? param.Substring(equalIndex + 1) : string.Empty;

            // URL解码参数名和值
            string decodedName = HttpUtility.UrlDecode(name);
            string decodedValue = HttpUtility.UrlDecode(value);

            options.AddValue(OptionName.MakeKebabCase(decodedName.AsSpan()), decodedValue);
        }
    }
}
