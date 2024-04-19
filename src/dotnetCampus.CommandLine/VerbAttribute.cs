namespace dotnetCampus.Cli;

/// <summary>
/// 将一个类绑定一个命令行谓词。
/// </summary>
/// <param name="verbName">
/// 命令行谓词，可选命名风格有 PascalCase 和 kebab-case（不带 -- 前缀）；此命名规则并不影响实际用户使用时允许的命名风格，也不影响解析结果和性能。
/// 可选三种形式：
/// <list type="number">
/// <item>null、空字符串或空白字符串，表示默认命令行谓词。当在启动程序没有传入任何谓词时，会匹配此类型。例如 `dotnet --list-sdks`。</item>
/// <item>一个 PascalCase 风格的词组，表示一个一级命令行谓词。当启动程序传入的谓词与此相同时，会匹配此类型。例如 `dotnet build`。</item>
/// <item>多个 PascalCase 风格的词组以“/”分隔，表示一个多级命令行谓词。当启动程序传入多个谓词且逐一匹配时，会匹配此类型。例如 `dotnet sln add`。</item>
/// </list>
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VerbAttribute(string? verbName) : CommandLineAttribute
{
    /// <summary>
    /// 获取命令行谓词。
    /// </summary>
    public string? VerbName { get; } = verbName;
}
