using System.Collections;

namespace DotNetCampus.Cli.Utils;

/// <summary>
/// 从一个只读集合中取出一个范围，让此集合表现得就像那个范围内的一个子集合一样。
/// </summary>
/// <param name="sourceList">原集合。</param>
/// <param name="range">范围。</param>
/// <typeparam name="T">集合的元素类型。</typeparam>
internal readonly struct ReadOnlyListRange<T>(IReadOnlyList<T> sourceList, Range range) : IReadOnlyList<T>
{
    public int Count => range.GetOffsetAndLength(sourceList.Count).Length;

    public T this[int index] => sourceList[range.GetOffsetAndLength(sourceList.Count).Offset + index];

    public IEnumerator<T> GetEnumerator()
    {
        var (offset, length) = range.GetOffsetAndLength(sourceList.Count);
        for (var i = offset; i < offset + length; i++)
        {
            yield return sourceList[i];
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
