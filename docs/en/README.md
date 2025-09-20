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

| Style                               | Flexible      | DotNet        | Gnu           | Posix         | PowerShell    | URL               |
| ----------------------------------- | ------------- | ------------- | ------------- | ------------- | ------------- | ----------------- |
| Case                                | Insensitive   | Sensitive     | Sensitive     | Sensitive     | Insensitive   | Insensitive       |
| Long options                        | Supported     | Supported     | Supported     | Not supported | Supported     | Supported         |
| Short options                       | Supported     | Supported     | Supported     | Supported     | Supported     | Not supported     |
| Option value `=`                    | -o=value      | -o=value      | -o=value      |               |               | option=value      |
| Option value `:`                    | -o:value      | -o:value      |               |               |               |                   |
| Option value (space)                | -o value      | -o value      | -o value      | -o value      | -o value      |                   |
| Boolean option (implicit true)      | -o            | -o            | -o            | -o            | -o            | option            |
| Boolean option (with value)         | -o=true       | -o=true       |               |               | -o:true       | option=true       |
| Boolean values                      | true/false    | true/false    | true/false    | true/false    | true/false    | true/false        |
| Boolean values                      | yes/no        | yes/no        | yes/no        | yes/no        | yes/no        | yes/no            |
| Boolean values                      | on/off        | on/off        | on/off        | on/off        | on/off        | on/off            |
| Boolean values                      | 1/0           | 1/0           | 1/0           | 1/0           | 1/0           | 1/0               |
| Collection option                   | -o A -o B     | -o A -o B     | -o A -o B     | -o A -o B     | -o A -o B     | option=A&option=B |
| Collection option `,`               | -o A,B,C      | -o A,B,C      | -o A,B,C      | -o A,B,C      | -o A,B,C      | -o A,B,C          |
| Collection option `;`               | -o A;B;C      | -o A;B;C      | -o A;B;C      | -o A;B;C      | -o A;B;C      | -o A;B;C          |
| Collection option (space separated) | -o A B C      | -o A B C      |               |               | -o A B C      |                   |
| Dictionary option                   | -o:A=X;B=Y    | -o:A=X;B=Y    |               |               | -o:A=X;B=Y    |                   |
| Combined short booleans             | Not supported | Not supported | -abc          | -abc          | Not supported | Not supported     |
| Single short option multi chars     | -ab           | -ab           | Not supported | Not supported | -ab           | Not supported     |
| Short option directly with value    | Not supported | Not supported | -o1.txt       | Not supported | Not supported | Not supported     |
| Long option prefixes                | `--` `-` `/`  | `--`          | `--`          | (None)        | `-` `/`       |                   |
| Short option prefixes               | `-` `/`       | `-`           | `-`           | `-`           | `-` `/`       |                   |
| Naming                              | --kebab-case  | --kebab-case  | --kebab-case  |               |               | kebab-case        |
| Naming                              | -PascalCase   |               |               |               | -PascalCase   |                   |
| Naming                              | -camelCase    |               |               |               | -camelCase    |                   |
| Naming                              | /PascalCase   |               |               |               | /PascalCase   |                   |
| Naming                              | /camelCase    |               |               |               | /camelCase    |                   |

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

| required | init | Collection | nullable | Behavior         | Explanation                                     |
| -------- | ---- | ---------- | -------- | ---------------- | ----------------------------------------------- |
| 1        | _    | _          | _        | Throw            | Must be supplied; missing raises exception      |
| 0        | 1    | 1          | _        | Empty collection | Collections are never null; missing => empty    |
| 0        | 1    | 0          | 1        | null             | Nullable; missing => null                       |
| 0        | 1    | 0          | 0        | Default value    | Non-nullable; missing => default(T)             |
| 0        | 0    | _          | _        | Keep initial     | Not required/immediate; keeps initializer value |

- 1 = present
- 0 = absent
- _ = regardless

1. Nullable behavior is the same for reference and value types (default value just yields `null` for reference types)
2. Missing required option throws `RequiredPropertyNotAssignedException`
3. "Keep initial" means you may assign an initial value at definition time:

```csharp
// Note: Initial value only applies when neither required nor init is used.
[Option('o', "option-name")]
public string OptionName { get; set; } = "Default Value";
```

## Commands and Subcommands

You can use the command handler pattern to process different commands, like `git commit` or `git remote add`. Multiple ways are provided:

### 1. Delegate-based handlers

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<AddOptions>(options => { /* handle add */ })
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
commandLine
    .AddHandler<ConvertCommandHandler>()
    .AddHandler<FooHandler>()
    .AddHandler<BarHandler>(options => { /* handle remove */ })
    .Run();
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

Corresponding generated source:

```csharp
// After AI translation is finished, human contributors will supplement it.
```

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

| Method                           | Job           | Runtime       |        Mean |     Error |    StdDev |   Gen0 | Allocated |
| -------------------------------- | ------------- | ------------- | ----------: | --------: | --------: | -----: | --------: |
| 'parse [GNU] -v=4.1 -p=flexible' | .NET 10.0     | .NET 10.0     |    355.9 ns |   4.89 ns |   4.58 ns | 0.0548 |     920 B |
| 'parse [GNU] -v=4.1 -p=gnu'      | .NET 10.0     | .NET 10.0     |    339.7 ns |   6.81 ns |   7.57 ns | 0.0548 |     920 B |
| 'parse [GNU] -v=4.0 -p=flexible' | .NET 10.0     | .NET 10.0     |    945.9 ns |  14.87 ns |  13.19 ns | 0.1583 |    2656 B |
| 'parse [GNU] -v=4.0 -p=gnu'      | .NET 10.0     | .NET 10.0     |    882.1 ns |  11.30 ns |  10.57 ns | 0.1631 |    2736 B |
| 'parse [GNU] -v=3.x -p=parser'   | .NET 10.0     | .NET 10.0     |    495.7 ns |   9.26 ns |   9.09 ns | 0.1040 |    1752 B |
| 'parse [GNU] -v=3.x -p=runtime'  | .NET 10.0     | .NET 10.0     | 18,025.5 ns | 194.73 ns | 162.61 ns | 0.4883 |    8730 B |
| 'NuGet: ConsoleAppFramework'     | .NET 10.0     | .NET 10.0     |    134.1 ns |   2.70 ns |   2.65 ns | 0.0215 |     360 B |
| 'parse [GNU] -v=4.1 -p=flexible' | NativeAOT 9.0 | NativeAOT 9.0 |    624.3 ns |   7.06 ns |   6.60 ns | 0.0505 |     856 B |
| 'parse [GNU] -v=4.1 -p=gnu'      | NativeAOT 9.0 | NativeAOT 9.0 |    600.3 ns |   6.72 ns |   6.28 ns | 0.0505 |     856 B |
| 'parse [GNU] -v=4.0 -p=flexible' | NativeAOT 9.0 | NativeAOT 9.0 |  1,395.6 ns |  20.43 ns |  19.11 ns | 0.1507 |    2529 B |
| 'parse [GNU] -v=4.0 -p=gnu'      | NativeAOT 9.0 | NativeAOT 9.0 |  1,438.1 ns |  19.84 ns |  18.55 ns | 0.1545 |    2609 B |
| 'parse [GNU] -v=3.x -p=parser'   | NativeAOT 9.0 | NativeAOT 9.0 |    720.8 ns |   7.47 ns |   6.99 ns | 0.1030 |    1737 B |
| 'parse [GNU] -v=3.x -p=runtime'  | NativeAOT 9.0 | NativeAOT 9.0 |          NA |        NA |        NA |     NA |        NA |
| 'NuGet: ConsoleAppFramework'     | NativeAOT 9.0 | NativeAOT 9.0 |    195.3 ns |   3.76 ns |   3.69 ns | 0.0234 |     392 B |

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

