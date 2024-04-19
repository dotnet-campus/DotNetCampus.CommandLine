namespace dotnetCampus.Cli
{
    /// <summary>
    /// 为命令行参数与类型属性的关联提供特性基类。
    /// </summary>
    public abstract class CommandLinePropertyAttribute : CommandLineAttribute
    {
        /// <summary>
        /// 只允许内部的类型继承自 <see cref="CommandLinePropertyAttribute"/>。
        /// </summary>
        internal CommandLinePropertyAttribute()
        {
        }

        /// <summary>
        /// 如果此命令行属性的值是限定类型的，那么使用 <see cref="TypeName"/> 来指定类型的名称；这将会在输出使用说明文档的时候给予用户适当的提示。
        /// 默认情况下，枚举类型/路径类型会自动生成名称而无需另外指定。
        /// </summary>
        /// <remarks>
        /// 此属性待稳定后决定是否公开。
        /// </remarks>
        internal string? TypeName { get; set; }
    }
}
