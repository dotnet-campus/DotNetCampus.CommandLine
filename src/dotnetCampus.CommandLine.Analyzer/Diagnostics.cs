using dotnetCampus.CommandLine.Properties;
using Microsoft.CodeAnalysis;
using static dotnetCampus.CommandLine.Properties.Localizations;

namespace dotnetCampus.CommandLine;

public static class Diagnostics
{
    #region Verb/Value/Options Definition 101-199

    public static readonly DiagnosticDescriptor DCL101_OptionLongNameMustBeKebabCase = new DiagnosticDescriptor(
        OptionLongNameMustBeKebabCase,
        Localize(nameof(OptionLongNameMustBeKebabCaseTitle)),
        Localize(nameof(OptionLongNameMustBeKebabCaseMessage)),
        "dotnetCampus.Naming",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize(nameof(OptionLongNameMustBeKebabCaseDescription)),
        helpLinkUri: Url(OptionLongNameMustBeKebabCase));

    #endregion

    #region Options Properties 201-299

    /// <summary>
    /// Supported diagnostics.
    /// </summary>
    public static readonly DiagnosticDescriptor DCL201_SupportedOptionPropertyType = new DiagnosticDescriptor(
        SupportedOptionPropertyType,
        Localize(nameof(SupportedOptionPropertyTypeTitle)),
        Localize(nameof(SupportedOptionPropertyTypeMessage)),
        "dotnetCampus.Usage",
        DiagnosticSeverity.Hidden,
        isEnabledByDefault: true,
        description: Localize(nameof(SupportedOptionPropertyTypeDescription)),
        helpLinkUri: Url(SupportedOptionPropertyType));

    /// <summary>
    /// Supported diagnostics.
    /// </summary>
    public static readonly DiagnosticDescriptor DCL202_NotSupportedOptionPropertyType = new DiagnosticDescriptor(
        NotSupportedOptionPropertyType,
        Localize(nameof(NotSupportedOptionPropertyTypeTitle)),
        Localize(nameof(NotSupportedOptionPropertyTypeMessage)),
        "dotnetCampus.Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize(nameof(NotSupportedOptionPropertyTypeDescription)),
        helpLinkUri: Url(NotSupportedOptionPropertyType));

    #endregion

    public const string OptionLongNameMustBeKebabCase = "DCL101";
    public const string SupportedOptionPropertyType = "DCL201";
    public const string NotSupportedOptionPropertyType = "DCL202";

    private static class Categories
    {
        /// <summary>
        /// 可能产生 bug，则报告此诊断。
        /// </summary>
        public const string AvoidBugs = "dotnetCampus.AvoidBugs";

        /// <summary>
        /// 为了提供代码生成能力，则报告此诊断。
        /// </summary>
        public const string CodeFixOnly = "dotnetCampus.CodeFixOnly";

        /// <summary>
        /// 因编译要求而必须满足的条件没有满足，则报告此诊断。
        /// </summary>
        public const string Compiler = "dotnetCampus.Compiler";

        /// <summary>
        /// 因库内的机制限制，必须满足此要求后库才可正常工作，则报告此诊断。
        /// </summary>
        public const string Mechanism = "dotnetCampus.Mechanism";

        /// <summary>
        /// 为了代码可读性，使之更易于理解、方便调试，则报告此诊断。
        /// </summary>
        public const string Readable = "dotnetCampus.Readable";

        /// <summary>
        /// 为了提升性能，或避免性能问题，则报告此诊断。
        /// </summary>
        public const string Performance = "dotnetCampus.Performance";

        /// <summary>
        /// 能写得出来正常编译，但会引发运行时异常，则报告此诊断。
        /// </summary>
        public const string RuntimeException = "dotnetCampus.RuntimeException";

        /// <summary>
        /// 编写了无法生效的代码，则报告此诊断。
        /// </summary>
        public const string Useless = "dotnetCampus.Useless";
    }

    private static LocalizableString Localize(string key) => new LocalizableResourceString(key, ResourceManager, typeof(Localizations));

    public static string Url(string diagnosticId) => $"https://github.com/dotnet-campus/dotnetCampus.CommandLine/docs/analyzers/{diagnosticId}.md";
}
