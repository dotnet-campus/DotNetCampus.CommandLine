using System.Runtime.CompilerServices;
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
    private const string FragmentName = "fragment";
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

        var longOptions = new OptionDictionary(true);
        var shortOptions = new OptionDictionary(true);
        string? guessedVerbName = null;
        List<string> arguments = [];

        string? lastParameterName = null;
        var lastType = UrlParsedType.Start;

        for (var i = 0; i < url.Length;)
        {
            var result = UrlPart.ReadNext(url, ref i, lastType);
            lastType = result.Type;

            if (result.Type is UrlParsedType.VerbOrPositionalArgument)
            {
                lastParameterName = null;
                guessedVerbName = result.Value;
                arguments.Add(guessedVerbName);
                continue;
            }

            if (result.Type is UrlParsedType.PositionalArgument)
            {
                lastParameterName = null;
                arguments.Add(result.Value);
                continue;
            }

            if (result.Type is UrlParsedType.ParameterName)
            {
                lastParameterName = result.Name;
                longOptions.AddOption(result.Name);
                continue;
            }

            if (result.Type is UrlParsedType.ParameterValue)
            {
                if (lastParameterName is null)
                {
                    throw new CommandLineParseException($"Invalid URL format: {url}. Parameter value '{result.Value}' without a name.");
                }

                longOptions.AddValue(lastParameterName, result.Value);
                lastParameterName = null;
                continue;
            }

            if (result.Type is UrlParsedType.Fragment)
            {
                lastParameterName = null;
                longOptions.AddValue(result.Name, result.Value);
                continue;
            }
        }

        return new CommandLineParsedResult(guessedVerbName,
            longOptions,
            shortOptions,
            arguments.ToReadOnlyList());
    }


    internal readonly ref struct UrlPart(UrlParsedType type)
    {
        public UrlParsedType Type { get; } = type;
        public string Name { get; private init; } = "";
        public string Value { get; private init; } = "";

        public static UrlPart ReadNext(string url, ref int index, UrlParsedType lastType)
        {
            if (lastType is UrlParsedType.Start)
            {
                // 取出第一个位置参数（或谓词）
                var startIndex = -1;
                for (var i = index; i < url.Length - 3; i++)
                {
                    if (url[i] is ':' && url[i + 1] is '/' && url[i + 2] is '/')
                    {
                        startIndex = i + 3;
                        break;
                    }
                }
                if (startIndex < 0)
                {
                    throw new CommandLineParseException($"Invalid URL format: {url}. Missing '://'");
                }
                var endIndex = url.IndexOfAny(['/', '?', '#', '&'], startIndex);
                if (endIndex < 0)
                {
                    endIndex = url.Length;
                    index = endIndex + 1;
                }
                else
                {
                    index = endIndex;
                }
                var value = HttpUtility.UrlDecode(url.AsSpan(startIndex, endIndex - startIndex).ToString());
                return new UrlPart(UrlParsedType.VerbOrPositionalArgument)
                {
                    Value = value,
                };
            }

            if (lastType is UrlParsedType.VerbOrPositionalArgument or UrlParsedType.PositionalArgument)
            {
                return url[index] switch
                {
                    // 新的位置参数。
                    '/' => ReadNextPositionalArgument(url, ref index),
                    // 查询参数名。
                    '?' => ReadNextParameterName(url, ref index),
                    // 片段。
                    '#' => ReadFragment(url, ref index),
                    _ => throw new CommandLineParseException($"Invalid URL format: {url}. Expected '/', '?' or '#' after a positional argument."),
                };
            }

            if (lastType is UrlParsedType.ParameterName)
            {
                return url[index] switch
                {
                    // 查询参数值。
                    '=' => ReadNextParameterValue(url, ref index),
                    // 查询新的参数名。
                    '&' => ReadNextParameterName(url, ref index),
                    // 片段。
                    '#' => ReadFragment(url, ref index),
                    _ => throw new CommandLineParseException($"Invalid URL format: {url}. Expected '=', '&' or '#' after a parameter name."),
                };
            }

            if (lastType is UrlParsedType.ParameterValue)
            {
                return url[index] switch
                {
                    // 查询新的参数名。
                    '&' => ReadNextParameterName(url, ref index),
                    // 片段。
                    '#' => ReadFragment(url, ref index),
                    _ => throw new CommandLineParseException($"Invalid URL format: {url}. Expected '&' or '#' after a parameter value."),
                };
            }

            throw new CommandLineParseException($"Invalid URL format: {url}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UrlPart ReadNextPositionalArgument(string url, ref int index)
        {
            var startIndex = index;
            var endIndex = url.IndexOfAny(['/', '?', '#', '&'], startIndex + 1);
            if (endIndex < 0)
            {
                endIndex = url.Length;
                index = endIndex + 1;
            }
            else
            {
                index = endIndex;
            }
            var value = HttpUtility.UrlDecode(url.AsSpan(startIndex + 1, endIndex - startIndex - 1).ToString());
            index = endIndex;
            return new UrlPart(UrlParsedType.PositionalArgument)
            {
                Value = value,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UrlPart ReadNextParameterName(string url, ref int index)
        {
            var startIndex = index;
            var endIndex = url.IndexOfAny(['=', '#', '&'], index + 1);
            if (endIndex < 0)
            {
                endIndex = url.Length;
                index = endIndex + 1;
            }
            else
            {
                index = endIndex;
            }
            var value = HttpUtility.UrlDecode(url.AsSpan(startIndex + 1, endIndex - startIndex - 1).ToString());
            index = endIndex;
            return new UrlPart(UrlParsedType.ParameterName)
            {
                Name = OptionName.MakeKebabCase(value
#if !NETCOREAPP3_1_OR_GREATER
                        .AsSpan()
#endif
                ),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UrlPart ReadNextParameterValue(string url, ref int index)
        {
            var startIndex = index;
            var endIndex = url.IndexOfAny(['&', '#'], index + 1);
            if (endIndex < 0)
            {
                endIndex = url.Length;
                index = endIndex + 1;
            }
            else
            {
                index = endIndex;
            }
            var value = HttpUtility.UrlDecode(url.AsSpan(startIndex + 1, endIndex - startIndex - 1).ToString());
            index = endIndex;
            return new UrlPart(UrlParsedType.ParameterValue)
            {
                Value = value,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UrlPart ReadFragment(string url, ref int index)
        {
            var startIndex = index;
            index =  url.Length + 1;
            return new UrlPart(UrlParsedType.Fragment)
            {
                Name = FragmentName,
                Value = HttpUtility.UrlDecode(url.AsSpan(startIndex + 1).ToString()),
            };
        }
    }
}

internal enum UrlParsedType
{
    /// <summary>
    /// 尚未开始解析。
    /// </summary>
    Start,

    /// <summary>
    /// 第一个位置参数，也可能是谓词。
    /// </summary>
    VerbOrPositionalArgument,

    /// <summary>
    /// 位置参数。
    /// </summary>
    PositionalArgument,

    /// <summary>
    /// 查询参数名。
    /// </summary>
    ParameterName,

    /// <summary>
    /// 查询参数值。
    /// </summary>
    ParameterValue,

    /// <summary>
    /// 片段参数名。
    /// </summary>
    Fragment,
}
