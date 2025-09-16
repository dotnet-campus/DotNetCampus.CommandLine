using System.Diagnostics.Contracts;

namespace DotNetCampus.Cli.Utils;

/// <summary>
/// 用节省空间的方式存储多个布尔值。
/// </summary>
internal struct BooleanValues16()
{
    private ushort _value;

    internal BooleanValues16(ushort packedValue) : this()
    {
        _value = packedValue;
    }

    /// <summary>
    /// 获取或设置指定索引处的布尔值。
    /// </summary>
    /// <param name="index">索引，范围 0-15。</param>
    internal bool this[int index]
    {
        get => (_value & (1 << index)) != 0;
        set
        {
            if (value)
            {
                _value |= (ushort)(1 << index);
            }
            else
            {
                _value &= (ushort)~(1 << index);
            }
        }
    }

    /// <summary>
    /// 获取或设置指定索引处的两个布尔值。
    /// </summary>
    /// <param name="index">索引，范围 0-14。</param>
    /// <param name="index1">必须等于 <paramref name="index"/> + 1。</param>
    internal (bool Item1, bool Item2) this[int index, int index1]
    {
        get
        {
            var bits = (_value & (3 << index)) >> index;
            return ((bits & 1) != 0, (bits & 2) != 0);
        }
        set
        {
            var bits = (value.Item1 ? 1 : 0) | (value.Item2 ? 2 : 0);
            _value = (ushort)((_value & ~(3 << index)) | (bits << index));
        }
    }

    /// <summary>
    /// 获取或设置指定索引处的三个布尔值。
    /// </summary>
    /// <param name="index">索引，范围 0-13。</param>
    /// <param name="index1">必须等于 <paramref name="index"/> + 1。</param>
    /// <param name="index2">必须等于 <paramref name="index"/> + 2。</param>
    internal (bool Item1, bool Item2, bool Item3) this[int index, int index1, int index2]
    {
        get
        {
            var bits = (_value & (7 << index)) >> index;
            return ((bits & 1) != 0, (bits & 2) != 0, (bits & 4) != 0);
        }
        set
        {
            var bits = (value.Item1 ? 1 : 0) | (value.Item2 ? 2 : 0) | (value.Item3 ? 4 : 0);
            _value = (ushort)((_value & ~(7 << index)) | (bits << index));
        }
    }

    /// <summary>
    /// 获取用于存储布尔值的魔术数字。
    /// </summary>
    /// <returns>魔术数字。</returns>
    [Pure]
    internal ushort GetMagicNumber() => _value;
}
