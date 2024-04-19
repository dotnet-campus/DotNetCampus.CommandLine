using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace dotnetCampus.Cli.Standard
{
    partial class GnuFilter
    {
        /// <summary>
        /// 为了避免引用此 NuGet 包带来大量的本地化附属库的引用，我们不能使用会生成额外程序集的本地化方案。
        /// 因此，采用了简易的代码硬编码方案。
        /// </summary>
        private class LocalizableStrings
        {
            private Dictionary<string, string> _defaultDictionary = new Dictionary<string, string>
            {
                { "", "" },
            };

            internal string VersionOptionDescription => GetString(Thread.CurrentThread.CurrentUICulture);
            internal Dictionary<string, string> _versionOptionDescription = new Dictionary<string, string>
            {
                {  "", "Display version." },
                { "zh-CN", "显示版本。" },
            };

            internal string HelpOptionDescription => GetString(Thread.CurrentThread.CurrentUICulture);
            internal Dictionary<string, string> _helpOptionDescription = new Dictionary<string, string>
            {
                {  "", "Display help." },
                { "zh-CN", "显示使用说明。" },
            };

            internal string UsageHeader => GetString(Thread.CurrentThread.CurrentUICulture);
            internal Dictionary<string, string> _usageHeader = new Dictionary<string, string>
            {
                {  "", "Usage: " },
                { "zh-CN", "用法：" },
            };

            internal string OptionsHeader => GetString(Thread.CurrentThread.CurrentUICulture);
            internal Dictionary<string, string> _optionsHeader = new Dictionary<string, string>
            {
                {  "", "Options: " },
                { "zh-CN", "选项：" },
            };

            internal string CommandHeader => GetString(Thread.CurrentThread.CurrentUICulture);
            internal Dictionary<string, string> _commandHeader = new Dictionary<string, string>
            {
                {  "", "Commands: " },
                { "zh-CN", "命令：" },
            };

            internal string UnknownCommandFormat => GetString(Thread.CurrentThread.CurrentUICulture);
            internal Dictionary<string, string> _unknownCommandFormat = new Dictionary<string, string>
            {
                {  "", "{0}: '{2}' is not a {0} command. See '{1} --help'." },
                { "zh-CN", "{0}: 无法执行“{2}”命令，请参阅“{1} --help”。" },
            };

            private string GetString(CultureInfo culture, [CallerMemberName] string? propertyName = null)
                => GetString(propertyName!, culture) ?? "";

            internal string? GetString(string name, CultureInfo currentUICulture)
            {
                var dictionary = name switch
                {
                    nameof(VersionOptionDescription) => _versionOptionDescription,
                    nameof(HelpOptionDescription) => _helpOptionDescription,
                    nameof(UsageHeader) => _usageHeader,
                    nameof(OptionsHeader) => _optionsHeader,
                    nameof(CommandHeader) => _commandHeader,
                    nameof(UnknownCommandFormat) => _unknownCommandFormat,
                    _ => _defaultDictionary,
                };
                return dictionary.TryGetValue(currentUICulture.Name, out var text)
                    ? text
                    : dictionary[""];
            }
        }
    }
}
