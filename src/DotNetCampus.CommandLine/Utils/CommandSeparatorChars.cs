using System.Collections;
using System.Runtime.CompilerServices;

namespace DotNetCampus.Cli.Utils;

/// <summary>
/// 允许用户在命令行中使用的分隔符字符集合。最多只能支持 <see cref="CommandSeparatorChars"/> 个字符。
/// </summary>
#if NET8_0_OR_GREATER
[CollectionBuilder(typeof(CommandSeparatorChars), nameof(Create))]
#endif
public readonly record struct CommandSeparatorChars : IEnumerable<char>
{
    /// <summary>
    /// 获取一个空的分隔符字符集合实例。
    /// </summary>
    public static CommandSeparatorChars Empty => new CommandSeparatorChars('\0', '\0');

    /// <summary>
    /// 分隔符字符集合中允许的最大字符数量。
    /// </summary>
    internal const int MaxSupportedCount = 2;

    private readonly char _char0;

    private readonly char _char1;

    private CommandSeparatorChars(char char0, char char1)
    {
        _char0 = char0;
        _char1 = char1;
    }

    /// <summary>
    /// 返回指定文本中第一个分隔符字符的索引；如果未找到任何分隔符字符，则返回 -1。
    /// </summary>
    /// <param name="text">要搜索的文本。</param>
    /// <returns>第一个分隔符字符的索引；如果未找到任何分隔符字符，则返回 -1。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SeparateIndex(ReadOnlySpan<char> text)
    {
        foreach (var c in text)
        {
            if (c == _char0 || c == _char1)
            {
                return text.IndexOf(c);
            }
        }
        return -1;
    }

    /// <summary>
    /// 以只读列表形式返回分隔符字符集合。
    /// </summary>
    /// <returns>分隔符字符集合。</returns>
    public void CopyTo(Span<char> buffer, out int length)
    {
        if (_char0 is '\0')
        {
            length = 0;
            return;
        }
        if (_char1 is '\0')
        {
            buffer[0] = _char0;
            length = 1;
            return;
        }
        buffer[0] = _char0;
        buffer[1] = _char1;
        length = 2;
    }

    /// <summary>
    /// 返回一个枚举器，该枚举器按添加顺序遍历 <see cref="CommandSeparatorChars"/> 中的字符。
    /// </summary>
    /// <returns>一个可用于遍历 <see cref="CommandSeparatorChars"/> 中字符的枚举器。</returns>
    public IEnumerator<char> GetEnumerator()
    {
        if (_char0 is not '\0')
        {
            yield return _char0;
        }
        if (_char1 is not '\0')
        {
            yield return _char1;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// 从长度不大于 <see cref="CommandSeparatorChars"/> 的字符（ASCII）集合创建一个新的 <see cref="CommandSeparatorChars"/> 实例。
    /// </summary>
    /// <param name="chars">分隔符字符集合。</param>
    /// <returns>新的 <see cref="CommandSeparatorChars"/> 实例。</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果 <paramref name="chars"/> 长度大于 <see cref="CommandSeparatorChars"/>。</exception>
    /// <exception cref="ArgumentException">如果 <paramref name="chars"/> 中包含 null 字符。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandSeparatorChars Create(params ReadOnlySpan<char> chars) => chars.Length switch
    {
        0 => new CommandSeparatorChars('\0', '\0'),
        1 => new CommandSeparatorChars(chars[0], '\0'),
        2 => new CommandSeparatorChars(chars[0], chars[1]),
        _ => throw new ArgumentOutOfRangeException(nameof(chars), $"The length of chars cannot be greater than {MaxSupportedCount}."),
    };
}

/// <summary>
/// <see cref="CommandSeparatorChars"/> 的扩展方法。
/// </summary>
public static class CommandSeparatorCharsExtensions
{
    /// <summary>
    /// 返回指定文本中第一个分隔符字符的索引；如果未找到任何分隔符字符，则返回 -1。
    /// </summary>
    /// <param name="span">要搜索的文本。</param>
    /// <param name="separatorChars">分隔符字符集合。</param>
    /// <returns>第一个分隔符字符的索引；如果未找到任何分隔符字符，则返回 -1。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAny(this ReadOnlySpan<char> span, CommandSeparatorChars separatorChars)
    {
        return separatorChars.SeparateIndex(span);
    }
}
