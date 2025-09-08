namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 将一个类绑定一个命令行谓词。可使用空格分隔多个谓词，表示多级命令行谓词。
/// 例如：
/// <code>
/// // 匹配命令行：dotnet build
/// [Verb("build")]
/// public class BuildCommand { ... }
/// </code>
/// <code>
/// // 匹配命令行：dotnet sln add
/// [Verb("sln/add")]
/// public class SlnAddCommand { ... }
/// </code>
/// </summary>
/// <param name="name">
/// 命令行谓词。必须使用 kebab-case 命名规则，且不带 -- 前缀。
/// 可选三种形式：
/// <list type="number">
/// <item>不标记此 Attribute，或者标记但传入 <see langword="null"/>、空字符串或空白字符串，表示默认命令行谓词。当在启动程序没有传入任何谓词时，会匹配此类型。例如 `dotnet --list-sdks`。</item>
/// <item>一个 kebab-case 风格的词组，表示一个一级命令行谓词。当启动程序传入的谓词与此相同时，会匹配此类型。例如 `dotnet build`。</item>
/// <item>多个 kebab-case 风格的词组以“/”分隔，表示一个多级命令行谓词。当启动程序传入多个谓词且逐一匹配时，会匹配此类型。例如 `dotnet sln add`。</item>
/// </list>
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VerbAttribute(string? name) : CommandLineAttribute
{
    /// <summary>
    /// 获取命令行谓词。
    /// </summary>
    public string? Name { get; } = name;
}
