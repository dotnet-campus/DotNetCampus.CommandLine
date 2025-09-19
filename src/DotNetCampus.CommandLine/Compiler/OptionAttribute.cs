namespace DotNetCampus.Cli.Temp40.Compiler;

/// <summary>
/// 标记一个属性为命令行选项。
/// </summary>
/// <remarks>
/// 示例用法：
/// <code>
/// [Option("property-name")]
/// public required string PropertyName { get; init; }
/// </code>
/// 其中：
/// <list type="bullet">
/// <item><c>required</c> 为可选的修饰符，表示该选项为必填项；如果没有在命令行中传入，则会抛出异常或输出错误信息。</item>
/// <item>选项名称可以不指定，当不指定时将自动使用属性名。</item>
/// <item>选项名称建议使用 kebab-case 命名法（以获得更好的大小写和数字的区分度；当然，这并不影响实际使用，你仍可以使用其他命令行风格的命名法传入命令行参数）。</item>
/// </list>
/// 如果希望传入多个参数，则可以使用数组类型：
/// <code>
/// [Option("property-name")]
/// public ImmutableArray&lt;string&gt; Values { get; init; }
/// </code>
/// 数组、常用的只读/不可变集合类型（包括接口）都是支持的，例如：
/// <list type="bullet">
/// <item><c>string[]</c></item>
/// <item><c>IReadOnlyList&lt;string&gt;</c></item>
/// <item><c>ImmutableArray&lt;string&gt;</c></item>
/// <item><c>ImmutableHashSet&lt;string&gt;</c></item>
/// <item><c>IReadOnlyDictionary&lt;string, string&gt;</c></item>
/// <item><c>ImmutableDictionary&lt;string, string&gt;</c></item>
/// </list>
/// 对于字典类型的属性，命令行可通过如下方式传入：
/// <code>
/// do --property-name key1=value1 --property-name key2=value2
/// do --property-name:key1=value1;key2=value2
/// </code>
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

    /// <summary>
    /// 命令行参数中传入的选项名称必须严格保持与此属性中指定的长名称一致。
    /// </summary>
    /// <remarks>
    /// 默认情况下，我们会为了支持多种不同的命令行风格而自动识别选项的长名称，例如：
    /// <list type="bullet">
    /// <item>属性名 SampleProperty 可匹配：--Sample-Property --sample-property -SampleProperty</item>
    /// <item>属性名 sample-property 可匹配：--Sample-Property --sample-property -SampleProperty</item>
    /// </list>
    /// 但设置了此属性为 <see langword="true"/> 后，命令行中传入的选项名称必须完全一致：
    /// <list type="bullet">
    /// <item>属性名 SampleProperty 可匹配：--SampleProperty --sampleproperty -SampleProperty</item>
    /// <item>属性名 sample-property 可匹配：--Sample-Property --sample-property -Sample-Property</item>
    /// </list>
    /// </remarks>
    public bool ExactSpelling { get; init; }
}
