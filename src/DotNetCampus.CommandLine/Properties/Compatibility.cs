global using DotNetCampus.Cli.Properties;

namespace DotNetCampus.Cli.Properties;

internal static class CompatibilityExtensionMethods
{
#if NETCOREAPP3_1_OR_GREATER
#else
    internal static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.ContainsKey(key))
        {
            return false;
        }
        dictionary.Add(key, value);
        return true;
    }
#endif
}
