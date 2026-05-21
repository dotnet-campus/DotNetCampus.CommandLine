namespace DotNetCampus.Cli.Help;

/// <summary>
/// 根据命令行风格检测帮助请求。
/// </summary>
internal static class HelpDetector
{
    /// <summary>
    /// 检测命令行参数中是否包含帮助请求。
    /// </summary>
    public static bool IsHelpRequested(IReadOnlyList<string> args, CommandLineStyle style)
    {
        if (style.Name == "Url")
        {
            return false;
        }

        foreach (var arg in args)
        {
            if (IsHelpArg(arg, style))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 从参数列表中过滤掉帮助写法，返回剩余参数。
    /// </summary>
    public static List<string> FilterOutHelpArgs(IReadOnlyList<string> args, CommandLineStyle style)
    {
        var result = new List<string>(args.Count);
        foreach (var arg in args)
        {
            if (!IsHelpArg(arg, style))
            {
                result.Add(arg);
            }
        }
        return result;
    }

    private static bool IsHelpArg(string arg, CommandLineStyle style)
    {
        var comparison = style.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        var prefix = style.OptionPrefix;

        // --help (DotNet, Gnu, Flexible)
        if (prefix is CommandOptionPrefix.DoubleDash or CommandOptionPrefix.Any)
        {
            if (style.SupportsLongOption && arg.Equals("--help", comparison))
            {
                return true;
            }
        }

        // -h (DotNet, Gnu, Posix, Flexible)
        if (prefix is CommandOptionPrefix.DoubleDash or CommandOptionPrefix.Any)
        {
            if (style.SupportsShortOption)
            {
                if (arg.Equals("-h", comparison))
                {
                    return true;
                }

                // 短选项组合 (Gnu, Posix): -hxxx contains -h
                if (style.SupportsShortOptionCombination && arg.Length > 2 && arg[0] == '-' && arg[1] != '-')
                {
                    var chars = arg.AsSpan(1);
                    foreach (var c in chars)
                    {
                        if (c == 'h' || (!style.CaseSensitive && (c == 'H')))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        // /help, /h (Flexible, Windows)
        if (prefix is CommandOptionPrefix.Slash or CommandOptionPrefix.SlashOrDash or CommandOptionPrefix.Any)
        {
            if (arg.Equals("/help", comparison))
            {
                return true;
            }
            if (arg.Equals("/h", comparison))
            {
                return true;
            }
            if (arg.Equals("/?", StringComparison.Ordinal))
            {
                return true;
            }
        }

        // -? (Flexible, Windows)
        if (prefix is CommandOptionPrefix.SlashOrDash or CommandOptionPrefix.Any)
        {
            if (arg.Equals("-?", StringComparison.Ordinal))
            {
                return true;
            }
        }
        // -? also supported for DoubleDash prefix in Flexible (which uses Any prefix)
        // Already covered by the Any case above.

        return false;
    }
}
