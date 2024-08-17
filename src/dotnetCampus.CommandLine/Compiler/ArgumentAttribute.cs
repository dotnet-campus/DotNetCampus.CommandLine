namespace dotnetCampus.Cli.Compiler;

/// <summary>
/// 标记一个属性为命令行变量。
/// </summary>
/// <remarks>
/// 示例用法：
/// <code>
/// [Argument]
/// public required string Argument { get; init; }
/// </code>
/// 如果支持传入多个参数，则可以使用数组类型：
/// <code>
/// [Argument]
/// public string[] Arguments { get; init; }
/// </code>
/// 常用的集合类型（包括接口）都是支持的，例如：
/// <list type="bullet">
/// <item><c>string[]</c></item>
/// <item><c>List&lt;string&gt;</c></item>
/// <item><c>IList&lt;string&gt;</c></item>
/// <item><c>IReadOnlyList&lt;string&gt;</c></item>
/// <item><c>ImmutableArray&lt;string&gt;</c></item>
/// <item><c>HashSet&lt;string&gt;</c></item>
/// <item><c>FrozenSet&lt;string&gt;</c></item>
/// <item><c>Dictionary&lt;string, string&gt;</c></item>
/// <item><c>ImmutableDictionary&lt;string, string&gt;</c></item>
/// </list>
/// 等。
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ArgumentAttribute : CommandLineAttribute;
