using System.Collections.Generic;

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 为命令行类型 <typeparamref name="T"/> 提供解析器。
    /// </summary>
    /// <typeparam name="T">命令行类型。</typeparam>
    public interface ICommandLineOptionParser<out T>
    {
        /// <summary>
        /// 获取此命令行选项的谓词。
        /// 此属性仅在存在多个命令行参数类型时才会生效，所以如果确定只有一个命令行参数类型，可以在这里返回 null。
        /// </summary>
        string? Verb { get; }

        /// <summary>
        /// 将一个无参数的所有值填充到命令行参数类型 <typeparamref name="T"/> 的实例中。
        /// </summary>
        /// <param name="values">值集合，保持传入的大小写。</param>
        void SetValue(IReadOnlyList<string> values);

        /// <summary>
        /// 将短名称的布尔值填充到命令行参数类型 <typeparamref name="T"/> 的实例中。
        /// </summary>
        /// <param name="shortName">命令行短名称，区分大小写。</param>
        /// <param name="value">布尔值。</param>
        void SetValue(char shortName, bool value);

        /// <summary>
        /// 将短名称的值填充到命令行参数类型 <typeparamref name="T"/> 的实例中。
        /// </summary>
        /// <param name="shortName">命令行短名称，区分大小写。</param>
        /// <param name="value">从命令行中传入的值，保持传入的大小写。</param>
        void SetValue(char shortName, string value);

        /// <summary>
        /// 将短名称的值集合填充到命令行参数类型 <typeparamref name="T"/> 的实例中。
        /// </summary>
        /// <param name="shortName">命令行短名称，区分大小写。</param>
        /// <param name="values">从命令行中传入的值集合，保持传入的大小写。</param>
        void SetValue(char shortName, IReadOnlyList<string> values);

        /// <summary>
        /// 将长名称的布尔值填充到命令行参数类型 <typeparamref name="T"/> 的实例中。
        /// </summary>
        /// <param name="longName">
        /// 命令行长名称，与 <see cref="OptionAttribute.LongName"/> 值相等，使用 PascalCase 风格，区分大小写。
        /// </param>
        /// <param name="value">布尔值。</param>
        void SetValue(string longName, bool value);

        /// <summary>
        /// 将长名称的值填充到命令行参数类型 <typeparamref name="T"/> 的实例中。
        /// </summary>
        /// <param name="longName">
        /// 命令行长名称，与 <see cref="OptionAttribute.LongName"/> 值相等，使用 PascalCase 风格，区分大小写。
        /// </param>
        /// <param name="value">值，保持传入的大小写。</param>
        void SetValue(string longName, string value);

        /// <summary>
        /// 将长名称的值集合填充到命令行参数类型 <typeparamref name="T"/> 的实例中。
        /// </summary>
        /// <param name="longName">
        /// 命令行长名称，与 <see cref="OptionAttribute.LongName"/> 值相等，使用 PascalCase 风格，区分大小写。
        /// </param>
        /// <param name="values">值集合，保持传入的大小写。</param>
        void SetValue(string longName, IReadOnlyList<string> values);

        /// <summary>
        /// 提交全部的命令行解析结果。
        /// <para>对于不可变类型 <typeparamref name="T"/>，一般只有这里才能创建 <typeparamref name="T"/> 的新实例，然后返回。</para>
        /// <para>如果是可变类型，可以在此接口实现的构造函数中创建实例，在 SetValue 方法中修改，在此方法中返回。</para>
        /// </summary>
        /// <returns>命令行参数 <typeparamref name="T"/> 类型的新实例。</returns>
        T Commit();
    }
}