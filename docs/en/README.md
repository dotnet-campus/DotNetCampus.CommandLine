# Command Line Parser

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine provides simple and high-performance command line parsing. Benefiting from source generators (and interceptors), it now delivers more efficient parsing and a friendlier development experience. All features live under the `DotNetCampus.Cli` namespace.

## Quick Usage

```csharp
class Program
{
    static void Main(string[] args)
    {
        // Create a new CommandLine instance from the command line arguments
        var commandLine = CommandLine.Parse(args);

        // Parse the command line into an instance of the Options type
        // The source generator automatically performs the parsing; no manual parser creation needed
        var options = commandLine.As<Options>();

        // Next, use your options object to implement other functionality
    }
}
```

You need to define a type that contains the mapping for command line arguments:

```csharp
public class Options
{
    [Option("debug")]
    public required bool IsDebugMode { get; init; }

    [Option('c', "count")]
    public required int TestCount { get; init; }

    [Option('n', "test-name")]
    public string? TestName { get; set; }

    [Option("test-category")]
    public string? TestCategory { get; set; }

    [Option('d', "detail-level")]
    public DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

    [Value(0, int.MaxValue)]
    public IReadOnlyList<string> TestItems { get; init; } = null!;
}

public enum DetailLevel
{
    Low,
    Medium,
    High,
}
```

Then use different styles of command lines to populate an instance. The library supports multiple styles:

| Style      | Example                                                                                    |
| ---------- | ------------------------------------------------------------------------------------------ |
| DotNet     | `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`         |
| PowerShell | `demo.exe 1.txt 2.txt 3.txt -c 20 -TestName BenchmarkTest -DetailLevel High -Debug`        |
| CMD        | `demo.exe 1.txt 2.txt 3.txt /c 20 /TestName BenchmarkTest /DetailLevel High /Debug`        |
| Gnu        | `demo.exe 1.txt 2.txt 3.txt -c 20 --test-name BenchmarkTest --detail-level High --debug`   |
| Flexible   | `demo.exe 1.txt 2.txt 3.txt --count:20 /TestName BenchmarkTest --detail-level=High -Debug` |

## Command Line Styles

DotNetCampus.CommandLine supports multiple styles; you can specify one when parsing:

```csharp
// Parse using the .NET CLI style
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

Supported styles include:

- `CommandLineStyle.Flexible` (default): Flexible style offering broad compatibility among styles; case-insensitive by default
- `CommandLineStyle.DotNet`: .NET CLI style; case-sensitive by default
- `CommandLineStyle.Gnu`: GNU-compliant style; case-sensitive by default
- `CommandLineStyle.Posix`: POSIX-compliant style; case-sensitive by default
- `CommandLineStyle.PowerShell`: PowerShell style; case-insensitive by default

By default, their detailed differences are:

| Style                  | Flexible       | DotNet         | Gnu               | Posix         | PowerShell    | URL               |
| ---------------------- | -------------- | -------------- | ----------------- | ------------- | ------------- | ----------------- |
| Positional args        | Supported      | Supported      | Supported         | Supported     | Supported     | Supported         |
| Trailing args `--`     | Supported      | Supported      | Supported         | Supported     | Not supported | Not supported     |
| Case                   | Insensitive    | Sensitive      | Sensitive         | Sensitive     | Insensitive   | Insensitive       |
| Long options           | Supported      | Supported      | Supported         | Not supported | Supported     | Supported         |
| Short options          | Supported      | Supported      | Supported         | Supported     | Supported     | Not supported     |
| Long option prefixes   | `--` `-` `/`   | `--`           | `--`              | (None)        | `-` `/`       |                   |
| Short option prefixes  | `-` `/`        | `-`            | `-`               | `-`           | `-` `/`       |                   |
| Long option (space)    | --option value | --option value | --option value    | -o value      | -o value      |                   |
| Long option `=`        | --option=value | --option=value | --option=value    |               | -o=value      | option=value      |
| Long option `:`        | --option:value | --option:value |                   |               | -o:value      |                   |
| Short option (space)   | -o value       | -o value       | -o value          | -o value      | -o value      |                   |
| Short option `=`       | -o=value       | -o=value       |                   |               | -o=value      | option=value      |
| Short option `:`       | -o:value       | -o:value       |                   |               | -o:value      |                   |
| Short option inline    |                |                | -ovalue           |               |               |                   |
| Multi-char short opt   | -abc value     | -abc value     |                   |               | -abc value    |                   |
| Long boolean option    | --option       | --option       | --option          |               | -Option       | option            |
| Long boolean ` `       | --option true  | --option true  |                   |               | -Option true  |                   |
| Long boolean `=`       | --option=true  | --option=true  | --option=true[^1] |               | -Option=true  |                   |
| Long boolean `:`       | --option:true  | --option:true  |                   |               | -Option:true  |                   |
| Short boolean option   | -o             | -o             | -o                | -o            | -o            |                   |
| Short boolean ` `      | -o true        | -o true        |                   |               | -o true       |                   |
| Short boolean `=`      | -o=true        | -o=true        |                   |               | -o=true       | option=true       |
| Short boolean `:`      | -o:true        | -o:true        |                   |               | -o:true       |                   |
| Short boolean inline   |                |                | -o1               |               |               |                   |
| Boolean values         | true/false     | true/false     | true/false        | true/false    | true/false    | true/false        |
| Boolean values         | yes/no         | yes/no         | yes/no            | yes/no        | yes/no        | yes/no            |
| Boolean values         | on/off         | on/off         | on/off            | on/off        | on/off        | on/off            |
| Boolean values         | 1/0            | 1/0            | 1/0               | 1/0           | 1/0           | 1/0               |
| Combined short bools   |                |                | -abc              | -abc          |               |                   |
| Collection option      | -o A -o B      | -o A -o B      | -o A -o B         | -o A -o B     | -o A -o B     | option=A&option=B |
| Collection (space)[^2] |                |                |                   |               |               |                   |
| Collection `,`         | -o A,B,C       | -o A,B,C       | -o A,B,C          | -o A,B,C      | -o A,B,C      |                   |
| Collection `;`         | -o A;B;C       | -o A;B;C       | -o A;B;C          | -o A;B;C      | -o A;B;C      |                   |
| Dictionary option      | -o:A=X;B=Y     | -o:A=X;B=Y     |                   |               | -o:A=X;B=Y    |                   |
| Naming                 | --kebab-case   | --kebab-case   | --kebab-case      |               |               | kebab-case        |
| Naming                 | -PascalCase    |                |                   |               | -PascalCase   |                   |
| Naming                 | -camelCase     |                |                   |               | -camelCase    |                   |
| Naming                 | /PascalCase    |                |                   |               | /PascalCase   |                   |
| Naming                 | /camelCase     |                |                   |               | /camelCase    |                   |

[^1]: GNU style does not officially support supplying an explicit value to a boolean option, but since the syntax is unambiguous we additionally allow it.
[^2]: All styles default to not supporting space-separated collections, to avoid ambiguity with positional arguments as much as possible. If you need it, you can enable it via `CommandLineParsingOptions.Style.SupportsSpaceSeparatedCollectionValues`.

Notes:

1. Except for PowerShell style, all other styles support using `--` to mark the start of trailing positional arguments; everything after is treated as a positional argument. URL style cannot express trailing positionals.
2. Before `--`, options and positional arguments may be interleaved. The rule: an option greedily consumes following tokens as long as they can be accepted by that option; once it can no longer take a token, the remaining tokens (until the next option or `--`) are treated as positional arguments.

An option takes the immediate values greedily:

For example, if `--option` is a boolean option, then in `--option true text` or `--option 1 text`, the `true` or `1` is consumed by `--option`, and `text` becomes a positional argument.
Another example: if `--option` is a boolean option, `--option text` leaves `text` as a positional argument because it is not a boolean value.
Another example: if a style supports space separated collections (see table), then when `--option a b c` is a collection option, `a` `b` `c` are consumed until the next option or `--`. GNU does not support space separated collections.

## Naming

1. When defining an option in code, you should use kebab-case
   - [Why do this?](https://github.com/dotnet-campus/DotNetCampus.CommandLine/blob/main/docs/analyzers/DCL101.md)
   - If we suspect you did not use kebab-case, we'll emit warning DCL101
   - You may ignore the warning; regardless of the string you write, we treat it as kebab-case (this provides unambiguous word boundary info; see example)
2. After you define a string treated as kebab-case
   - Depending on the style you set, you can use any of kebab-case, PascalCase, and camelCase

Example command line type:

```csharp
[Command("open command-line")]
public class Options
{
    [Option('o', "option-name")]
    public required string OptionName { get; init; }
}
```

Two kebab-case usages here: the `Command` attribute and the `Option` attribute. You can accept:

- DotNet/Gnu style: `demo.exe open command-line --option-name value`
- PowerShell style: `demo.exe Open CommandLine -OptionName value`
- CMD style: `demo.exe Open CommandLine /optionName value`

If you instead write them in other styles, you might get results different from expectations (or maybe intentional):

```csharp
#pragma warning disable DCL101
[Command("Open CommandLine")]
public class Options
{
    // Analyzer warning: OptionName is not kebab-case. Suppress DCL101 if desired.
    [Option('o', "OptionName")]
    public required string OptionName { get; init; }
}
#pragma warning restore DCL101
```

Because we treat them as kebab-case anyway, you will accept:

- DotNet/Gnu style: `demo.exe Open CommandLine --OptionName value`
- PowerShell style: `demo.exe Open CommandLine -OptionName value`
- CMD style: `demo.exe Open CommandLine /optionName value`

## Data Types

The library supports many data types:

1. **Basic types**: string, integer, boolean, enum, etc.
2. **Collection types**: arrays, lists, read-only collections, immutable collections
3. **Dictionary types**: `IDictionary`, `IReadOnlyDictionary`, `ImmutableDictionary`, etc.

See the big table above for how these are passed on the command line.

## Required Options and Default Values

When defining a property, these modifiers apply:

1. Use `required` to mark that an option is mandatory
2. Use `init` to mark that an option is immutable
3. Use `?` to mark that an option is nullable

What value a property ultimately receives depends on:

| required | init | nullable | Collection | Behavior            | Explanation                                        |
| -------- | ---- | -------- | ---------- | ------------------- | -------------------------------------------------- |
| 1        | _    | _        | _          | Throw               | Must be supplied; missing throws exception         |
| 0        | 1    | 1        | _          | null                | Nullable; missing => null                          |
| 0        | 1    | 0        | 1          | Empty collection    | Collections are never null; missing => empty       |
| 0        | 1    | 0        | 0          | Default/empty value | Non-nullable; missing => default value[^2]         |
| 0        | 0    | _        | _          | Keep initial        | Not required or immediate; keeps initializer value |

[^2]: If it's a value type, it receives its default value; if it's a reference type (currently only string), it becomes the empty string `""`.

- 1 = present
- 0 = absent
- _ = regardless of presence

1. Nullable behavior is the same for reference and value types (default value just yields `null` for reference types)
2. Missing required option throws `RequiredPropertyNotAssignedException`
3. "Keep initial" means you may assign an initial value at definition time:

```csharp
// Note: Initial value only applies when neither required nor init is used.
[Option('o', "option-name")]
public string OptionName { get; set; } = "Default Value";
```

## Exceptions

The command-line library's exceptions fall into several categories:

1. Command-line parsing exceptions `CommandLineParseException`
    - Option or positional argument mismatch exceptions
    - Command-line argument format exceptions
    - Command-line value conversion exceptions
2. Command-line object creation exceptions
    - Only one: `RequiredPropertyNotAssignedException`, which occurs when a property marked `required` is not provided in the command line
3. Command and subcommand matching exceptions
    - Multiple match exception `CommandNameAmbiguityException`
    - No match exception `CommandNameNotFoundException`

A common scenario occurs when multiple cooperating applications are not upgraded synchronously; one application might call this program using new command-line options, but the current version cannot recognize options that will only appear in the "next version". In such cases, you might need to ignore these compatibility errors (option or positional argument mismatch exceptions). If you anticipate this situation happening frequently, you can ignore such errors:

```csharp
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet with
{
    // You can ignore only options, only positional arguments, or both like this.
    UnknownArgumentsHandling = UnknownCommandArgumentHandling.IgnoreAllUnknownArguments,
});
```

## Commands and Subcommands

You can use the command handler pattern to process different commands, like `git commit` or `git remote add`. Multiple ways are provided:

### 1. Delegate-based handlers

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.ToRunner()
    .AddHandler<AddOptions>(options => { /* handle add */ })
    .AddHandler<RemoveOptions>(options => { /* handle remove */ })
    .Run();
```

```csharp
[Command("add")]
public class AddOptions
{
    [Value(0)]
    public string ItemToAdd { get; init; }
}

[Command("remove")]
public class RemoveOptions
{
    [Value(0)]
    public string ItemToRemove { get; init; }
}
```

### 2. `ICommandHandler` interface

```csharp
[Command("convert")]
internal class ConvertCommandHandler : ICommandHandler
{
    [Option('i', "input")]
    public required string InputFile { get; init; }

    [Option('o', "output")]
    public string? OutputFile { get; init; }

    [Option('f', "format")]
    public string Format { get; set; } = "json";

    public Task<int> RunAsync()
    {
        // Command handling logic
        Console.WriteLine($"Converting {InputFile} to {Format} format");
        // ...
        return Task.FromResult(0); // Exit code
    }
}
```

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.ToRunner()
    .AddHandler<ConvertCommandHandler>()
    .AddHandler<FooHandler>()
    .AddHandler<BarHandler>(options => { /* handle remove */ })
    .RunAsync();
```

### 3. Using the ICommandHandler<TState> Interface

Sometimes, the program's state is not entirely determined by command line arguments; there may be some internal program state that affects the execution of command line handlers. Since we cannot pass any parameters when using `AddHandler<T>` as shown before, we have other methods to pass state in:

```csharp
using var scope = serviceProvider.BeginScope();
var state = scope.ServiceProvider.GetRequiredService<MyState>();
var commandLine = CommandLine.Parse(args);
commandLine.ToRunner()
    .ForState(state).AddHandler<CommandHandlerWithState>()
    .RunAsync();
```

```csharp
internal class CommandHandlerWithState : ICommandHandler
{
    [Option('o', "option")]
    public required string Option { get; init; }

    public Task<int> RunAsync(MyState state)
    {
        // At this point, you can additionally use the passed-in state.
    }
}
```

If multiple handlers can be executed for the same state, you can keep chaining `AddHandler` calls; if different command handlers need to handle different states, you can use `ForState` again; if no state is needed afterwards, don't pass parameters to `ForState`. Here's a more complex example:

```csharp
commandLine.ToRunner()
    .AddHandler<Handler0>()
    .ForState(state1).AddHandler<Handler1>().AddHandler<Handler2>()
    .ForState(state2).AddHandler<Handler3>()
    .ForState().AddHandler<Handler4>()
    .RunAsync();
```

### Notes

1. `[Command]` supports multiple words, representing subcommands (e.g., `[Command("remote add")]`).
2. Absence of `[Command]`, or one with null/empty string, means default command (`[Command("")]`).
3. If multiple handlers match the same command, `CommandNameAmbiguityException` is thrown.
4. If any handler is asynchronous, you must use `RunAsync` instead of `Run` (otherwise compilation fails).

## URL Protocol Support

DotNetCampus.CommandLine can parse a URL protocol string:

```ini
// scheme://command/subcommand/positional-argument1/positional-argument2?option1=value1&option2=value2
```

The example near the top expressed as URL:

```ini
# `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`
dotnet-campus://1.txt/2.txt?count=20&test-name=BenchmarkTest&detail-level=High&debug
```

Details:

1. Collection options can repeat query names: `tags=csharp&tags=dotnet`
2. Special and non-ASCII characters are URL-decoded automatically

## Source Generators, Interceptors & Performance

DotNetCampus.CommandLine leverages source generators and interceptors for major performance gains.

### Example user code

```csharp
public class BenchmarkOptions41
{
    [Option("debug")]
    public required bool IsDebugMode { get; init; }

    [Option('c', "count")]
    public required int TestCount { get; init; }

    [Option('n', "test-name")]
    public string? TestName { get; set; }

    [Option("test-category")]
    public string? TestCategory { get; set; }

    [Option('d', "detail-level")]
    public DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

    [Value(0, int.MaxValue)]
    public IReadOnlyList<string> TestItems { get; init; } = null!;
}
```

<details>
  <summary>Corresponding generated source</summary>

```csharp
#nullable enable
using global::System;
using global::DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Performance.Fakes;

/// <summary>
/// 辅助 <see cref="global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41"/> 生成命令行选项、子命令或处理函数的创建。
/// </summary>
public sealed class BenchmarkOptions41Builder(global::DotNetCampus.Cli.CommandLine commandLine)
{
    public static readonly global::DotNetCampus.Cli.Compiler.NamingPolicyNameGroup CommandNameGroup = default;

    public static global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41 CreateInstance(global::DotNetCampus.Cli.CommandLine commandLine)
    {
        return new DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41Builder(commandLine).Build();
    }

    private global::DotNetCampus.Cli.Compiler.BooleanArgument IsDebugMode = new();

    private global::DotNetCampus.Cli.Compiler.NumberArgument TestCount = new();

    private global::DotNetCampus.Cli.Compiler.StringArgument TestName = new();

    private global::DotNetCampus.Cli.Compiler.StringArgument TestCategory = new();

    private __GeneratedEnumArgument__DotNetCampus_Cli_Performance_Fakes_DetailLevel__ DetailLevel = new();

    private global::DotNetCampus.Cli.Compiler.StringListArgument TestItems = new();

    public global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41 Build()
    {
        if (commandLine.RawArguments.Count is 0)
        {
            return BuildDefault();
        }

        var parser = new global::DotNetCampus.Cli.Utils.Parsers.CommandLineParser(commandLine, "BenchmarkOptions41", 0)
        {
            MatchLongOption = MatchLongOption,
            MatchShortOption = MatchShortOption,
            MatchPositionalArguments = MatchPositionalArguments,
            AssignPropertyValue = AssignPropertyValue,
        };
        parser.Parse().WithFallback(commandLine);
        return BuildCore(commandLine);
    }

    private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchLongOption(ReadOnlySpan<char> longOption, bool defaultCaseSensitive, global::DotNetCampus.Cli.CommandNamingPolicy namingPolicy)
    {
        // 1. 先匹配 kebab-case 命名法（原样字符串）
        if (namingPolicy.SupportsOrdinal())
        {
            // 1.1 先快速原字符匹配一遍（能应对规范命令行大小写，并优化 DotNet / GNU 风格的性能）。
            switch (longOption)
            {
                case "debug":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(IsDebugMode), 0, global::DotNetCampus.Cli.Compiler.OptionValueType.Boolean);
                case "count":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
                case "test-name":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
                case "test-category":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCategory), 3, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
                case "detail-level":
                    return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }

            // 1.2 再按指定大小写匹配一遍（能应对不规范命令行大小写）。
            var defaultComparison = defaultCaseSensitive
                ? global::System.StringComparison.Ordinal
                : global::System.StringComparison.OrdinalIgnoreCase;
            if (longOption.Equals("debug".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(IsDebugMode), 0, global::DotNetCampus.Cli.Compiler.OptionValueType.Boolean);
            }
            if (longOption.Equals("count".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("test-name".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("test-category".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCategory), 3, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("detail-level".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
        }

        // 2. 再匹配其他命名法（能应对所有不规范命令行大小写，并支持所有风格）。
        if (namingPolicy.SupportsPascalCase())
        {
            var defaultComparison = defaultCaseSensitive
                ? global::System.StringComparison.Ordinal
                : global::System.StringComparison.OrdinalIgnoreCase;
            if (longOption.Equals("Debug".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(IsDebugMode), 0, global::DotNetCampus.Cli.Compiler.OptionValueType.Boolean);
            }
            if (longOption.Equals("Count".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("TestName".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("TestCategory".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCategory), 3, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
            if (longOption.Equals("DetailLevel".AsSpan(), defaultComparison))
            {
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            }
        }

        return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;
    }

    private global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch MatchShortOption(ReadOnlySpan<char> shortOption, bool defaultCaseSensitive)
    {
        // 1. 先快速原字符匹配一遍（能应对规范命令行大小写，并优化 DotNet / GNU 风格的性能）。
        switch (shortOption)
        {
            // 属性 IsDebugMode 没有短名称，无需匹配。
            case "c":
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            case "n":
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
            // 属性 TestCategory 没有短名称，无需匹配。
            case "d":
                return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
        }

        var defaultComparison = defaultCaseSensitive
            ? global::System.StringComparison.Ordinal
            : global::System.StringComparison.OrdinalIgnoreCase;

        // 2. 再按指定大小写指定命名法匹配一遍（能应对不规范命令行大小写）。
        // 属性 IsDebugMode 没有短名称，无需匹配。
        if (shortOption.Equals("c".AsSpan(), defaultComparison))
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestCount), 1, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
        }
        if (shortOption.Equals("n".AsSpan(), defaultComparison))
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(TestName), 2, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
        }
        // 属性 TestCategory 没有短名称，无需匹配。
        if (shortOption.Equals("d".AsSpan(), defaultComparison))
        {
            return new global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch(nameof(DetailLevel), 4, global::DotNetCampus.Cli.Compiler.OptionValueType.Normal);
        }

        return global::DotNetCampus.Cli.Utils.Parsers.OptionValueMatch.NotMatch;
    }

    private global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch MatchPositionalArguments(ReadOnlySpan<char> value, int argumentIndex)
    {
        // 属性 TestItems 覆盖了所有位置参数，直接匹配。
        return new global::DotNetCampus.Cli.Utils.Parsers.PositionalArgumentValueMatch("TestItems", 5, global::DotNetCampus.Cli.Compiler.PositionalArgumentValueType.Normal);
    }

    private void AssignPropertyValue(string propertyName, int propertyIndex, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        switch (propertyIndex)
        {
            case 0:
                IsDebugMode = IsDebugMode.Assign(value);
                break;
            case 1:
                TestCount = TestCount.Assign(value);
                break;
            case 2:
                TestName = TestName.Assign(value);
                break;
            case 3:
                TestCategory = TestCategory.Assign(value);
                break;
            case 4:
                DetailLevel = DetailLevel.Assign(value);
                break;
            case 5:
                TestItems = TestItems.Append(value);
                break;
        }
    }

    private global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41 BuildCore(global::DotNetCampus.Cli.CommandLine commandLine)
    {
        var result = new global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41
        {
            // 1. There is no [RawArguments] property to be initialized.

            // 2. [Option]
            IsDebugMode = IsDebugMode.ToBoolean() ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($"The command line arguments doesn't contain a required option 'debug'. Command line: {commandLine}", "IsDebugMode"),
            TestCount = TestCount.ToInt32() ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($"The command line arguments doesn't contain a required option 'count'. Command line: {commandLine}", "TestCount"),

            // 3. [Value]
            TestItems = TestItems.ToList() ?? [],
        };

        // 1. There is no [RawArguments] property to be assigned.

        // 2. [Option]
        if (TestName.ToString() is { } o0)
        {
            result.TestName = o0;
        }
        if (TestCategory.ToString() is { } o1)
        {
            result.TestCategory = o1;
        }
        if (DetailLevel.ToEnum() is { } o2)
        {
            result.DetailLevel = o2;
        }

        // 3. There is no [Value] property to be assigned.

        return result;
    }

    private global::DotNetCampus.Cli.Performance.Fakes.BenchmarkOptions41 BuildDefault()
    {
        throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($"The command line arguments doesn't contain any required option or positional argument. Command line: {commandLine}", null!);
    }

    /// <summary>
    /// Provides parsing and assignment for the enum type <see cref="global::DotNetCampus.Cli.Performance.Fakes.DetailLevel"/>.
    /// </summary>
    private readonly record struct __GeneratedEnumArgument__DotNetCampus_Cli_Performance_Fakes_DetailLevel__
    {
        /// <summary>
        /// Indicates whether to ignore exceptions when parsing fails.
        /// </summary>
        public bool IgnoreExceptions { get; init; }

        /// <summary>
        /// Stores the parsed enum value.
        /// </summary>
        private global::DotNetCampus.Cli.Performance.Fakes.DetailLevel? Value { get; init; }

        /// <summary>
        /// Assigns a value when a command line input is parsed.
        /// </summary>
        /// <param name="value">The parsed string value.</param>
        public __GeneratedEnumArgument__DotNetCampus_Cli_Performance_Fakes_DetailLevel__ Assign(ReadOnlySpan<char> value)
        {
            Span<char> lowerValue = stackalloc char[value.Length];
            for (var i = 0; i < value.Length; i++)
            {
                lowerValue[i] = char.ToLowerInvariant(value[i]);
            }
            global::DotNetCampus.Cli.Performance.Fakes.DetailLevel? newValue = lowerValue switch
            {
                "low" => global::DotNetCampus.Cli.Performance.Fakes.DetailLevel.Low,
                "medium" => global::DotNetCampus.Cli.Performance.Fakes.DetailLevel.Medium,
                "high" => global::DotNetCampus.Cli.Performance.Fakes.DetailLevel.High,
                _ when IgnoreExceptions => null,
                _ => throw new global::DotNetCampus.Cli.Exceptions.CommandLineParseValueException($"Cannot convert '{value.ToString()}' to enum type 'DotNetCampus.Cli.Performance.Fakes.DetailLevel'."),
            };
            return this with { Value = newValue };
        }

        /// <summary>
        /// Converts the parsed value to the enum type.
        /// </summary>
        public global::DotNetCampus.Cli.Performance.Fakes.DetailLevel? ToEnum() => Value;
    }
}
```

</details>

## Performance Data

Source generator implementation yields very high parsing performance.

Parsing empty command line arguments:

| Method                        |         Mean |      Error |     StdDev |   Gen0 | Allocated |
| ----------------------------- | -----------: | ---------: | ---------: | -----: | --------: |
| 'parse [] -v=4.1 -p=flexible' |     27.25 ns |   0.485 ns |   0.454 ns | 0.0143 |     240 B |
| 'parse [] -v=4.1 -p=dotnet'   |     27.35 ns |   0.471 ns |   0.440 ns | 0.0143 |     240 B |
| 'parse [] -v=4.0 -p=flexible' |     97.16 ns |   0.708 ns |   0.628 ns | 0.0134 |     224 B |
| 'parse [] -v=4.0 -p=dotnet'   |     95.90 ns |   0.889 ns |   0.742 ns | 0.0134 |     224 B |
| 'parse [] -v=3.x -p=parser'   |     49.73 ns |   0.931 ns |   0.870 ns | 0.0239 |     400 B |
| 'parse [] -v=3.x -p=runtime'  | 19,304.17 ns | 194.337 ns | 162.280 ns | 0.4272 |    7265 B |

Parsing GNU style command line arguments:

```bash
test DotNetCampus.CommandLine.Performance.dll DotNetCampus.CommandLine.Sample.dll DotNetCampus.CommandLine.Test.dll -c 20 --test-name BenchmarkTest --detail-level High --debug
```

| Method                           | Runtime       |         Mean |       Error |      StdDev |   Gen0 | Allocated |
| -------------------------------- | ------------- | -----------: | ----------: | ----------: | -----: | --------: |
| 'parse [GNU] -v=4.1 -p=flexible' | .NET 10.0     |     355.9 ns |     4.89 ns |     4.58 ns | 0.0548 |     920 B |
| 'parse [GNU] -v=4.1 -p=gnu'      | .NET 10.0     |     339.7 ns |     6.81 ns |     7.57 ns | 0.0548 |     920 B |
| 'parse [GNU] -v=4.0 -p=flexible' | .NET 10.0     |     945.9 ns |    14.87 ns |    13.19 ns | 0.1583 |    2656 B |
| 'parse [GNU] -v=4.0 -p=gnu'      | .NET 10.0     |     882.1 ns |    11.30 ns |    10.57 ns | 0.1631 |    2736 B |
| 'parse [GNU] -v=3.x -p=parser'   | .NET 10.0     |     495.7 ns |     9.26 ns |     9.09 ns | 0.1040 |    1752 B |
| 'parse [GNU] -v=3.x -p=runtime'  | .NET 10.0     |  18,025.5 ns |   194.73 ns |   162.61 ns | 0.4883 |    8730 B |
| 'NuGet: ConsoleAppFramework'     | .NET 10.0     |     134.1 ns |     2.70 ns |     2.65 ns | 0.0215 |     360 B |
| 'NuGet: CommandLineParser'       | .NET 10.0     | 177,520.8 ns | 2,225.66 ns | 1,737.65 ns | 3.9063 |   68895 B |
| 'NuGet: System.CommandLine'      | .NET 10.0     |  66,581.6 ns | 1,323.17 ns | 3,245.76 ns | 1.0986 |   18505 B |
| 'parse [GNU] -v=4.1 -p=flexible' | NativeAOT 9.0 |     624.3 ns |     7.06 ns |     6.60 ns | 0.0505 |     856 B |
| 'parse [GNU] -v=4.1 -p=gnu'      | NativeAOT 9.0 |     600.3 ns |     6.72 ns |     6.28 ns | 0.0505 |     856 B |
| 'parse [GNU] -v=4.0 -p=flexible' | NativeAOT 9.0 |   1,395.6 ns |    20.43 ns |    19.11 ns | 0.1507 |    2529 B |
| 'parse [GNU] -v=4.0 -p=gnu'      | NativeAOT 9.0 |   1,438.1 ns |    19.84 ns |    18.55 ns | 0.1545 |    2609 B |
| 'parse [GNU] -v=3.x -p=parser'   | NativeAOT 9.0 |     720.8 ns |     7.47 ns |     6.99 ns | 0.1030 |    1737 B |
| 'parse [GNU] -v=3.x -p=runtime'  | NativeAOT 9.0 |           NA |          NA |          NA |     NA |        NA |
| 'NuGet: ConsoleAppFramework'     | NativeAOT 9.0 |     195.3 ns |     3.76 ns |     3.69 ns | 0.0234 |     392 B |
| 'NuGet: CommandLineParser'       | NativeAOT 9.0 |           NA |          NA |          NA |     NA |        NA |
| 'NuGet: System.CommandLine'      | NativeAOT 9.0 |           NA |          NA |          NA |     NA |        NA |

Notes:

1. `parse` means calling `CommandLine.Parse`
2. `handle` means calling `CommandLine.AddHandler`
3. Brackets `[Xxx]` show the style of passed arguments
4. `--flexible`, `--gnu` etc. indicate parser style used (matching improves efficiency)
5. `-v=3.x -p=parser` shows old manually-written parsers (best performance but limited syntax support)
6. `-v=3.x -p=runtime` shows old reflection-based runtime parser
7. `-v=4.0` vs `-v=4.1` illustrate performance evolution
8. `NuGet: ...` rows show performance of other libraries
9. `parse [URL]` rows (omitted above) indicate URL protocol parsing performance

Author's perspective (@walterlv):

1. Fastest library observed is [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework); ours is close and same order of magnitude.
2. Great thanks to ConsoleAppFramework's pursuit of zero dependencies / allocations / reflection; it motivated the current version (`-v4.1`).
3. ConsoleAppFramework targets extreme performance (sacrificing some syntax breadth). Our goal: full-featured plus high performance—so we sit in the same tier but can't surpass it. Choose based on audience and requirements.

