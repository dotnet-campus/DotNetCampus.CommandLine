# DotNetCampus.CommandLine

![Build](https://github.com/dotnet-campus/DotNetCampus.CommandLine/actions/workflows/dotnet-build.yml/badge.svg)  ![NuGet Package](https://github.com/dotnet-campus/DotNetCampus.CommandLine/actions/workflows/nuget-tag-publish.yml/badge.svg) [![DotNetCampus.CommandLine](https://img.shields.io/nuget/v/DotNetCampus.CommandLine.svg?label=DotnetCampus.CommandLine)](https://www.nuget.org/packages/DotnetCampus.CommandLine/) [![dotnetCampus.CommandLine.Source](https://img.shields.io/nuget/v/DotnetCampus.CommandLine.Source?label=DotnetCampus.CommandLine.Source)](https://www.nuget.org/packages/DotnetCampus.CommandLine.Source/)

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine is a simple yet high-performance command line parsing library for .NET. Powered by source generators (and interceptors), it delivers efficient parsing and a friendly development experience across multiple command line styles.

Parsing a typical command line takes only about 0.8μs (microseconds) in benchmarks, placing it among the fastest .NET command line parsers while still pursuing full-featured syntax support.

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
class Options
{
    [Value(0)]
    public required string FilePath { get; init; }

    [Option('s', "silence")]
    public bool IsSilence { get; init; }

    [Option('m', "mode")]
    public string? StartMode { get; init; }

    [Option("startup-sessions")]
    public IReadOnlyList<string> StartupSessions { get; init; } = [];
}
```

Then use different command line styles to populate instances of this type:

### Windows PowerShell Style

```powershell
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" -s -Mode Edit -StartupSessions A B C
```

### Windows CMD Style

```cmd
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" /s /Mode Edit /StartupSessions A B C
```

### Linux/GNU Style

```bash
$ demo.exe "C:/Users/lvyi/Desktop/demo.txt" -s --mode Edit --startup-sessions A --startup-sessions B --startup-sessions C
```

### .NET CLI Style
```
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" -s:true --mode:Edit --startup-sessions:A;B;C
```

## Command Styles and Features

The library supports multiple command line styles via `CommandLineStyle` (Flexible/DotNet/Gnu/Posix/PowerShell) and a Flexible default that offers broad compatibility (case sensitivity depends on style). Key capabilities include:
- Rich option syntax (long/short options, multiple separators `= : space`)
- Booleans with multiple literal forms (true/false, yes/no, on/off, 1/0)
- Collections (repeat, comma, semicolon, space forms) and dictionaries
- Positional arguments via `ValueAttribute` (ranges allowed)
- Required / nullable / immutable (`required` / `init`) property semantics
- Command & subcommand handling (multi-word `[Command]` supported)
- Optional URL protocol parsing (`schema://...` form)
- High performance from source generators and interceptors

## Engage, Contribute and Provide Feedback

Thank you very much for firing a new issue and providing new pull requests.

### Issue

Click here to file a new issue:

- [New Issue · dotnet-campus/DotNetCampus.CommandLine](https://github.com/dotnet-campus/DotNetCampus.CommandLine/issues/new)

### Contributing Guide

Be kindly.

## License

DotNetCampus.CommandLine is licensed under the [MIT license](/LICENSE).
