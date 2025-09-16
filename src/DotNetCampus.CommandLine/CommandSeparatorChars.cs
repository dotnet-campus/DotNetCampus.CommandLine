using System.Collections;
using System.Runtime.CompilerServices;

namespace DotNetCampus.Cli;

/// <summary>
/// 允许用户在命令行中使用的分隔符字符集合。<br/>
/// 用节省空间的方式存储不小于长度 4 的多个字符。
/// </summary>
#if NET8_0_OR_GREATER
[CollectionBuilder(typeof(CommandSeparatorChars), nameof(Create))]
#endif
public readonly record struct CommandSeparatorChars : IEnumerable<char>
{
    /// <summary>
    /// 最多支持 4 个分隔符字符。
    /// </summary>
    private readonly uint _chars;

    private CommandSeparatorChars(uint packedChars)
    {
        _chars = packedChars;
    }

    /// <summary>
    /// 以只读列表形式返回分隔符字符集合。
    /// </summary>
    /// <returns>分隔符字符集合。</returns>
    public void CopyTo(Span<char> buffer, out int length)
    {
        length = 0;
        var packed = _chars;
        while (packed != 0)
        {
            var c = (char)(packed & 0xFF);
            if (length < buffer.Length)
            {
                buffer[length] = c;
            }

            length++;
            packed >>= 8;
        }
    }

    /// <summary>
    /// 返回一个枚举器，该枚举器按添加顺序遍历 <see cref="CommandSeparatorChars"/> 中的字符。
    /// </summary>
    /// <returns>一个可用于遍历 <see cref="CommandSeparatorChars"/> 中字符的枚举器。</returns>
    public IEnumerator<char> GetEnumerator()
    {
        var packed = _chars;
        while (packed != 0)
        {
            var c = (char)(packed & 0xFF);
            yield return c;
            packed >>= 8;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// 从长度不大于 4 的字符（ASCII）集合创建一个新的 <see cref="CommandSeparatorChars"/> 实例。
    /// </summary>
    /// <param name="chars">分隔符字符集合。</param>
    /// <returns>新的 <see cref="CommandSeparatorChars"/> 实例。</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果 <paramref name="chars"/> 长度大于 4。</exception>
    /// <exception cref="ArgumentException">如果 <paramref name="chars"/> 中包含 null 字符。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CommandSeparatorChars Create(params ReadOnlySpan<char> chars)
    {
        if (chars.Length > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(chars), "最多只能指定 4 个分隔符字符。");
        }

        uint packed = 0;
        for (var i = chars.Length - 1; i >= 0; i--)
        {
            var c = chars[i];
            if (c == 0)
            {
                throw new ArgumentException("不支持 null 字符作为分隔符。", nameof(chars));
            }

            packed = (packed << 8) | c;
        }

        return new CommandSeparatorChars(packed);
    }
}
