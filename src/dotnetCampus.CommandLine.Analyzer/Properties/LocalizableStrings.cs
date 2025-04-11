using Microsoft.CodeAnalysis;

namespace dotnetCampus.CommandLine.Properties;

public static class LocalizableStrings
{
    public static LocalizableString Get(string key) => new LocalizableResourceString(key, Resources.ResourceManager, typeof(Resources));
}
