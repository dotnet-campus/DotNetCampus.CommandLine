using System;
using System.Diagnostics.CodeAnalysis;
using System.Resources;

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 为命令行参数与类型属性的关联提供特性基类。
    /// </summary>
    public abstract class CommandLineAttribute : Attribute
    {
        /// <summary>
        /// 只允许内部的类型继承自 <see cref="CommandLineAttribute"/>。
        /// </summary>
        internal CommandLineAttribute()
        {
        }

        /// <summary>
        /// 此命令行类/属性的描述信息（会在命令行输出帮助信息时使用）。
        /// <para>如果希望添加本地化支持，请使用 <see cref="LocalizableDescription"/> 属性。</para>
        /// </summary>
        [DisallowNull]
        public string? Description { get; set; }

        /// <summary>
        /// 此命令行类/属性的本地化描述信息（会在命令行输出帮助信息时使用）。
        /// <para></para>
        /// <para>设置方法：</para>
        /// <para><code>LocalizableDescription = nameof(Resources.XxxDescription)</code></para>
        /// <para></para>
        /// <para>要使本地化生效，你还需要在 <see cref="CommandLine.Parse(string[], ResourceManager)"/> 中传入资源参数，方法是：</para>
        /// <para><code>CommandLine.Parse(args, Resources.ResourceManager)</code></para>
        /// <para>其中，你可以通过创建 Resources.resx 文件来获得以上的 Resources 类型。如果创建的是 LocalizableStrings.resx 文件，则可得到 LocalizableStrings 类型，对应的，前面例子中传入的参数也应换成 LocalizableStrings 类型。</para>
        /// <para>为了获得本地化支持，你可额外创建对应的 Resources.zh-CN.resx / LocalizableStrings.zh-CN.resx 等文件并填入对应语言的字符串；随后 <see cref="CommandLine"/> 将自动使用你填入的本地化字符串。</para>
        /// </summary>
        [DisallowNull]
        public string? LocalizableDescription { get; set; }
    }
}
