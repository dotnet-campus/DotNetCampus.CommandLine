#nullable enable
using System;
using System.Collections.Generic;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Utils.Parsers;

namespace DotNetCampus.Cli.Tests;

internal class ExperimentalTests
{
    public void Test()
    {
        var foo = CommandLine.Parse(
                ["--bool", "--number:42", "--string", "hello", "--strings", "one", "two", "three", "--dict", "key1=value"],
                CommandLineParsingOptions.DotNet)
            .As<Foo>();
    }
}

internal record Foo
{
    [Option("bool")]
    public required bool BooleanProperty { get; init; }

    [Option("number")]
    public required int NumberProperty { get; init; }

    [Option("string")]
    public required string StringProperty { get; init; }

    [Option("strings")]
    public required IReadOnlyList<string> StringsProperty { get; init; }

    [Option("dict")]
    public required IReadOnlyDictionary<string, string> DictionaryProperty { get; init; }

    [Option("log-level")]
    public required LogLevel LogLevelProperty { get; init; }
}

internal sealed class ExperimentalFooBuilder(CommandLine commandLine)
{
    private BooleanArgument BooleanProperty { get; }
    private NumberArgument NumberProperty { get; }
    private StringArgument StringProperty { get; }
    private StringListArgument StringsProperty { get; }
    private DictionaryArgument DictionaryProperty { get; }
    private __GeneratedEnumPropertyAssignment__LogLevel__ LogLevelProperty { get; }

    public Foo Build()
    {
        var parser = new CommandLineParser(commandLine, "Foo", 0)
        {
            MatchLongOption = MatchLongOption,
            MatchShortOption = MatchShortOption,
            MatchPositionalArguments = MatchPositionalArguments,
            AssignPropertyValue = AssignPropertyValue,
        };
        parser.Parse();
        return BuildCore();
    }

    private OptionValueMatch MatchLongOption(ReadOnlySpan<char> longOption, bool defaultCaseSensitive, CommandNamingPolicy namingPolicy)
    {
        // 先原样匹配一遍。
        if (namingPolicy.SupportsOrdinal())
        {
            var match = longOption switch
            {
                "boolean-property" => new OptionValueMatch(nameof(BooleanProperty), 0, OptionValueType.Boolean),
                _ => OptionValueMatch.NotMatch,
            };
            if (match != OptionValueMatch.NotMatch)
            {
                return match;
            }
        }
        // 再根据命名法匹配一遍（只匹配与上述名称不同的名称）。
        if (namingPolicy.SupportsCamelCase())
        {
            var match = longOption switch
            {
                "boolean-property" => new OptionValueMatch(nameof(BooleanProperty), 0, OptionValueType.Boolean),
                _ => OptionValueMatch.NotMatch,
            };
            return match;
        }
        return OptionValueMatch.NotMatch;
    }

    private OptionValueMatch MatchShortOption(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive)
    {
        var match = shortOption switch
        {
            "b" => new OptionValueMatch(nameof(BooleanProperty), 0, OptionValueType.Boolean),
            _ => OptionValueMatch.NotMatch,
        };
        return match;
    }

    private PositionalArgumentValueMatch MatchPositionalArguments(ReadOnlySpan<char> value, int argumentIndex)
    {
        if (argumentIndex is 0)
        {
            return new PositionalArgumentValueMatch(nameof(StringProperty), 2, PositionalArgumentValueType.Normal);
        }
        return PositionalArgumentValueMatch.NotMatch;
    }

    private void AssignPropertyValue(string propertyName, int propertyIndex, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        _ = propertyIndex switch
        {
            0 => BooleanProperty.Assign(value[0] == '1'),
            1 => NumberProperty.Assign(value),
            2 => StringProperty.Assign(value),
            3 => StringsProperty.Append(value),
            4 => DictionaryProperty.Append(key, value),
            5 => LogLevelProperty.SetValue(value),
            _ => throw new ArgumentOutOfRangeException(nameof(propertyIndex), propertyIndex, null),
        };
    }

    private Foo BuildCore()
    {
        var result = new Foo
        {
            BooleanProperty = BooleanProperty.ToBoolean() ?? throw new InvalidOperationException("BooleanProperty 未被赋值"),
            NumberProperty = NumberProperty.ToInt32() ?? throw new InvalidOperationException("NumberProperty 未被赋值"),
            StringProperty = StringProperty.ToString() ?? throw new InvalidOperationException("StringProperty 未被赋值"),
            StringsProperty = StringsProperty.ToList() ?? throw new InvalidOperationException("StringsProperty 未被赋值"),
            DictionaryProperty = DictionaryProperty.ToDictionary() ?? throw new InvalidOperationException("DictionaryProperty 未被赋值"),
            LogLevelProperty = LogLevelProperty.ToEnum() ?? throw new InvalidOperationException("LogLevelProperty 未被赋值"),
        };

        // 1. [RawArguments]
        // result.MainArgs = commandLine.CommandLineArguments;

        // 2. [Option]
        // There is no option to be assigned.

        // 3. [Value]
        // There is no positional argument to be assigned.

        return result;
    }

    // ReSharper disable once InconsistentNaming
    private struct __GeneratedEnumPropertyAssignment__LogLevel__
    {
        private LogLevel? _value;

        public bool SetValue(ReadOnlySpan<char> value)
        {
            _ = value switch
            {
                "0" or "Debug" or "debug" => _value = LogLevel.Debug,
                "1" or "Info" or "info" => _value = LogLevel.Info,
                "2" or "Warning" or "warning" => _value = LogLevel.Warning,
                "3" or "Error" or "error" => _value = LogLevel.Error,
                "4" or "Fatal" or "fatal" => _value = LogLevel.Critical,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value.ToString(), null),
            };
            return true;
        }

        public LogLevel? ToEnum() => _value;
    }
}
