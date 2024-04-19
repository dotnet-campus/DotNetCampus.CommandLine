using System;

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 标记此类型会处理甚至拦截任何种类的命令行输入。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class FilterAttribute : CommandLineAttribute
    {
        /// <summary>
        /// 标记此类型会处理甚至拦截任何种类的命令行输入。
        /// </summary>
        public FilterAttribute()
        {
        }
    }
}
