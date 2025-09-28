namespace DotNetCampus.CommandLine.CodeAnalysis;

/// <summary>
/// 从命令行解析参数的含义时，对于值（选项和位置参数）的类型的分类。<br/>
/// 源生成器给命令行对象的不同类型属性赋值时，只有这些类型才存在代码上的差异，其他类型都可映射到这些类型上。
/// </summary>
internal enum CommandValueKind
{
    /// <summary>
    /// 尚不知道是什么类型。
    /// </summary>
    Unknown,

    /// <summary>
    /// 布尔类型。
    /// </summary>
    Boolean,

    /// <summary>
    /// 数值类型，包括所有的整数和浮点数。
    /// </summary>
    Number,

    /// <summary>
    /// 枚举类型。
    /// </summary>
    Enum,

    /// <summary>
    /// 字符串类型。
    /// </summary>
    String,

    /// <summary>
    /// 列表类型。
    /// </summary>
    List,

    /// <summary>
    /// 字典类型。
    /// </summary>
    Dictionary,
}
