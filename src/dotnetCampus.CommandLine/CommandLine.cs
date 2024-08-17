using System.Collections.Immutable;

namespace dotnetCampus.Cli;

/// <summary>
/// 为应用程序提供统一的命令行参数解析功能。
/// </summary>
public record CommandLine
{
    /// <summary>
    /// 获取此命令行解析类型所关联的命令行参数。
    /// </summary>
    public readonly ImmutableArray<string> Arguments;

    private CommandLine(ImmutableArray<string> arguments)
    {
        Arguments = arguments;
    }

    /// <summary>
    /// 解析命令行参数，并获得一个通用的命令行解析类型。
    /// </summary>
    /// <param name="args">命令行参数。</param>
    /// <returns>统一的命令行参数解析中间类型。</returns>
    public static CommandLine Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return new CommandLine([..args]);
    }

    public static CommandLine Parse(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new CommandLine(ImmutableArray<string>.Empty);
        }

        var parts = ImmutableArray.CreateBuilder<Range>();

        var start = 0;
        var length = 0;
        var quoteDepth = 0;
        for (var i = 0; i < arguments.Length; i++)
        {
            var c = arguments[i];
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

        return new CommandLine([..parts.Select(part => arguments[part])]);
    }

    public int Run<T>()
        where T : CommandLineParsingContext, new()
    {
        return 0;
    }
}

public abstract class CommandLineParsingContext
{
}
