using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;

using dotnetCampus.Cli.Utils;

namespace dotnetCampus.Cli.Standard
{
    partial class GnuFilter
    {
        /// <summary>
        /// 名称和描述。
        /// </summary>
        private readonly struct NameDescription
        {
            public NameDescription(string name, string description) : this()
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Description = description ?? throw new ArgumentNullException(nameof(description));
            }

            /// <summary>
            /// 命令行选项/值/命令的标准化语法。
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// 命令行选项/值/命令的本地化描述。
            /// </summary>
            public string Description { get; }

            internal static NameDescription? FromVerbType(Type verbType,
                ResourceManager? _resourceManager)
            {
                if (verbType.IsDefined(typeof(VerbAttribute)))
                {
                    var attribute = verbType.GetCustomAttribute<VerbAttribute>()!;

                    var name = attribute.VerbName;
                    var description = GetLocalizedDescription(attribute, _resourceManager);

                    return new NameDescription(name, description);
                }
                else
                {
                    return default;
                }
            }

            internal static NameDescription? FromPropertyInfo(PropertyInfo propertyInfo,
                LocalizableStrings _localizableStrings,
                ResourceManager? _resourceManager)
            {
                var selfAssembly = typeof(GnuFilter).Assembly;
                if (propertyInfo.IsDefined(typeof(OptionAttribute)))
                {
                    var verbType = propertyInfo.DeclaringType!;
                    var propertyType = propertyInfo.PropertyType;
                    var attribute = propertyInfo.GetCustomAttribute<OptionAttribute>()!;

                    var shortName = attribute.ShortName;
                    var longName = attribute.LongName ?? propertyInfo.Name;
                    var typeName = GetTypeName(propertyType, attribute.TypeName);

                    var name = $"{(shortName is null ? "" : $"{shortName}|")}--{NamingHelper.MakeKebabCase(longName)}{(typeName is null ? "" : $" <{typeName}>")}";
                    var description = verbType.Assembly == selfAssembly
                        ? GetLocalizedDescription(attribute, _localizableStrings)
                        : GetLocalizedDescription(attribute, _resourceManager);

                    return new NameDescription(name, description);
                }
                else
                {
                    return default;
                }
            }

            internal static IEnumerable<NameDescription> EnumerateFromVerbType(Type type,
                LocalizableStrings _localizableStrings,
                ResourceManager? _resourceManager)
            {
                return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(x => FromPropertyInfo(x, _localizableStrings, _resourceManager))
                    .OfType<NameDescription>();
            }

            private static string? GetTypeName(Type type, string? typeName)
            {
                if (typeName != null && !string.IsNullOrWhiteSpace(typeName))
                {
                    return typeName;
                }

                return CommandLineAssigningExtensions.GetAssignableTypeName(type);
            }
        }
    }
}
