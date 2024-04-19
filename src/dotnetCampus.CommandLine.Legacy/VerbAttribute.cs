#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 标记一个命令行参数类型所从属的谓词。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class VerbAttribute : CommandLineAttribute
    {
        /// <summary>
        /// 获取命令行谓词。
        /// </summary>
        public string VerbName { get; }

        /// <summary>
        /// 将此属性与命令行参数的一个长名称进行绑定。
        /// </summary>
        /// <param name="verbName">命令行长名称，使用 PascalCase 风格，区分大小写。</param>
        public VerbAttribute(string verbName)
        {
            if (verbName == null)
            {
                throw new ArgumentNullException(nameof(verbName));
            }

            if (string.IsNullOrWhiteSpace(verbName))
            {
                throw new ArgumentException("命令行谓词不能是空白字符串。", nameof(verbName));
            }

            VerbName = verbName;
        }
    }
}
