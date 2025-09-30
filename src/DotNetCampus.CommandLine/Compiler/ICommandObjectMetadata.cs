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
