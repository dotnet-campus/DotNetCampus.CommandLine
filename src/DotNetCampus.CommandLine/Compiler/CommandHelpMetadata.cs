namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 命令的帮助元数据。
/// </summary>
public sealed class CommandHelpMetadata
{
    /// <summary>
    /// 命令的名称（如 "add" 或 "remote add"）。对于默认命令，此属性为 <see langword="null"/>。
    /// </summary>
    public required string? CommandName { get; init; }

    /// <summary>
    /// 命令的描述。如果命令没有指定 <see cref="CommandLineAttribute.Description"/>，则此属性为 <see langword="null"/>。
    /// </summary>
    public required string? Description { get; init; }

    /// <summary>
    /// 此命令的选项帮助信息列表。没有选项时为空集合。
    /// </summary>
    public required IReadOnlyList<OptionHelpInfo> Options { get; init; }

    /// <summary>
    /// 此命令的位置参数帮助信息列表。没有位置参数时为空集合。
    /// </summary>
    public required IReadOnlyList<ValueHelpInfo> PositionalArguments { get; init; }
}

/// <summary>
/// 单个选项的帮助信息。
/// </summary>
public readonly record struct OptionHelpInfo
{
    /// <summary>
    /// 选项的短名称列表。
    /// </summary>
    public required IReadOnlyList<string> ShortNames { get; init; }

    /// <summary>
    /// 选项的长名称列表。
    /// </summary>
    public required IReadOnlyList<string> LongNames { get; init; }

    /// <summary>
    /// 选项值在帮助文本中的占位符名称。
    /// </summary>
    public string? ValueName { get; init; }

    /// <summary>
    /// 选项的描述。如果选项没有指定 <see cref="CommandLineAttribute.Description"/>，则此属性为 <see langword="null"/>。
    /// </summary>
    public required string? Description { get; init; }

    /// <summary>
    /// 是否为必需选项。
    /// </summary>
    public required bool IsRequired { get; init; }

    /// <summary>
    /// 选项值的类型。
    /// </summary>
    public required OptionValueType ValueType { get; init; }
}

/// <summary>
/// 单个位置参数的帮助信息。
/// </summary>
public readonly record struct ValueHelpInfo
{
    /// <summary>
    /// 位置参数的起始索引。
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// 位置参数的数量；<see langword="null"/> 表示无限制。
    /// </summary>
    public required int? Count { get; init; }

    /// <summary>
    /// 位置参数的名称，从属性名推断。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 位置参数的描述。如果位置参数没有指定 <see cref="CommandLineAttribute.Description"/>，则此属性为 <see langword="null"/>。
    /// </summary>
    public required string? Description { get; init; }

    /// <summary>
    /// 是否为必需位置参数。
    /// </summary>
    public required bool IsRequired { get; init; }
}
