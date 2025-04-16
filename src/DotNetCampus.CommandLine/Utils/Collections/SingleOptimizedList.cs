using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace DotNetCampus.Cli.Utils.Collections;

/// <summary>
/// 为 0 个和 1 个值特殊优化的列表。
/// </summary>
[DebuggerDisplay(nameof(SingleOptimizedList<T>) + " {_firstValue,nq}, {_restValues}")]
internal readonly struct SingleOptimizedList<T> : IReadOnlyList<T>
{
    /// <summary>
    /// 是否有值。如果为 <see langword="false"/>，则是空列表。
    /// </summary>
    [MemberNotNullWhen(true, nameof(_firstValue))]
    private bool HasValue { get; }

    /// <summary>
    /// 在此命令行解析的上下文中，通常也不会为空字符串或空白字符串。
    /// </summary>
    private readonly T? _firstValue;

    /// <summary>
    /// 当所需储存的值超过 1 个时，将启用此列表。所以此列表要么为 null，要么有多于 1 个的值。
    /// </summary>
    private readonly List<T>? _restValues;

    public SingleOptimizedList()
    {
    }

    public SingleOptimizedList(T value)
    {
        HasValue = true;
        _firstValue = value;
    }

    private SingleOptimizedList(T firstValue, List<T> restValues)
    {
        HasValue = true;
        _firstValue = firstValue;
        _restValues = restValues;
    }

    /// <summary>
    /// 获取集合中值的个数。
    /// </summary>
    public int Count => HasValue switch
    {
        false => 0,
        true when _restValues is null => 1,
        true => _restValues.Count + 1,
    };

    /// <summary>
    /// 获取集合中指定索引处的值。
    /// </summary>
    public T this[int index] => HasValue
        ? index is 0 ? _firstValue! : _restValues![index - 1]
        : throw new ArgumentOutOfRangeException(nameof(index), "集合中没有值。");

    /// <summary>
    /// 添加一个值到集合中，并返回包含该值的新集合。
    /// </summary>
    /// <param name="value">要添加的值。</param>
    [Pure]
    public SingleOptimizedList<T> Add(T value)
    {
        if (!HasValue)
        {
            // 空集合，添加第一个值。
            return new SingleOptimizedList<T>(value);
        }

        if (_restValues is null)
        {
            // 只有一个值，添加第二个值。
            return new SingleOptimizedList<T>(_firstValue, [value]);
        }

        // 已经有多个值，添加到现有的列表中。
        // 注意！此行为与其他任何集合都不同，会导致新旧对象共享同一个列表的引用，同时被修改！所以日常不要使用此集合。
        _restValues.Add(value);
        return new SingleOptimizedList<T>(_firstValue, _restValues);
    }

    public SingleOptimizedList<T> AddRange(IReadOnlyList<T> values)
    {
        if (values.Count is 0)
        {
            return this;
        }

        if (values.Count is 1)
        {
            return Add(values[0]);
        }

        if (!HasValue)
        {
            // 空集合，添加第一个值。
            return new SingleOptimizedList<T>(values[0], values.Skip(1).ToList());
        }

        if (_restValues is null)
        {
            // 只有一个值，添加第二个值。
            return new SingleOptimizedList<T>(_firstValue, values.ToList());
        }

        // 已经有多个值，添加到现有的列表中。
        // 注意！此行为与其他任何集合都不同，会导致新旧对象共享同一个列表的引用，同时被修改！所以日常不要使用此集合。
        _restValues.AddRange(values);
        return new SingleOptimizedList<T>(_firstValue, _restValues);
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (!HasValue)
        {
            yield break;
        }

        yield return _firstValue;

        if (_restValues is not { } restValues)
        {
            yield break;
        }

        for (var i = 0; i < restValues.Count; i++)
        {
            var value = restValues[i];
            yield return value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class SingleOptimizedListExtensions
{
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, SingleOptimizedList<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var list))
        {
            // 已经有值了，添加到列表中。
            dictionary[key] = list.Add(value);
            return false;
        }

        // 没有值，添加一个新的值。
        dictionary[key] = new SingleOptimizedList<TValue>(value);
        return true;
    }

    public static void AddOrUpdateSingle<TKey, TValue>(this Dictionary<TKey, SingleOptimizedList<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        dictionary[key] = new SingleOptimizedList<TValue>(value);
    }
}
