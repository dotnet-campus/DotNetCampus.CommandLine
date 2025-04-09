namespace dotnetCampus.Cli.Compiler;

/// <summary>
/// 标记一个属性为命令行位置参数。
/// </summary>
/// <remarks>
/// 示例用法：
/// <code>
/// [Value]
/// public required string Value { get; init; }
/// </code>
/// 如果支持传入多个参数，则可以使用数组类型：
/// <code>
/// [Value(Length = int.MaxValue)]
/// public string[] Values { get; init; }
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
public sealed class ValueAttribute : CommandLineAttribute
{
    /// <summary>
    /// 获取位置参数的索引。
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// 获取或初始化从索引处开始的参数个数。
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 标记一个属性为命令行位置参数，其接收第一个位置参数。
    /// </summary>
    public ValueAttribute()
    {
        Index = 0;
        Length = 1;
    }

    /// <summary>
    /// 标记一个属性为命令行位置参数，其接收指定索引的参数。
    /// </summary>
    /// <param name="index">指定的位置参数的索引。</param>
    public ValueAttribute(int index)
    {
        Index = index;
        Length = 1;
    }
}
