using System;
using System.Collections.Generic;
using IndexString = System.Collections.Generic.Dictionary<int, System.Action<string>>;
using CharBool = System.Collections.Generic.Dictionary<char, System.Action<bool>>;
using CharString = System.Collections.Generic.Dictionary<char, System.Action<string>>;
using CharStrings =
    System.Collections.Generic.Dictionary<char, System.Action<System.Collections.Generic.IReadOnlyList<string>>>;
using StringBool = System.Collections.Generic.Dictionary<string, System.Action<bool>>;
using StringString = System.Collections.Generic.Dictionary<string, System.Action<string>>;
using StringStrings =
    System.Collections.Generic.Dictionary<string, System.Action<System.Collections.Generic.IReadOnlyList<string>>>;
using dotnetCampus.Cli.Core;

#pragma warning disable CA1033 // Interface methods should be callable by child types

namespace dotnetCampus.Cli
{
    /// <summary>
    /// 提供 <see cref="ICommandLineOptionParser{T}"/> 的一个抽象实现，用于牺牲解析性能获得更好的解析器编写体验。
    /// 如果你需要手写一个命令行解析器的实现，那么继承自此类型可以大幅减少代码量。
    /// </summary>
    /// <typeparam name="T">命令行类型。</typeparam>
    public abstract class CommandLineOptionParser<T> : ICommandLineOptionParser<T>, IRawCommandLineOptionParser<T>
    {
        private string? _verb;
        private Func<T>? _commit;
        private readonly IndexString _indexString = new IndexString();
        private readonly CharBool _shortNameBool = new CharBool();
        private readonly CharString _shortNameString = new CharString();
        private readonly CharStrings _shortNameStrings = new CharStrings();
        private readonly StringBool _longNameBool = new StringBool();
        private readonly StringString _longNameString = new StringString();
        private readonly StringStrings _longNameStrings = new StringStrings();

        /// <summary>
        /// 添加一个以值序号定位的命令行参数。
        /// </summary>
        /// <param name="index">值序号，即此属性经常出现在命令行的第几个参数中。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(int index, Action<string> action)
            => _indexString[index] = action ?? throw new ArgumentNullException(nameof(action));

        /// <summary>
        /// 添加一个以短名称定位的命令行参数。
        /// </summary>
        /// <param name="shortName">短名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(char shortName, Action<bool> action)
            => _shortNameBool[shortName] = action ?? throw new ArgumentNullException(nameof(action));

        /// <summary>
        /// 添加一个以短名称定位的命令行参数。
        /// </summary>
        /// <param name="shortName">短名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(char shortName, Action<string> action)
            => _shortNameString[shortName] = action ?? throw new ArgumentNullException(nameof(action));

        /// <summary>
        /// 添加一个以短名称定位的命令行参数。
        /// </summary>
        /// <param name="shortName">短名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(char shortName, Action<IReadOnlyList<string>> action)
            => _shortNameStrings[shortName] = action ?? throw new ArgumentNullException(nameof(action));

        /// <summary>
        /// 添加一个以长名称定位的命令行参数。
        /// </summary>
        /// <param name="longName">长名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(string longName, Action<bool> action)
            => _longNameBool[longName] = action ?? throw new ArgumentNullException(nameof(action));

        /// <summary>
        /// 添加一个以长名称定位的命令行参数。
        /// </summary>
        /// <param name="longName">长名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(string longName, Action<string> action)
            => _longNameString[longName] = action ?? throw new ArgumentNullException(nameof(action));

        /// <summary>
        /// 添加一个以长名称定位的命令行参数。
        /// </summary>
        /// <param name="longName">长名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(string longName, Action<IReadOnlyList<string>> action)
            => _longNameStrings[longName] = action ?? throw new ArgumentNullException(nameof(action));

        /// <summary>
        /// 添加一个用短名称和长名称定位的命令行参数。
        /// </summary>
        /// <param name="shortName">短名称。</param>
        /// <param name="longName">长名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(char shortName, string longName, Action<bool> action)
        {
            _shortNameBool[shortName] = action ?? throw new ArgumentNullException(nameof(action));
            _longNameBool[longName] = action;
        }

        /// <summary>
        /// 添加一个用短名称和长名称定位的命令行参数。
        /// </summary>
        /// <param name="shortName">短名称。</param>
        /// <param name="longName">长名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(char shortName, string longName, Action<string> action)
        {
            _shortNameString[shortName] = action ?? throw new ArgumentNullException(nameof(action));
            _longNameString[longName] = action;
        }

        /// <summary>
        /// 添加一个用短名称和长名称定位的命令行参数。
        /// </summary>
        /// <param name="shortName">短名称。</param>
        /// <param name="longName">长名称。</param>
        /// <param name="action">请使用此委托中的参数用以给你对应的参数赋值。</param>
        protected void AddMatch(char shortName, string longName, Action<IReadOnlyList<string>> action)
        {
            _shortNameStrings[shortName] = action ?? throw new ArgumentNullException(nameof(action));
            _longNameStrings[longName] = action;
        }

        /// <summary>
        /// 提供一个创建命令行参数实例的方法，用于在解析完毕后创建命令行参数的一个新实例。
        /// 通常，对于不可变类型，此方法传入的委托将是创建实例的唯一时机；对于可变类型，建议在解析器构造函数一开始就创建实例，仅在此参数中返回那个创建好的实例。
        /// </summary>
        /// <param name="commit">用于创建命令行参数实例的方法。</param>
        protected void SetResult(Func<T> commit)
            => _commit = commit ?? throw new ArgumentNullException(nameof(commit));

        /// <summary>
        /// 获取或设置此命令行参数类型的谓词。
        /// </summary>
        protected string? Verb
        {
            get => _verb;
            set => _verb = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        string? ICommandLineOptionParser<T>.Verb => Verb;

        void ICommandLineOptionParser<T>.SetValue(IReadOnlyList<string> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (_indexString.TryGetValue(i, out var action))
                {
                    action(values[i]);
                }
            }
        }

        void ICommandLineOptionParser<T>.SetValue(char shortName, bool value)
        {
            if (_shortNameBool.TryGetValue(shortName, out var action))
            {
                action(value);
            }
        }

        void ICommandLineOptionParser<T>.SetValue(char shortName, string value)
        {
            if (_shortNameString.TryGetValue(shortName, out var action))
            {
                action(value);
            }
        }

        void ICommandLineOptionParser<T>.SetValue(char shortName, IReadOnlyList<string> values)
        {
            if (_shortNameStrings.TryGetValue(shortName, out var action))
            {
                action(values);
            }
        }

        void ICommandLineOptionParser<T>.SetValue(string longName, bool value)
        {
            if (_longNameBool.TryGetValue(longName, out var action))
            {
                action(value);
            }
        }

        void ICommandLineOptionParser<T>.SetValue(string longName, string value)
        {
            if (_longNameString.TryGetValue(longName, out var action))
            {
                action(value);
            }
        }

        void ICommandLineOptionParser<T>.SetValue(string longName, IReadOnlyList<string> values)
        {
            if (_longNameStrings.TryGetValue(longName, out var action))
            {
                action(values);
            }
        }

        void IRawCommandLineOptionParser<T>.SetValue(char shortName, SingleOptimizedStrings? values)
        {
            if (_shortNameBool.TryGetValue(shortName, out var actionB))
            {
                actionB(values is null ? true : (bool.TryParse(values[0], out var result) && result));
            }
            else if (_shortNameString.TryGetValue(shortName, out var actionS) && values != null)
            {
                actionS(values[0]);
            }
            else if (_shortNameStrings.TryGetValue(shortName, out var actionA) && values != null)
            {
                actionA(values);
            }
        }

        void IRawCommandLineOptionParser<T>.SetValue(string longName, SingleOptimizedStrings? values)
        {
            if (_longNameBool.TryGetValue(longName, out var actionB))
            {
                actionB(values is null ? true : (bool.TryParse(values[0], out var result) && result));
            }
            else if (_longNameString.TryGetValue(longName, out var actionS) && values != null)
            {
                actionS(values.Count == 1 ? values[0] : string.Join(" ", values));
            }
            else if (_longNameStrings.TryGetValue(longName, out var actionA) && values != null)
            {
                actionA(values);
            }
        }

        T ICommandLineOptionParser<T>.Commit()
        {
            if (_commit == null)
            {
                throw new InvalidOperationException(
                    $"在命令行参数解析完毕后依然没有无法提交解析的参数实例，是否忘记在 {GetType().FullName} 中调用 {nameof(SetResult)} 方法指定如何提交参数解析的实例？");
            }

            return _commit();
        }
    }
}
