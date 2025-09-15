namespace DotNetCampus.Cli.Compiler;

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
        ShortNames = [];
        LongNames = [];
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的短名称。
    /// </summary>
    /// <param name="shortName">选项的短名称。必须是单个字符。</param>
    public OptionAttribute(char shortName)
    {
        ShortNames = [shortName.ToString()];
        LongNames = [];
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称。
    /// </summary>
    /// <param name="longName">选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。</param>
    public OptionAttribute(string longName)
    {
        ShortNames = [];
        LongNames = [longName];
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称和短名称。
    /// </summary>
    /// <param name="shortName">选项的短名称。必须是单个字符。</param>
    /// <param name="longName">选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。</param>
    public OptionAttribute(char shortName, string longName)
    {
        ShortNames = [shortName.ToString()];
        LongNames = [longName];
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称和短名称。
    /// </summary>
    /// <param name="shortName">选项的短名称。必须是单个字符。</param>
    /// <param name="longNames">选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。</param>
    public OptionAttribute(char shortName, string[] longNames)
    {
        ShortNames = [shortName.ToString()];
        LongNames = longNames;
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称和短名称。
    /// </summary>
    /// <param name="shortName">支持多字符的多个短名称，如用 -tl 来表示 --terminal-logger。</param>
    /// <param name="longName">选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。</param>
    public OptionAttribute(string shortName, string longName)
    {
        ShortNames = [shortName];
        LongNames = [longName];
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称和短名称。
    /// </summary>
    /// <param name="shortName">支持多字符的多个短名称，如用 -tl 来表示 --terminal-logger。</param>
    /// <param name="longNames">选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。</param>
    public OptionAttribute(string shortName, string[] longNames)
    {
        ShortNames = [shortName];
        LongNames = longNames;
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称和短名称。
    /// </summary>
    /// <param name="shortNames">支持多字符的多个短名称，如用 -tl 来表示 --terminal-logger。</param>
    /// <param name="longName">选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。</param>
    public OptionAttribute(string[] shortNames, string longName)
    {
        ShortNames = shortNames;
        LongNames = [longName];
    }

    /// <summary>
    /// 标记一个属性为命令行选项，并具有指定的长名称和短名称。
    /// </summary>
    /// <param name="shortNames">支持多字符的多个短名称，如用 -tl 来表示 --terminal-logger。</param>
    /// <param name="longNames">选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。</param>
    public OptionAttribute(string[] shortNames, string[] longNames)
    {
        ShortNames = shortNames;
        LongNames = longNames;
    }

    /// <summary>
    /// 获取或初始化选项的短名称。
    /// </summary>
    public string[] ShortNames { get; }

    /// <summary>
    /// 获取选项的长名称。
    /// </summary>
    public string[] LongNames { get; }

    /// <summary>
    /// 获取或设置是否大小写敏感。
    /// </summary>
    /// <remarks>
    /// 默认情况下使用 <see cref="CommandLine"/> 解析时所指定的大小写敏感性（而 <see cref="CommandLine"/> 默认为大小写不敏感）。
    /// </remarks>
    public bool CaseSensitive { get; init; }
}
