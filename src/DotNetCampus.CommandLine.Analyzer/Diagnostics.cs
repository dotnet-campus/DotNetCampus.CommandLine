using DotNetCampus.CommandLine.Temp40.Properties;
using Microsoft.CodeAnalysis;
using static DotNetCampus.CommandLine.Temp40.Properties.Localizations;

// ReSharper disable InconsistentNaming

namespace DotNetCampus.CommandLine.Temp40;

public static class Diagnostics
{
    #region Command/Value/Options Definition 101-199

    public static readonly DiagnosticDescriptor DCL101_OptionLongNameMustBeKebabCase = new DiagnosticDescriptor(
        nameof(DCL101),
        Localize(nameof(DCL101)),
        Localize(nameof(DCL101_Message)),
        Categories.AvoidBugs,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Localize(nameof(DCL101_Description)),
        helpLinkUri: Url(OptionLongNameMustBeKebabCase));

    public static readonly DiagnosticDescriptor DCL102_OptionLongNameCanBeKebabCase = new DiagnosticDescriptor(
        nameof(DCL102),
        Localize(nameof(DCL102)),
        Localize(nameof(DCL102_Message)),
        Categories.AvoidBugs,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Localize(nameof(DCL102_Description)),
        helpLinkUri: Url(OptionLongNameCanBeKebabCase));

    #endregion

    #region Options Properties 201-299

    public static readonly DiagnosticDescriptor DCL201_SupportedOptionPropertyType = new DiagnosticDescriptor(
        nameof(DCL201),
        Localize(nameof(DCL201)),
        Localize(nameof(DCL201_Message)),
        Categories.CodeFixOnly,
        DiagnosticSeverity.Hidden,
        isEnabledByDefault: true,
        description: Localize(nameof(DCL201_Description)),
        helpLinkUri: Url(SupportedOptionPropertyType));

    public static readonly DiagnosticDescriptor DCL202_NotSupportedOptionPropertyType = new DiagnosticDescriptor(
        nameof(DCL202),
        Localize(nameof(DCL202)),
        Localize(nameof(DCL202_Message)),
        Categories.RuntimeException,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize(nameof(DCL202_Description)),
        helpLinkUri: Url(NotSupportedOptionPropertyType));

    public static readonly DiagnosticDescriptor DCL203_NotSupportedRawArgumentsPropertyType = new DiagnosticDescriptor(
        nameof(DCL203),
        Localize(nameof(DCL203)),
        Localize(nameof(DCL203_Message)),
        Categories.Mechanism,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize(nameof(DCL203_Description)),
        helpLinkUri: Url(NotSupportedRawArgumentsPropertyType));

    #endregion

    public const string OptionLongNameMustBeKebabCase = "DCL101";
    public const string OptionLongNameCanBeKebabCase = "DCL102";
    public const string SupportedOptionPropertyType = "DCL201";
    public const string NotSupportedOptionPropertyType = "DCL202";
    public const string NotSupportedRawArgumentsPropertyType = "DCL203";

    private static class Categories
    {
        /// <summary>
        /// 可能产生 bug，则报告此诊断。
        /// </summary>
        public const string AvoidBugs = "DotNetCampus.AvoidBugs";

        /// <summary>
        /// 为了提供代码生成能力，则报告此诊断。
        /// </summary>
        public const string CodeFixOnly = "DotNetCampus.CodeFixOnly";

        /// <summary>
        /// 因编译要求而必须满足的条件没有满足，则报告此诊断。
        /// </summary>
        public const string Compiler = "DotNetCampus.Compiler";

        /// <summary>
        /// 因库内的机制限制，必须满足此要求后库才可正常工作，则报告此诊断。
        /// </summary>
        public const string Mechanism = "DotNetCampus.Mechanism";

        /// <summary>
        /// 为了代码可读性，使之更易于理解、方便调试，则报告此诊断。
        /// </summary>
        public const string Readable = "DotNetCampus.Readable";

        /// <summary>
        /// 为了提升性能，或避免性能问题，则报告此诊断。
        /// </summary>
        public const string Performance = "DotNetCampus.Performance";

        /// <summary>
        /// 能写得出来正常编译，但会引发运行时异常，则报告此诊断。
        /// </summary>
        public const string RuntimeException = "DotNetCampus.RuntimeException";

        /// <summary>
        /// 编写了无法生效的代码，则报告此诊断。
        /// </summary>
        public const string Useless = "DotNetCampus.Useless";
    }

    private static LocalizableString Localize(string key) => new LocalizableResourceString(key, ResourceManager, typeof(Localizations));

    public static string Url(string diagnosticId) => $"https://github.com/dotnet-campus/DotNetCampus.CommandLine/docs/analyzers/{diagnosticId}.md";
}
