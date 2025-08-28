namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 标记在一个 string[] 或 IReadOnlyList&lt;string&gt; 类型的属性上，表示此属性将接收保留的原始命令行参数。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class RawArgumentsAttribute : CommandLineAttribute
{
}
