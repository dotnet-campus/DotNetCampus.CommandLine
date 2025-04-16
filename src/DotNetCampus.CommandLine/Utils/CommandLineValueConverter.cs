using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DotNetCampus.Cli.Exceptions;

namespace DotNetCampus.Cli.Utils;

internal static class CommandLineValueConverter
{
    [return: NotNullIfNotNull(nameof(arguments))]
    internal static T? ArgumentStringsToValue<T>(IReadOnlyList<string>? arguments, in ConvertingContext context)
    {
        var type = typeof(T);
        if (type == typeof(bool))
        {
            return (T)(object)ArgumentStringsToBoolean(arguments);
        }
        if (arguments is null or { Count: 0 })
        {
            return default;
        }
        var values = arguments;
        if (type.IsEnum)
        {
            return Enum.TryParse(type, values[0], true, out var result) ? (T)result : default!;
        }
        return type == typeof(byte) ? (T)(object)ArgumentStringsToByte(arguments) :
            type == typeof(sbyte) ? (T)(object)ArgumentStringsToSByte(arguments) :
            type == typeof(char) ? (T)(object)ArgumentStringsToChar(arguments) :
            type == typeof(decimal) ? (T)(object)ArgumentStringsToDecimal(arguments) :
            type == typeof(double) ? (T)(object)ArgumentStringsToDouble(arguments) :
            type == typeof(float) ? (T)(object)ArgumentStringsToSingle(arguments) :
            type == typeof(int) ? (T)(object)ArgumentStringsToInt32(arguments) :
            type == typeof(uint) ? (T)(object)ArgumentStringsToUInt32(arguments) :
            type == typeof(nint) ? (T)(object)ArgumentStringsToIntPtr(arguments) :
            type == typeof(nuint) ? (T)(object)ArgumentStringsToUIntPtr(arguments) :
            type == typeof(long) ? (T)(object)ArgumentStringsToInt64(arguments) :
            type == typeof(ulong) ? (T)(object)ArgumentStringsToUInt64(arguments) :
            type == typeof(short) ? (T)(object)ArgumentStringsToInt16(arguments) :
            type == typeof(ushort) ? (T)(object)ArgumentStringsToUInt16(arguments) :
            type == typeof(string) ? (T)(object)ArgumentStringsToString(arguments, in context) :
            type == typeof(string[]) ? (T)(object)ArgumentStringsToStringArray(arguments) :
            type == typeof(ImmutableArray<string>) ? (T)(object)ArgumentStringsToStringImmutableArray(arguments) :
            type == typeof(ImmutableHashSet<string>) ? (T)(object)ArgumentStringsToStringImmutableHashSet(arguments) :
            type == typeof(ImmutableDictionary<string, string>) ? (T)ArgumentStringsToStringDictionary(arguments) :
            type.IsAssignableFrom(typeof(IReadOnlyList<string>)) ? (T)ArgumentStringsToStringReadOnlyList(arguments) :
            type.IsAssignableFrom(typeof(IReadOnlyDictionary<string, string>)) ? (T)ArgumentStringsToStringDictionary(arguments) :
            throw new NotSupportedException($"Option type {type} is not supported.");
    }

    private static bool ArgumentStringsToBoolean(IReadOnlyList<string>? arguments)
    {
        return arguments switch
        {
            // 没传选项时，相当于传了 false。
            null => false,
            // 传了选项时，相当于传了 true。
            { Count: 0 } => true,
            // 传了选项，后面还带了参数时，取第一个参数的值作为 true/false。
            { } values => ParseBoolean(values[0]) ?? throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid boolean value. Available values are: 1, true, yes, on, 0, false, no, off."),
        };

        static bool? ParseBoolean(string value)
        {
            var isTrue = value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                         value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                         value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                         value.Equals("on", StringComparison.OrdinalIgnoreCase);
            if (isTrue)
            {
                return true;
            }
            var isFalse = value.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                          value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                          value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                          value.Equals("off", StringComparison.OrdinalIgnoreCase);
            if (isFalse)
            {
                return false;
            }
            return null;
        }
    }

#if NET8_0_OR_GREATER
    private static T? ArgumentStringsToValue<T>(IReadOnlyList<string>? arguments) where T : IParsable<T> => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => T.TryParse(values[0], null, out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid value for type {typeof(T).Name}."),
    };
#endif

    private static byte ArgumentStringsToByte(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => byte.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid byte value."),
    };

    private static sbyte ArgumentStringsToSByte(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => sbyte.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid sbyte value."),
    };

    private static char ArgumentStringsToChar(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => char.TryParse(values[0], out var result) ? result : '\0',
    };

    private static decimal ArgumentStringsToDecimal(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => decimal.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid decimal value."),
    };

    private static double ArgumentStringsToDouble(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => double.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid double value."),
    };

    private static float ArgumentStringsToSingle(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => float.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid float value."),
    };

    private static int ArgumentStringsToInt32(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => int.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid int value."),
    };

    private static uint ArgumentStringsToUInt32(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => uint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid uint value."),
    };

    private static nint ArgumentStringsToIntPtr(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => nint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid nint value."),
    };

    private static nuint ArgumentStringsToUIntPtr(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => nuint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid unint value."),
    };

    private static long ArgumentStringsToInt64(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => long.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid long value."),
    };

    private static ulong ArgumentStringsToUInt64(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => ulong.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid ulong value."),
    };

    private static short ArgumentStringsToInt16(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => short.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid short value."),
    };

    private static ushort ArgumentStringsToUInt16(IReadOnlyList<string>? arguments) => arguments switch
    {
        null or { Count: 0 } => default,
        { } values => ushort.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid ushort value."),
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    private static string? ArgumentStringsToString(IReadOnlyList<string>? arguments, in ConvertingContext context) => arguments switch
    {
        null => null,
        { Count: 0 } => "",
        { } values => context.MultiValueHandling switch
        {
            MultiValueHandling.First => values[0],
            MultiValueHandling.Last => values[^1],
            MultiValueHandling.SpaceAll => string.Join(' ', values),
            MultiValueHandling.SlashAll => string.Join('/', values),
            _ => values[0],
        },
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    private static string[]? ArgumentStringsToStringArray(IReadOnlyList<string>? arguments) => arguments switch
    {
        null => null,
        { Count: 0 } => Array.Empty<string>(),
        { } values => values.ToArray(),
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    private static IReadOnlyList<string>? ArgumentStringsToStringReadOnlyList(IReadOnlyList<string>? arguments) => arguments switch
    {
        null => null,
        { Count: 0 } => Array.Empty<string>(),
        { } values => values,
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    private static ImmutableArray<string>? ArgumentStringsToStringImmutableArray(IReadOnlyList<string>? arguments) => arguments switch
    {
        null => null,
        { Count: 0 } => ImmutableArray<string>.Empty,
        { } values => values.ToImmutableArray(),
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    private static ImmutableHashSet<string>? ArgumentStringsToStringImmutableHashSet(IReadOnlyList<string>? arguments) => arguments switch
    {
        null => null,
        { Count: 0 } => ImmutableHashSet<string>.Empty,
        { } values => values.ToImmutableHashSet(),
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    private static IReadOnlyDictionary<string, string>? ArgumentStringsToStringDictionary(IReadOnlyList<string>? arguments) => arguments switch
    {
        null => null,
        { Count: 0 } => new Dictionary<string, string>(),
        { } values => values
            .SelectMany(x => x.Split(';', StringSplitOptions.RemoveEmptyEntries))
            .Select(x =>
            {
                var parts = x.Split('=');
                if (parts.Length is not 2)
                {
                    throw new CommandLineParseValueException(
                        $"Value [{x}] is not a valid dictionary. Expected format is key1=value1;key2=value2.");
                }
                return new KeyValuePair<string, string>(parts[0], parts[1]);
            })
            .GroupBy(x => x.Key)
            .ToImmutableDictionary(x => x.Key, x => x.Last().Value),
    };
}
