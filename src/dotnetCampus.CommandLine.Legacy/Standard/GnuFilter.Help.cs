#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

using dotnetCampus.Cli.Utils;

namespace dotnetCampus.Cli.Standard
{
    partial class GnuFilter
    {
        private void PrintHelpText(IReadOnlyList<Type> relatedTypes)
        {
            var selfAssembly = typeof(GnuFilter).Assembly;
            var verbInfoList = relatedTypes
                .Select(x => NameDescription.FromVerbType(x, _resourceManager))
                .OfType<NameDescription>()
                .ToList();
            var mergedOptionInfoList = relatedTypes
                .Where(x => !x.IsDefined(typeof(VerbAttribute)))
                .SelectMany(x => NameDescription.EnumerateFromVerbType(x, _localizableStrings!, _resourceManager)).ToList();

            var maxVerbTextLength = verbInfoList.Count == 0 ? 0 : verbInfoList.Max(x => x.Name.Length);
            var maxOptionTextLength = mergedOptionInfoList.Count == 0 ? 0 : mergedOptionInfoList.Max(x => x.Name.Length);
            var columnLength = Math.Max(maxOptionTextLength, maxVerbTextLength);
            columnLength = Math.Max(12, columnLength);

            Console.Write(_localizableStrings!.UsageHeader);
            if (verbInfoList.Count > 0)
            {
                Console.WriteLine("[options] [command] [command-options] [arguments]");
            }
            else
            {
                Console.WriteLine("[options] [arguments]");
            }

            Console.WriteLine();

            if (mergedOptionInfoList.Count > 0)
            {
                Console.WriteLine(_localizableStrings.OptionsHeader);
            }
            foreach (var x in mergedOptionInfoList)
            {
                Console.Write(GetColumnString(x.Name, columnLength));
                Console.WriteLine(x.Description);
            }

            Console.WriteLine();

            if (verbInfoList.Count > 0)
            {
                Console.WriteLine(_localizableStrings.CommandHeader);
            }
            foreach (var x in verbInfoList)
            {
                Console.Write(GetColumnString(x.Name, columnLength));
                Console.WriteLine(x.Description);
            }
        }

        private void PrintVerbHelpText(Type verbType, string? commandLineVerb)
        {
            var verbAttribute = verbType.GetCustomAttribute<VerbAttribute>();
            if (verbAttribute != null)
            {
                var verb = NamingHelper.MakeKebabCase(verbAttribute.VerbName);
                
                Console.Write(_localizableStrings!.UsageHeader);
                Console.WriteLine($"{verb} [options]");
                Console.WriteLine();

                var optionInfoList = verbType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(x => NameDescription.FromPropertyInfo(x, _localizableStrings, _resourceManager))
                    .OfType<NameDescription>()
                    .ToList();
                var columnLength = optionInfoList.Count == 0 ? 0 : optionInfoList.Max(x => x.Name.Length);

                if (optionInfoList.Count > 0)
                {
                    Console.WriteLine(_localizableStrings.OptionsHeader);
                }
                foreach (var x in optionInfoList)
                {
                    Console.Write(GetColumnString(x.Name, columnLength));
                    Console.WriteLine(x.Description);
                }
            }
            else if (!string.IsNullOrWhiteSpace(commandLineVerb))
            {
                PrintUnknownVerbHelpText(commandLineVerb);
            }
        }

        private void PrintUnknownVerbHelpText(string? verb)
        {
            var (commandDisplayName, commandName) = GetCommandName();
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture,
                _localizableStrings!.UnknownCommandFormat,
                commandDisplayName, commandName, verb));
        }

        private void PrintDetailHelpText(IReadOnlyList<Type> relatedTypes)
        {
            PrintHelpText(relatedTypes);
        }

        private static void PrintVersionText()
        {
            var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrWhiteSpace(version))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("此程序没有指定版本号，建议开发者在开发时通过项目属性指定版本号，或者设置程序集的 AssemblyInformationalVersionAttribute 特性。");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(version);
            }
        }

        /// <summary>
        /// 获取此进程的命令名称。
        /// </summary>
        /// <returns></returns>
        private static ValueTupleSlim<string, string> GetCommandName()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly is null)
            {
                var processName = Process.GetCurrentProcess().ProcessName;
                return new ValueTupleSlim<string, string>(processName, processName);
            }
            else
            {
                var processName = Process.GetCurrentProcess().ProcessName;
                var entryNameWithoutExtension = Path.GetFileNameWithoutExtension(assembly.Location);
                var entryNameWithExtension = Path.GetFileName(assembly.Location);
                if (string.Equals(processName, entryNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return new ValueTupleSlim<string, string>(processName, processName);
                }
                else
                {
                    return new ValueTupleSlim<string, string>(entryNameWithoutExtension, $"{processName} {entryNameWithExtension}");
                }
            }
        }

        private static string GetColumnString(string originalString, int columnLength)
            => $"  {originalString.PadRight(columnLength, ' ')}  ";

        private static string GetLocalizedDescription(CommandLineAttribute attribute, ResourceManager? resourceManager)
            => resourceManager != null && !string.IsNullOrWhiteSpace(attribute.LocalizableDescription)
                ? resourceManager.GetString(attribute.LocalizableDescription, CultureInfo.CurrentUICulture) ?? ""
                : attribute.Description ?? "";

        private static string GetLocalizedDescription(CommandLineAttribute attribute, LocalizableStrings resourceManager)
            => attribute.LocalizableDescription != null && !string.IsNullOrWhiteSpace(attribute.LocalizableDescription)
                ? resourceManager.GetString(attribute.LocalizableDescription, CultureInfo.CurrentUICulture) ?? ""
                : attribute.Description ?? "";
    }
}
