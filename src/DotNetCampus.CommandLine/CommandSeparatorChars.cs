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
    /// 一个特殊的字符（不能是 0），用来表示有分隔，但没有符。<br/>
    /// 例如，一般的分隔符是这样：-o:1.txt；<br/>
    /// 但有部分风格的分隔符是这样：-o1.txt。<br/>
    /// 这时，我们需要一个特殊的字符来表示这种情况。
    /// </summary>
    private const char Null = '\x1E';

    /// <summary>
    /// 最多支持 4 个分隔符字符。
    /// </summary>
    private readonly ushort _chars;

    private CommandSeparatorChars(ushort packedChars)
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
            var c = (char)(packed & 0xF);
            if (c == Null)
            {
                c = '\0';
            }

            if (length < buffer.Length)
            {
                buffer[length] = c;
            }

            length++;
            packed >>= 4;
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
            var c = (char)(packed & 0xF);
            if (c == Null)
            {
                c = '\0';
            }

            yield return c;
            packed >>= 4;
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

        ushort packed = 0;
        for (var i = chars.Length - 1; i >= 0; i--)
        {
            var c = chars[i];
            if (c == 0)
            {
                c = Null;
            }

            packed = (ushort)((packed << 4) | c);
        }

        return new CommandSeparatorChars(packed);
    }
}
