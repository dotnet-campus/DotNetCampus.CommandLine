namespace DotNetCampus.Cli.Utils.Parsers;

internal readonly ref struct OptionName(bool isLongOption, ReadOnlySpan<char> optionName)
{
    /// <summary>
    /// <see langword="true"/> 表示长选项，<see langword="false"/> 表示短选项。
    /// </summary>
    internal bool IsLongOption { get; } = isLongOption;

    /// <summary>
    /// 选项名称，不包含前缀符号。
    /// </summary>
    internal ReadOnlySpan<char> Name { get; } = optionName;

    public override string ToString()
    {
        return Name.ToString();
    }
}
