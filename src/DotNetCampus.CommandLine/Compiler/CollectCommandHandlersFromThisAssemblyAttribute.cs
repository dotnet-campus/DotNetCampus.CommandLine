namespace DotNetCampus.Cli.Temp40.Compiler;

/// <summary>
/// 在一个 partial 类上标记，源生成器会自动查找此类型所在项目中所有支持的命令，并允许添加到 <see cref="CommandLine"/> 命令行解析器中执行。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CollectCommandHandlersFromThisAssemblyAttribute : Attribute
{
}
