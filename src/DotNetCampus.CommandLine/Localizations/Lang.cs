using DotNetCampus.Localizations;

namespace DotNetCampus.Cli.Localizations;

[LocalizedConfiguration(Default = "en",
    EnsureKeysIdentical = true,
    DependencyMode = DependencyMode.NestedSource,
    GenerationMode = GenerationMode.Compiled,
    NotificationMode = NotificationMode.InitOnly)]
internal partial class Lang
{
}
