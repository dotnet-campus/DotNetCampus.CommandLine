namespace dotnetCampus.Cli.Compiler;

/// <summary>
/// 标记一个属性为命令行选项。
/// </summary>
/// <remarks>
/// 示例用法：
/// <code>
/// [Option("property-name")]
/// public required string PropertyName { get; init; }
/// </code>
/// 其中，<c>required</c> 为可选的修饰符，表示该选项为必填项；如果没有在命令行中传入，则会抛出异常或输出错误信息。
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class OptionAttribute : CommandLineAttribute
{
    /// <summary>
    /// 标记一个属性为命令行选项，其长名称为属性名。
    /// </summary>
    public OptionAttribute()
    {
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称。
    /// </summary>
    /// <param name="shortName">选项的短名称。必须是单个字符。</param>
    public OptionAttribute(char shortName)
    {
        if (!char.IsLetter(shortName))
        {
            throw new ArgumentException($"选项的短名称必须是字母字符，但实际为 '{shortName}'。", nameof(shortName));
        }

        ShortName = shortName;
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称。
    /// </summary>
    /// <param name="longName">
    /// 选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。
    /// </param>
    public OptionAttribute(string longName)
    {
        LongName = longName;
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称和短名称。
    /// </summary>
    /// <param name="shortName">选项的短名称。必须是单个字符。</param>
    /// <param name="longName">
    /// 选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。
    /// </param>
    public OptionAttribute(char shortName, string longName)
    {
        if (!char.IsLetter(shortName))
        {
            throw new ArgumentException($"选项的短名称必须是字母字符，但实际为 '{shortName}'。", nameof(shortName));
        }

        LongName = longName;
        ShortName = shortName;
    }

    /// <summary>
    /// 获取或初始化选项的短名称。
    /// </summary>
    public char ShortName { get; } = '\0';

    /// <summary>
    /// 获取选项的长名称。
    /// </summary>
    public string? LongName { get; }

    /// <summary>
    /// 获取或设置选项的别名。
    /// </summary>
    /// <remarks>
    /// 可以指定短名称（如 `v`）或长名称（如 `verbose`）。单个字符的别名会被视为短名称。<br/>
    /// 如果指定区分大小写，但期望允许部分单词使用多种大小写，则应该在别名中指定多个大小写形式。如将 `verbose` 的别名指定为 `verbose Verbose VERBOSE`。
    /// </remarks>
    public string[] Aliases { get; init; } = [];

    /// <summary>
    /// 获取或设置是否大小写敏感。
    /// </summary>
    /// <remarks>
    /// 默认情况下使用 <see cref="CommandLine"/> 解析时所指定的大小写敏感性（而 <see cref="CommandLine"/> 默认为大小写不敏感）。
    /// </remarks>
    public bool CaseSensitive { get; init; }
}
