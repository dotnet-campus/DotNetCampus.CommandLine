namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 将一个类绑定一个命令行命令。使用空格（` `）分隔多级子命令。
/// 例如：
/// <code>
/// // 匹配命令行：dotnet build
/// [Command("build")]
/// public class BuildCommand { ... }
/// </code>
/// <code>
/// // 匹配命令行：dotnet sln add
/// [Command("sln add")]
/// public class SlnAddCommand { ... }
/// </code>
/// </summary>
/// <param name="names">
/// 命令。必须使用 kebab-case 命名规则，且不带 -- 前缀。
/// 可选三种形式：
/// <list type="number">
/// <item>不标记此 Attribute，或者标记但传入 <see langword="null"/>、空字符串或空白字符串，表示默认命令。当在启动程序没有传入任何命令时，会匹配此类型。例如 `dotnet --list-sdks`。</item>
/// <item>一个 kebab-case 风格的词组，表示一个主命令（Main Command）。当启动程序传入的命令与此相同时，会匹配此类型。例如 `dotnet build`。</item>
/// <item>多个 kebab-case 风格的词组以空格（` `）分隔，表示一个子命令或多级子命令。当启动程序传入多个命令且逐一匹配时，会匹配此类型。例如 `dotnet sln add`。</item>
/// </list>
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class CommandAttribute(string? names = null) : CommandLineAttribute
{
    /// <summary>
    /// 获取命令行的命令，可以是单个词组的主命令（Main Command），也可以是多个词组的子命令或多级子命令（Sub Command）。
    /// </summary>
    public string? Names { get; } = names;
}

/// <summary>
/// 将一个类绑定一个命令行命令。使用空格（` `）分隔多级子命令。
/// </summary>
/// <param name="name"></param>
[Obsolete("因为子命令（MainCommand/SubCommand）具有更主流和广泛的认知，所以我们采用新名字 CommandAttribute 来替代 VerbAttribute。")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VerbAttribute(string? name) : CommandLineAttribute
{
    /// <summary>
    /// 获取命令行的命令，可以是单个词组的主命令（Main Command），也可以是多个词组的子命令或多级子命令（Sub Command）。
    /// </summary>
    public string? Name { get; } = name;
}
