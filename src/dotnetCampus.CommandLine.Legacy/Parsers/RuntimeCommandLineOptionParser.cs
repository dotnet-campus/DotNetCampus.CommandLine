#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using dotnetCampus.Cli.Core;

namespace dotnetCampus.Cli.Parsers
{
    internal abstract class RuntimeCommandLineOptionParser<T> : ICommandLineOptionParser<T>, IRawCommandLineOptionParser<T>
    {
        protected RuntimeCommandLineOptionParser(string? verb)
        {
            Verb = verb;
        }

        public string? Verb { get; }
        public abstract void SetValue(IReadOnlyList<string> values);
        public abstract void SetValue(char shortName, bool value);
        public abstract void SetValue(char shortName, string value);
        public abstract void SetValue(char shortName, IReadOnlyList<string> values);
        public abstract void SetValue(string longName, bool value);
        public abstract void SetValue(string longName, string value);
        public abstract void SetValue(string longName, IReadOnlyList<string> values);
        public abstract void SetValue(char shortName, SingleOptimizedStrings? values);
        public abstract void SetValue(string longName, SingleOptimizedStrings? values);
        public abstract T Commit();

        internal static RuntimeCommandLineOptionParser<T> Create()
        {
            var verb = typeof(T).IsDefined(typeof(VerbAttribute))
                ? typeof(T).GetCustomAttribute<VerbAttribute>()!.VerbName
                : null;

            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.IsDefined(typeof(OptionAttribute)) || x.IsDefined(typeof(ValueAttribute)))
                .ToList();
            if (properties.Count == 0)
            {
                throw new NotSupportedException($"类型 {typeof(T).FullName} 中没有发现任何可以作为命令行参数的可见属性。");
            }

            // 目前仅支持所有属性可写，或者所有属性不可写。不处理中间态。
            // 所以暂时的判定：
            // 1. 所有的属性都有 public set，视为可变类型，支持；
            // 2. 所有属性都只有 public get 方法，视为不可变类型，支持；
            // 3. 其他任何情况都不支持，包括属性中有 private set，或者部分有 public set 部分没有 set 等。
            var isImmutable = properties.All(x => !x.CanWrite);
            var allCanWrite = properties.All(x => x.CanWrite && x.SetMethod!.IsPublic);
            if (allCanWrite || isImmutable)
            {
                return isImmutable
                    ? new ImmutableRuntimeOptionParser<T>(verb, properties)
                    : (RuntimeCommandLineOptionParser<T>)new RuntimeOptionParser<T>(verb, properties);
            }

            throw new NotSupportedException(@"运行时命令行解析器仅支持两种类型的命令行参数类型：
1. 全部属性都只有 public get 的（public string Property { get; }）；
2. 全部属性都可以 public get set 的（public string Property { get; set; }）");
        }
    }
}