using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace dotnetCampus.Cli.Utils;

internal class CommandLineValueConverter
{
    [return: NotNullIfNotNull(nameof(arguments))]
    internal static T? OptionStringsToValue<T>(ImmutableArray<string>? arguments)
    {
        var type = typeof(T);
        if (type == typeof(bool))
        {
            return (T)(object)OptionStringsToBoolean(arguments);
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
        return type == typeof(byte) ? (T)(object)OptionStringsToByte(arguments) :
            type == typeof(sbyte) ? (T)(object)OptionStringsToSByte(arguments) :
            type == typeof(char) ? (T)(object)OptionStringsToChar(arguments) :
            type == typeof(decimal) ? (T)(object)OptionStringsToDecimal(arguments) :
            type == typeof(double) ? (T)(object)OptionStringsToDouble(arguments) :
            type == typeof(float) ? (T)(object)OptionStringsToSingle(arguments) :
            type == typeof(int) ? (T)(object)OptionStringsToInt32(arguments) :
            type == typeof(uint) ? (T)(object)OptionStringsToUInt32(arguments) :
            type == typeof(nint) ? (T)(object)OptionStringsToIntPtr(arguments) :
            type == typeof(nuint) ? (T)(object)OptionStringsToUIntPtr(arguments) :
            type == typeof(long) ? (T)(object)OptionStringsToInt64(arguments) :
            type == typeof(ulong) ? (T)(object)OptionStringsToUInt64(arguments) :
            type == typeof(short) ? (T)(object)OptionStringsToInt16(arguments) :
            type == typeof(ushort) ? (T)(object)OptionStringsToUInt16(arguments) :
            type == typeof(string) ? (T)(object)OptionStringsToString(arguments) :
            throw new NotSupportedException($"Option type {type} is not supported.");
    }

    internal static bool OptionStringsToBoolean(ImmutableArray<string>? arguments) => arguments switch
    {
        // 没传选项时，相当于传了 false。
        null => false,
        // 传了选项时，相当于传了 true。
        { Length: 0 } => true,
        // 传了选项，后面还带了参数时，取第一个参数的值作为 true/false。
        { } values => bool.TryParse(values[0], out var result) && result,
    };

    internal static T OptionStringsToEnum<T>(ImmutableArray<string>? arguments) where T : unmanaged, Enum => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => Enum.TryParse<T>(values[0], out var result) ? result : default,
    };

    internal static byte OptionStringsToByte(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => byte.TryParse(values[0], out var result) ? result : default,
    };

    internal static sbyte OptionStringsToSByte(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => sbyte.TryParse(values[0], out var result) ? result : default,
    };

    internal static char OptionStringsToChar(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => char.TryParse(values[0], out var result) ? result : '\0',
    };

    internal static decimal OptionStringsToDecimal(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => decimal.TryParse(values[0], out var result) ? result : 0,
    };

    internal static double OptionStringsToDouble(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => double.TryParse(values[0], out var result) ? result : 0,
    };

    internal static float OptionStringsToSingle(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => float.TryParse(values[0], out var result) ? result : 0,
    };

    internal static int OptionStringsToInt32(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => int.TryParse(values[0], out var result) ? result : 0,
    };

    internal static uint OptionStringsToUInt32(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => uint.TryParse(values[0], out var result) ? result : 0,
    };

    internal static nint OptionStringsToIntPtr(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => nint.TryParse(values[0], out var result) ? result : 0,
    };

    internal static nuint OptionStringsToUIntPtr(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => nuint.TryParse(values[0], out var result) ? result : 0,
    };

    internal static long OptionStringsToInt64(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => long.TryParse(values[0], out var result) ? result : 0,
    };

    internal static ulong OptionStringsToUInt64(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => ulong.TryParse(values[0], out var result) ? result : 0,
    };

    internal static short OptionStringsToInt16(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => short.TryParse(values[0], out var result) ? result : default,
    };

    internal static ushort OptionStringsToUInt16(ImmutableArray<string>? arguments) => arguments switch
    {
        null or { Length: 0 } => default,
        { } values => ushort.TryParse(values[0], out var result) ? result : default,
    };

    [return: NotNullIfNotNull(nameof(arguments))]
    internal static string? OptionStringsToString(ImmutableArray<string>? arguments) => arguments switch
    {
        null => null,
        { Length: 0 } => "",
        { } values => string.Join(" ", values),
    };
}
