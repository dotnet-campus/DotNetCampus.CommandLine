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

        foreach (var argument in args)
        {
            if (IsHelpOption(argument, style))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsHelpOption(string argument, CommandLineStyle style)
    {
        var comparison = style.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        var prefix = style.OptionPrefix;

        // --help (DotNet, Gnu, Flexible)
        if (prefix is CommandOptionPrefix.DoubleDash or CommandOptionPrefix.Any)
        {
            if (style.SupportsLongOption && argument.Equals("--help", comparison))
            {
                return true;
            }
        }

        // -h (DotNet, Gnu, Posix, Flexible)
        if (prefix is CommandOptionPrefix.DoubleDash or CommandOptionPrefix.Any)
        {
            if (style.SupportsShortOption)
            {
                if (argument.Equals("-h", comparison))
                {
                    return true;
                }

                // 短选项组合 (Gnu, Posix): -hxxx contains -h
                if (style.SupportsShortOptionCombination && argument.Length > 2 && argument[0] == '-' && argument[1] != '-')
                {
                    var chars = argument.AsSpan(1);
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
            if (argument.Equals("/help", comparison))
            {
                return true;
            }
            if (argument.Equals("/h", comparison))
            {
                return true;
            }
            if (argument.Equals("/?", StringComparison.Ordinal))
            {
                return true;
            }
        }

        // -? (Flexible, Windows)
        if (prefix is CommandOptionPrefix.SlashOrDash or CommandOptionPrefix.Any)
        {
            if (argument.Equals("-?", StringComparison.Ordinal))
            {
                return true;
            }
        }
        // -? also supported for DoubleDash prefix in Flexible (which uses Any prefix)
        // Already covered by the Any case above.

        return false;
    }
}
