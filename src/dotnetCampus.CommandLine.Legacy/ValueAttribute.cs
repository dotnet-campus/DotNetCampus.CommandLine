#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 标记一个属性对应命令行中的一个不带选项的值。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ValueAttribute : CommandLinePropertyAttribute
    {
        /// <summary>
        /// 获取此值在所有命令行不带选项值中的位置。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 当命令行匹配到此属性时，将取出此长度的参数全部设置到此属性中。
        /// <list type="bullet">
        /// <item>默认长度为 1，即取出一个参数作为值。</item>
        /// <item>如果指定成大于 1 的值，建议将属性设置为集合类型。</item>
        /// <item>如果希望所有后续的参数都作为值放入此属性中，请指定为 int.MaxValue；此时如果还存在 <see cref="OptionAttribute"/> 则更建议使用 -- 分隔命令行参数。</item>
        /// </list>
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 将此属性与命令行中指定位置的一个值进行绑定。
        /// </summary>
        /// <param name="index">命令行中值的位置。</param>
        public ValueAttribute(int index)
        {
            Index = index;
            Length = 1;
        }

        /// <summary>
        /// 将此属性与命令行中指定位置的一个值进行绑定。
        /// </summary>
        /// <param name="index">命令行中值的位置。</param>
        /// <param name="length">
        /// <list type="bullet">
        /// <item>默认长度为 1，即取出一个参数作为值。</item>
        /// <item>如果指定成大于 1 的值，建议将属性设置为集合类型。</item>
        /// <item>如果希望所有后续的参数都作为值放入此属性中，请指定为 int.MaxValue；此时如果还存在 <see cref="OptionAttribute"/> 则更建议使用 -- 分隔命令行参数。</item>
        /// </list>
        /// </param>
        public ValueAttribute(int index, int length)
        {
            if (index < 0)
            {
                throw new ArgumentException("命令中值的位置必须大于或等于 0。", nameof(length));
            }

            if (length <= 0)
            {
                throw new ArgumentException("命令中值的长度必须大于或等于 1。", nameof(length));
            }

            Index = index;
            Length = length;
        }
    }
}
