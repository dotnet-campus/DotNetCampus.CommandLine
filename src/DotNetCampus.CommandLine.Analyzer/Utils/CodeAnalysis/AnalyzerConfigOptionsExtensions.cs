using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotNetCampus.CommandLine.Temp40.Utils.CodeAnalysis;

internal static class AnalyzerConfigOptionsExtensions
{
    public static bool GetBoolean(this AnalyzerConfigOptions options, string key)
    {
        return options.TryGetValue($"build_property.{key}", out var v)
               && (v?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public static AnalyzerConfigOptionResult TryGetValue<T>(
        this AnalyzerConfigOptions options,
        string key,
        out T value)
        where T : notnull
    {
        if (options.TryGetValue($"build_property.{key}", out var stringValue))
        {
            value = ConvertFromString<T>(stringValue);
            return new AnalyzerConfigOptionResult(options, true)
            {
                UnsetPropertyNames = [],
            };
        }

        value = default!;
        return new AnalyzerConfigOptionResult(options, false)
        {
            UnsetPropertyNames = [key],
        };
    }

    public static AnalyzerConfigOptionResult TryGetValue<T>(
        this AnalyzerConfigOptionResult builder,
        string key,
        out T value)
        where T : notnull
    {
        var options = builder.Options;

        if (options.TryGetValue($"build_property.{key}", out var stringValue))
        {
            value = ConvertFromString<T>(stringValue);
            return builder.Link(true, key);
        }

        value = default!;
        return builder.Link(false, key);
    }

    private static T ConvertFromString<T>(string value)
    {
        if (typeof(T) == typeof(string))
        {
            return (T)(object)value;
        }
        if (typeof(T) == typeof(bool))
        {
            return (T)(object)value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return default!;
    }
}

public readonly record struct AnalyzerConfigOptionResult(AnalyzerConfigOptions Options, bool GotValue)
{
    public required ImmutableList<string> UnsetPropertyNames { get; init; }

    public AnalyzerConfigOptionResult Link(bool result, string propertyName)
    {
        if (result)
        {
            return this;
        }

        if (propertyName is null)
        {
            throw new ArgumentNullException(nameof(propertyName), @"The property name must be specified if the result is false.");
        }

        return this with
        {
            GotValue = false,
            UnsetPropertyNames = UnsetPropertyNames.Add(propertyName),
        };
    }

    public static implicit operator bool(AnalyzerConfigOptionResult result) => result.GotValue;
}
