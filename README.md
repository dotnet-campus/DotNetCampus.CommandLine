# DotNetCampus.CommandLine

![Build](https://github.com/dotnet-campus/DotNetCampus.CommandLine/actions/workflows/dotnet-build.yml/badge.svg)  ![NuGet Package](https://github.com/dotnet-campus/DotNetCampus.CommandLine/actions/workflows/nuget-tag-publish.yml/badge.svg) [![DotNetCampus.CommandLine](https://img.shields.io/nuget/v/DotNetCampus.CommandLine.svg?label=DotnetCampus.CommandLine)](https://www.nuget.org/packages/DotnetCampus.CommandLine/) [![dotnetCampus.CommandLine.Source](https://img.shields.io/nuget/v/DotnetCampus.CommandLine.Source?label=DotnetCampus.CommandLine.Source)](https://www.nuget.org/packages/DotnetCampus.CommandLine.Source/)

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine is a simple and high-performance command line parsing library for .NET. Benefiting from source generators (and interceptors), it delivers efficient parsing and a friendly development experience across multiple command line styles. All features live under the `DotNetCampus.Cli` namespace.

Benchmarks show parsing a typical command line takes well under a microsecond in many scenarios, placing it among the fastest .NET command line parsers while still pursuing full‑featured syntax support.

## Get Started

For your program `Main` method, write this code:

```csharp
class Program
{
    static void Main(string[] args)
    {
        // Create a new instance of CommandLine type from command line arguments
        var commandLine = CommandLine.Parse(args);

        // Parse the command line into an instance of Options type
        // The source generator will automatically handle the parsing for you
        var options = commandLine.As<Options>();

        // Now use your options object to implement your functionality
    }
}
```

Define a class that maps command line arguments:

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

Then use different command line styles to populate instances of this type (the library supports multiple styles):

| Style      | Example                                                                                    |
| ---------- | ------------------------------------------------------------------------------------------ |
| DotNet     | `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`         |
| PowerShell | `demo.exe 1.txt 2.txt 3.txt -c 20 -TestName BenchmarkTest -DetailLevel High -Debug`        |
| CMD        | `demo.exe 1.txt 2.txt 3.txt /c 20 /TestName BenchmarkTest /DetailLevel High /Debug`        |
| Gnu        | `demo.exe 1.txt 2.txt 3.txt -c 20 --test-name BenchmarkTest --detail-level High --debug`   |
| Flexible   | `demo.exe 1.txt 2.txt 3.txt --count:20 /TestName BenchmarkTest --detail-level=High -Debug` |

## Command Styles and Features

Multiple command line styles are supported; select one when parsing (Flexible is default). Styles differ in case sensitivity, accepted prefixes, separators, and naming forms. A detailed capability matrix (boolean literals, collection parsing forms, naming conventions, URL form, etc.) is documented in the full English guide under `docs/en/README.md`.

Core capabilities:
- Rich option syntax: long & short options; separators `= : space`; multi-value & repeat forms
- Boolean literals: `true/false`, `yes/no`, `on/off`, `1/0`
- Collections & dictionaries: repeat, space, comma, semicolon forms; key-value dictionaries
- Positional arguments: via `[Value(index)]` (ranges supported with `(index, length)` overload — second parameter is count)
- Property semantics: `required`, `init`, nullable reference/value types unified behavior
- Commands & subcommands: multi-word `[Command]` supported with handler chaining or `ICommandHandler`
- URL protocol parsing: `scheme://command/sub/positional1?...` for integration scenarios
- High performance: source generators + interceptors, minimizing allocations
- AOT compatible: no reflection; even enum name lookups are avoided at runtime

For the full feature matrix (including whether a style supports space-separated collections, explicit boolean values, multi-char short option groups, etc.), see the English documentation table.

### Naming

Define options using kebab-case in attributes (e.g., `[Option("test-name")]`). The analyzer warns (`DCL101`) if not kebab-case; we still treat what you write as kebab-case so users may invoke with PascalCase/camelCase depending on style.

### Required Options and Default Values

Modifiers: `required` (must be supplied), `init` (immutable after construction), `?` (nullable). Initial value semantics follow the table in `docs/en/README.md`: required & missing → exception; nullable + init → null; non-nullable collection → empty; non-nullable scalar → default value (value types) or empty string for `string`; otherwise keep initializer.

### Commands and Subcommands

Register handlers with `AddHandler<T>()` or implement `ICommandHandler`. Multi-word `[Command("remote add")]` expresses subcommands. Ambiguity throws `CommandNameAmbiguityException`. Use `RunAsync` if any handler is async.

### URL Protocol

You may express a command invocation as a URL: `dotnet-campus://1.txt/2.txt?count=20&test-name=BenchmarkTest&detail-level=High&debug` enabling shell integration or deep links.

### Performance

Benchmarks (see docs for detailed tables) show very low latency (hundreds of ns typical) and minimal allocations compared to earlier versions and other libraries, while preserving rich syntax coverage.

## Engage, Contribute and Provide Feedback

Thank you very much for firing a new issue and providing new pull requests.

### Issue

Click here to file a new issue:

- [New Issue · dotnet-campus/DotNetCampus.CommandLine](https://github.com/dotnet-campus/DotNetCampus.CommandLine/issues/new)

### Contributing Guide

Be kind.

## License

DotNetCampus.CommandLine is licensed under the [MIT license](/LICENSE).
