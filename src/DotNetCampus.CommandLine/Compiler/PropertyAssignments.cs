namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 为源生成器解析命令行提供属性赋值辅助。
/// </summary>
/// <remarks>
/// 当然，这个接口只为了给所有的实现提供实现标准。<br/>
/// 由于所有的实现都是结构，所以不会有任何代码直接使用到这个接口。
/// </remarks>
public interface IPropertyAssignment
{

}

/// <summary>
/// 专门解析来自命令行的布尔类型，并辅助赋值给属性。
/// </summary>
public readonly struct BooleanPropertyAssignment
{

}

/// <summary>
/// 专门解析来自命令行的数值类型，并辅助赋值给属性。
/// </summary>
public readonly struct NumberPropertyAssignment
{

}

/// <summary>
/// 专门解析来自命令行的字符串类型，并辅助赋值给属性。
/// </summary>
public readonly struct StringPropertyAssignment
{

}

/// <summary>
/// 专门解析来自命令行的字符串集合类型，并辅助赋值给属性。
/// </summary>
public readonly struct StringsPropertyAssignment
{

}

/// <summary>
/// 专门解析来自命令行的字典类型，并辅助赋值给属性。
/// </summary>
public readonly struct DictionaryPropertyAssignment
{

}

/// <summary>
/// 在运行时解析来自命令行的枚举类型，并辅助赋值给属性。
/// </summary>
/// <remarks>
/// 源生成器会为各个枚举生成专门的编译时类型来处理枚举的赋值。<br/>
/// 此类型是为那些在运行时才知道枚举类型的场景准备的。
/// </remarks>
public readonly struct RuntimeEnumPropertyAssignment
{

}
