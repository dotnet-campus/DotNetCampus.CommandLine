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
/// 其中：
/// <list type="bullet">
/// <item><c>required</c> 为可选的修饰符，表示该选项为必填项；如果没有在命令行中传入，则会抛出异常或输出错误信息。</item>
/// </list>
/// 如果希望传入多个参数，则可以使用数组类型：
/// <code>
/// [Value(Length = int.MaxValue)]
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
