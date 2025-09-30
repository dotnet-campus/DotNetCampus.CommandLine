using DotNetCampus.Cli.Utils.Parsers;

namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 提供给源生成器生成的类型实现，用于构建命令行对象 <see cref="ICommandObject"/> 的元数据。
/// </summary>
/// <remarks>
/// 源生成器生成元数据类型时，必须要求不包含任何字段（包括隐式字段）。
/// </remarks>
public interface ICommandObjectMetadata
{
    /// <summary>
    /// 构建命令行对象实例。
    /// </summary>
    /// <param name="context">包含此命令行对象创建时，命令行运行命令的相关信息。</param>
    /// <returns>命令行对象实例。</returns>
    object Build(CommandRunningContext context);
}

/// <summary>
/// 框架内部发现元数据不自带命令执行功能时，会在内部寻找适合的类型代理执行命令。
/// </summary>
internal interface ICommandHandlerMetadata : ICommandObjectMetadata
{
    /// <summary>
    /// 运行命令处理器。
    /// </summary>
    /// <param name="createdCommandObject">通过 <see cref="ICommandObjectMetadata.Build(CommandRunningContext)"/> 创建的命令行对象实例。</param>
    /// <returns>命令处理器的返回值。</returns>
    Task<int> RunAsync(object createdCommandObject);
}

/// <summary>
/// 提供给源生成器生成的类型实现，用于在解析命令行的过程中不断配合赋值，最终生成完整的命令行对象。
/// </summary>
public interface ICommandObjectBuilder
{
    /// <summary>
    /// 匹配长选项。
    /// </summary>
    /// <param name="longOption">来自用户输入的命令行长选项，不包含前导的两个短横线（--）或任何其他允许的前缀。</param>
    /// <param name="defaultCaseSensitive">指示此命令行对象的选项默认是否区分大小写。</param>
    /// <param name="namingPolicy">命名策略，用于将用户输入的选项名称转换为属性名称。</param>
    /// <returns>匹配结果。</returns>
    OptionValueMatch MatchLongOption(ReadOnlySpan<char> longOption, bool defaultCaseSensitive, CommandNamingPolicy namingPolicy);

    /// <summary>
    /// 匹配短选项。
    /// </summary>
    /// <param name="shortOption">来自用户输入的命令行短选项，不包含前导的单个短横线（-）或任何其他允许的前缀。</param>
    /// <param name="defaultCaseSensitive">指示此命令行对象的选项默认是否区分大小写。</param>
    /// <returns>匹配结果。</returns>
    OptionValueMatch MatchShortOption(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive);

    /// <summary>
    /// 匹配位置参数。
    /// </summary>
    /// <param name="value">来自用户输入的命令行位置参数。</param>
    /// <param name="argumentIndex">位置参数的索引，从 0 开始。</param>
    /// <returns>匹配结果。</returns>
    PositionalArgumentValueMatch MatchPositionalArguments(ReadOnlySpan<char> value, int argumentIndex);

    /// <summary>
    /// 为指定的属性赋值。
    /// </summary>
    /// <param name="propertyName">属性名，仅用于调试和日志记录。</param>
    /// <param name="propertyIndex">属性索引，源生成器应该根据此索引快速定位到对应的属性。</param>
    /// <param name="key">如果选项是字典类型的，则为字典的键，否则为空。</param>
    /// <param name="value">要赋予属性的值；如果属性是布尔类型且选项没有显式提供值，则此值为空；如果选项是字典类型的，则为字典的值。</param>
    void AssignPropertyValue(string propertyName, int propertyIndex, ReadOnlySpan<char> key, ReadOnlySpan<char> value);
}
