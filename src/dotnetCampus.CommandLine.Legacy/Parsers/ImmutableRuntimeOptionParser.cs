#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using dotnetCampus.Cli.Core;
using dotnetCampus.Cli.Utils;

namespace dotnetCampus.Cli.Parsers
{
    internal class ImmutableRuntimeOptionParser<T> : RuntimeCommandLineOptionParser<T>, IRawCommandLineOptionParser<T>
    {
        private readonly ConstructorInfo _constructor;
        private readonly Type[] _propertyTypes;
        private readonly object[] _values;
        private readonly Dictionary<PropertyInfo, int> _indexDictionary = new Dictionary<PropertyInfo, int>();
        private readonly Dictionary<int, ValueTupleSlim<int, PropertyInfo>> _indexedValueDictionary = new Dictionary<int, ValueTupleSlim<int, PropertyInfo>>();
        private readonly Dictionary<char, PropertyInfo> _shortNameDictionary = new Dictionary<char, PropertyInfo>();
        private readonly Dictionary<string, PropertyInfo> _longNameDictionary = new Dictionary<string, PropertyInfo>();

        public ImmutableRuntimeOptionParser(string? verb, IReadOnlyList<PropertyInfo> attributedProperties) : base(verb)
        {
            var properties = attributedProperties;

            _values = new object[properties.Count];
            _propertyTypes = properties.Select(x => x.PropertyType).ToArray();
            var constructor = typeof(T).GetConstructor(_propertyTypes);
            _constructor = constructor ?? throw new InvalidOperationException("Option 中必须为每个可能的命令行参数添加构造函数参数。");
            for (var i = 0; i < properties.Count; i++)
            {
                var propertyInfo = properties[i];
                _indexDictionary[propertyInfo] = i;

                if (propertyInfo.IsDefined(typeof(OptionAttribute)))
                {
                    var attribute = propertyInfo.GetCustomAttribute<OptionAttribute>();
                    if (attribute!.ShortName != null)
                    {
                        _shortNameDictionary[attribute.ShortName.Value] = propertyInfo;
                    }

                    if (attribute.LongName is null || string.IsNullOrWhiteSpace(attribute.LongName))
                    {
                        _longNameDictionary[propertyInfo.Name] = propertyInfo;
                    }
                    else
                    {
                        _longNameDictionary[attribute.LongName] = propertyInfo;
                    }
                }

                if (propertyInfo.IsDefined(typeof(ValueAttribute)))
                {
                    var attribute = propertyInfo.GetCustomAttribute<ValueAttribute>();
                    _indexedValueDictionary[attribute!.Index] = new ValueTupleSlim<int, PropertyInfo>(attribute.Length, propertyInfo);
                }
            }
        }

        public override void SetValue(IReadOnlyList<string> values)
        {
            var indexOffset = 0;
            foreach (var pair in _indexedValueDictionary)
            {
                var index = pair.Key + indexOffset;
                var (length, property) = pair.Value;
                indexOffset += length;

                var (_, propertyAsKey) = _indexedValueDictionary[index];
                var storeIndex = _indexDictionary[propertyAsKey];
                if (length == 1)
                {
                    SetValueCore(storeIndex, values[index]);
                }
                else
                {
                    SetValueCore(storeIndex, values.Skip(index).Take(length).ToList());
                }
            }
        }

        public override void SetValue(char shortName, bool value)
        {
            _values[_indexDictionary[_shortNameDictionary[shortName]]] = value;
        }

        public override void SetValue(char shortName, string value)
        {
            SetValueCore(_indexDictionary[_shortNameDictionary[shortName]], value);
        }

        public override void SetValue(char shortName, IReadOnlyList<string> values)
        {
            SetValueCore(_indexDictionary[_shortNameDictionary[shortName]], values);
        }

        public override void SetValue(string longName, bool value)
        {
            _values[_indexDictionary[_longNameDictionary[longName]]] = value;
        }

        public override void SetValue(string longName, string value)
        {
            SetValueCore(_indexDictionary[_longNameDictionary[longName]], value);
        }

        public override void SetValue(string longName, IReadOnlyList<string> values)
        {
            SetValueCore(_indexDictionary[_longNameDictionary[longName]], values);
        }

        public override void SetValue(char shortName, SingleOptimizedStrings? values)
        {
            SetValueCore(_indexDictionary[_shortNameDictionary[shortName]], values);
        }

        public override void SetValue(string longName, SingleOptimizedStrings? values)
        {
            SetValueCore(_indexDictionary[_longNameDictionary[longName]], values);
        }

        private void SetValueCore(int index, string value)
            => SetValueCore(index, string.IsNullOrEmpty(value) ? null : new[] { value });

        private void SetValueCore(int index, IReadOnlyList<string>? values)
        {
            var type = _propertyTypes[index];
            _values[index] = values.ToAssignableValue(type);
        }

        public override T Commit()
        {
            return (T)_constructor.Invoke(_values);
        }
    }
}
