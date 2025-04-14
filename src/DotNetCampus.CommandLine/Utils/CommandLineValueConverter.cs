using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DotNetCampus.Cli.Exceptions;

namespace DotNetCampus.Cli.Utils;

internal static class CommandLineValueConverter
{
    [return: NotNullIfNotNull(nameof(arguments))]
    internal static T? ArgumentStringsToValue<T>(ImmutableArray<string>? arguments, ConvertingContext context)
    {
        var type = typeof(T);
        if (type == typeof(bool))
        {
            return (T)(object)ArgumentStringsToBoolean(arguments);
        }
        if (arguments is null or { Length: 0 })
        {
            return default;
        }
        var values = arguments.Value;
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
            type == typeof(string) ? (T)(object)ArgumentStringsToString(arguments, context) :
            type == typeof(string[]) ? (T)(object)ArgumentStringsToStringArray(arguments) :
            type == typeof(ImmutableArray<string>) ? (T)(object)ArgumentStringsToStringImmutableArray(arguments) :
            type == typeof(ImmutableHashSet<string>) ? (T)(object)ArgumentStringsToStringImmutableHashSet(arguments) :
            type == typeof(ImmutableDictionary<string, string>) ? (T)(object)ArgumentStringsToStringDictionary(arguments) :
            type.IsAssignableFrom(typeof(IReadOnlyList<string>)) ? (T)(object)ArgumentStringsToStringReadOnlyList(arguments) :
            type.IsAssignableFrom(typeof(IReadOnlyDictionary<string, string>)) ? (T)(object)ArgumentStringsToStringDictionary(arguments) :
            throw new NotSupportedException($"Option type {type} is not supported.");
    }

    internal static bool ArgumentStringsToBoolean(ImmutableArray<string>? arguments)
    {
        return arguments switch
        {
            // 没传选项时，相当于传了 false。
            null => false,
            // 传了选项时，相当于传了 true。
            { Length: 0 } => true,
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

    private static T? ArgumentStringsToValue<T>(ImmutableArray<string>? arguments) where T : IParsable<T> => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => T.TryParse(values[0], null, out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid value for type {typeof(T).Name}."),
    };

    internal static byte ArgumentStringsToByte(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => byte.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid byte value."),
    };

    internal static sbyte ArgumentStringsToSByte(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => sbyte.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid sbyte value."),
    };

    internal static char ArgumentStringsToChar(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => char.TryParse(values[0], out var result) ? result : '\0',
    };

    internal static decimal ArgumentStringsToDecimal(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => decimal.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid decimal value."),
    };

    internal static double ArgumentStringsToDouble(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => double.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid double value."),
    };

    internal static float ArgumentStringsToSingle(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => float.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid float value."),
    };

    internal static int ArgumentStringsToInt32(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => int.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid int value."),
    };

    internal static uint ArgumentStringsToUInt32(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => uint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid uint value."),
    };

    internal static nint ArgumentStringsToIntPtr(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => nint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid nint value."),
    };

    internal static nuint ArgumentStringsToUIntPtr(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => nuint.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid unint value."),
    };

    internal static long ArgumentStringsToInt64(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => long.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid long value."),
    };

    internal static ulong ArgumentStringsToUInt64(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => ulong.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid ulong value."),
    };

    internal static short ArgumentStringsToInt16(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => short.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid short value."),
    };

    internal static ushort ArgumentStringsToUInt16(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => ushort.TryParse(values[0], out var result)
            ? result
            : throw new CommandLineParseValueException(
                $"Value [{values[0]}] is not a valid ushort value."),
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    internal static string? ArgumentStringsToString(ImmutableArray<string>? arguments, ConvertingContext context) => arguments switch
    {
        null => null,
        { Length: 0 } => "",
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
    internal static string[]? ArgumentStringsToStringArray(ImmutableArray<string>? arguments) => arguments switch
    {
        null => null,
        { Length: 0 } => Array.Empty<string>(),
        { } values => values.ToArray(),
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    internal static IReadOnlyList<string>? ArgumentStringsToStringReadOnlyList(ImmutableArray<string>? arguments) => arguments switch
    {
        null => null,
        { Length: 0 } => Array.Empty<string>(),
        { } values => values,
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    internal static ImmutableArray<string>? ArgumentStringsToStringImmutableArray(ImmutableArray<string>? arguments) => arguments switch
    {
        null => null,
        { Length: 0 } => ImmutableArray<string>.Empty,
        { } values => values,
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    internal static ImmutableHashSet<string>? ArgumentStringsToStringImmutableHashSet(ImmutableArray<string>? arguments) => arguments switch
    {
        null => null,
        { Length: 0 } => ImmutableHashSet<string>.Empty,
        { } values => values.ToImmutableHashSet(),
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    internal static IReadOnlyDictionary<string, string>? ArgumentStringsToStringDictionary(ImmutableArray<string>? arguments) => arguments switch
    {
        null => null,
        { Length: 0 } => new Dictionary<string, string>(),
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
