using System.Diagnostics;

namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 标记在一个 string[] 或 IReadOnlyList&lt;string&gt; 类型的属性上，表示此属性将接收保留的原始命令行参数。
/// </summary>
[Conditional("FOR_SOURCE_GENERATION_ONLY")]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class RawArgumentsAttribute : CommandLineAttribute
{
    /// <summary>
    /// 设置为 <see langword="true"/> 时，如果传入 URL，则会自动将其转换为普通的命令行参数列表（选项自动添加 -- 前缀，但不会改变命名规则）。<br/>
    /// 设置为 <see langword="false"/> 时，则不会转换 URL，Main 方法传入时收到什么就是什么。<br/>
    /// 默认值为 <see langword="true"/>。
    /// </summary>
    public bool ConvertUrl { get; set; } = true;
}
