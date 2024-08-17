namespace dotnetCampus.Cli.Compiler;

/// <summary>
/// 标记一个属性为命令行选项。
/// </summary>
/// <param name="name">
/// 选项名称。必须使用 kebab-case 命名规则，且不带 -- 前缀。
/// </param>
/// <remarks>
/// 示例用法：
/// <code>
/// [Option("property-name")]
/// public required string PropertyName { get; init; }
/// </code>
/// 其中，<c>required</c> 为可选的修饰符，表示该选项为必填项；如果没有在命令行中传入，则会抛出异常或输出错误信息。
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class OptionAttribute(string name) : CommandLineAttribute
{
    /// <summary>
    /// 获取选项名称。
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 获取或设置选项的别名。
    /// </summary>
    /// <remarks>
    /// 可以指定短名称（如 `v`）或长名称（如 `verbose`）。单个字符的别名会被视为短名称。<br/>
    /// 如果指定区分大小写，但期望允许部分单词使用多种大小写，则应该在别名中指定多个大小写形式。如将 `verbose` 的别名指定为 `verbose Verbose VERBOSE`。
    /// </remarks>
    public string[] Aliases { get; init; } = [];
}
