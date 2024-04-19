using System.Collections;
using System.Collections.Generic;

namespace dotnetCampus.Cli.Core
{
    /// <summary>
    /// 提供一个类似于字典，但又为索引和更换 Key 提供更高性能的集合。
    /// 注意，此类型为了性能实现了两个 <see cref="IEnumerable{T}"/>。
    /// 如果能够使用 ValueTuple，那么就不需要实现两个了，因为可以用一个三值的 ValueTuple。
    /// </summary>
    /// <typeparam name="T">集合中值的类型。</typeparam>
    internal class ListGroup<T> : IEnumerable<KeyValuePair<int, string>>, IEnumerable<KeyValuePair<string, T?>>
        where T : class, IReadOnlyList<string?>
    {
        /// <summary>
        /// 存储 Key 的集合。
        /// </summary>
        private readonly List<string> _keys = new List<string>();

        /// <summary>
        /// 存储值的集合。
        /// </summary>
        private readonly List<T?> _values = new List<T?>();

        /// <summary>
        /// 添加一个键值对。
        /// </summary>
        /// <param name="key">Key。</param>
        /// <param name="value">值。</param>
        public void Add(string key, T? value)
        {
            if (key is null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            _keys.Add(key);
            _values.Add(value);
        }

        /// <summary>
        /// 在指定的索引处更换 Key。此方法允许在遍历期间执行，因为遍历时不会改变集合个数。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <param name="newKey">新 Key。</param>
        public void ReplaceKey(int index, string newKey)
        {
            if (newKey is null)
            {
                throw new System.ArgumentNullException(nameof(newKey));
            }

            _keys[index] = newKey;
        }

        /// <summary>
        /// 以带索引和 Key 的方式遍历此集合。
        /// </summary>
        public IEnumerable<KeyValuePair<int, string>> ForIndexes() => this;

        /// <summary>
        /// 以带 Key 和值的方式遍历此集合。
        /// </summary>
        public IEnumerable<KeyValuePair<string, T?>> ForOptions() => this;

        /// <summary>
        /// 以带索引和 Key 的方式遍历此集合。
        /// </summary>
        IEnumerator<KeyValuePair<int, string>> IEnumerable<KeyValuePair<int, string>>.GetEnumerator()
        {
            for (var i = 0; i < _keys.Count; i++)
            {
                yield return new KeyValuePair<int, string>(i, _keys[i]);
            }
        }

        /// <summary>
        /// 以带 Key 和值的方式遍历此集合。
        /// </summary>
        public IEnumerator<KeyValuePair<string, T?>> GetEnumerator()
        {
            for (var i = 0; i < _keys.Count; i++)
            {
                yield return new KeyValuePair<string, T?>(_keys[i], _values[i]);
            }
        }

        /// <summary>
        /// 当使用 foreach 遍历此集合的时候，以带 Key 和值的方式遍历。
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}