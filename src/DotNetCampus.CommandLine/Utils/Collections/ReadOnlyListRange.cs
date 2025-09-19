using System.Collections;

namespace DotNetCampus.Cli.Temp40.Utils.Collections;

/// <summary>
/// 从一个只读集合中取出一个范围，让此集合表现得就像那个范围内的一个子集合一样。
/// </summary>
/// <typeparam name="T">集合的元素类型。</typeparam>
internal readonly struct ReadOnlyListRange<T> : IReadOnlyList<T>
{
    private readonly IReadOnlyList<T>? _sourceList;
    private readonly Range _range;

    /// <summary>
    /// 从一个只读集合中取出一个范围，让此集合表现得就像那个范围内的一个子集合一样。
    /// </summary>
    /// <param name="sourceList">原集合。</param>
    /// <param name="range">范围。</param>
    public ReadOnlyListRange(IReadOnlyList<T> sourceList, Range range)
    {
        _sourceList = sourceList;
        _range = range;
    }

    public int Count => _range.GetOffsetAndLength(_sourceList?.Count ?? 0).Length;

    public T this[int index] => _sourceList is null
        ? throw new ArgumentOutOfRangeException(nameof(index))
        : _sourceList[_range.GetOffsetAndLength(_sourceList.Count).Offset + index];

    /// <summary>
    /// 获取第一个元素，如果没有元素则返回默认值。
    /// </summary>
    public T? FirstOrDefault => Count is 0 ? default : this[0];

    public ReadOnlyListRange<T> Slice(int offset, int length)
    {
        if (_sourceList is null)
        {
            return offset is 0 && length is 0
                ? new ReadOnlyListRange<T>([], new Range(0, 0))
                : throw new ArgumentOutOfRangeException(nameof(length));
        }

        var (start, _) = _range.GetOffsetAndLength(_sourceList.Count);
        return new ReadOnlyListRange<T>(_sourceList, new Range(start + offset, start + offset + length));
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (_sourceList is null)
        {
            yield break;
        }

        var (offset, length) = _range.GetOffsetAndLength(_sourceList.Count);
        for (var i = offset; i < offset + length; i++)
        {
            yield return _sourceList[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class ReadOnlyListRangeExtensions
{
    public static ReadOnlyListRange<T> Slice<T>(this IReadOnlyList<T> sourceList, Range range)
    {
        return new ReadOnlyListRange<T>(sourceList, range);
    }

    public static ReadOnlyListRange<T> Slice<T>(this IReadOnlyList<T> sourceList, int offset, int length)
    {
        return new ReadOnlyListRange<T>(sourceList, new Range(offset, offset + length));
    }

    public static ReadOnlyListRange<T> ToReadOnlyList<T>(this IReadOnlyList<T> sourceList)
    {
        return new ReadOnlyListRange<T>(sourceList, new Range(0, sourceList.Count));
    }
}
