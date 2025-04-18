# Command Line Parsing

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine provides simple and high-performance command line parsing functionality. Thanks to the power of source code generators, it now offers more efficient parsing capabilities and a more developer-friendly experience. All functionalities are located under the DotNetCampus.Cli namespace.

## Quick Start

```csharp
class Program
{
    static void Main(string[] args)
    {
        // Create a new instance of CommandLine type from command-line arguments
        var commandLine = CommandLine.Parse(args);

        // Parse the command line into an instance of Options type
        // Source generator will automatically handle the parsing process for you, no need to manually create a parser
        var options = commandLine.As<Options>();

        // Next, write your other functionality using your options object
    }
}
```

You need to define a type that maps to command-line parameters:

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

Then use different command styles in the command line to populate an instance of this type. The library supports multiple command line styles:

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
$ demo.exe "C:/Users/lvyi/Desktop/demo.txt" -s --mode Edit --startup-sessions A B C
```

### .NET CLI Style
```
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" -s:true --mode:Edit --startup-sessions:A;B;C
```

## Command Line Styles

DotNetCampus.CommandLine supports multiple command line styles, and you can specify which style to use during parsing:

```csharp
// Use .NET CLI style to parse command-line arguments
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

Supported styles include:

- `CommandLineStyle.Flexible` (default): Smartly recognizes multiple styles, case-insensitive by default, and is an effective combination of DotNet/GNU/PowerShell styles
  - Supports all styles shown in the previous examples and can correctly parse them
  - Fully supports all command-line features of the DotNet style (including lists and dictionaries)
  - Supports all features of the GNU style except short name parameters (e.g., `-o1.txt`) and short name abbreviations (e.g., `-abc` represents `-a -b -c`)
  - Due to strict Posix rules, Flexible style naturally supports Posix style
  - The DotNet style itself is compatible with PowerShell command line style, so Flexible style also supports PowerShell style
- `CommandLineStyle.Gnu`: Style conforming to the GNU specification, case-sensitive by default
- `CommandLineStyle.Posix`: Style conforming to the POSIX specification, case-sensitive by default
- `CommandLineStyle.DotNet`: .NET CLI style, case-insensitive by default
- `CommandLineStyle.PowerShell`: PowerShell style, case-insensitive by default

## Data Type Support

The library supports parsing of multiple data types:

1. **Basic Types**: Strings, integers, booleans, enums, etc.
2. **Collection Types**: Arrays, lists, read-only collections, immutable collections
3. **Dictionary Types**: IDictionary, IReadOnlyDictionary, ImmutableDictionary, etc.

### Boolean Type Options

For boolean type options, there are multiple ways to specify them in the command line:

- Specifying only the option name indicates `true`: `-s` or `--silence`
- Explicitly specify a value: `-s:true`, `-s=false`, `--silence:on`, `--silence=off`

### Collection Type Options

For collection type options, you can specify the same option multiple times, or use semicolons to separate multiple values:

```
demo.exe --files file1.txt --files file2.txt
demo.exe --files:file1.txt;file2.txt;file3.txt
```

### Dictionary Type Options

For dictionary type options, multiple input methods are supported:

```
demo.exe --properties key1=value1 --properties key2=value2
demo.exe --properties:key1=value1;key2=value2
```

## Positional Arguments

In addition to named options, you can also use positional arguments, specifying the position of the arguments using the `ValueAttribute`:

```csharp
class FileOptions
{
    [Value(0)]
    public string InputFile { get; init; }
    
    [Value(1)]
    public string OutputFile { get; init; }
    
    [Option('v', "verbose")]
    public bool Verbose { get; init; }
}
```

Usage:

```
demo.exe input.txt output.txt --verbose
```

You can also capture multiple positional arguments into an array or collection:

```csharp
class MultiFileOptions
{
    [Value(0, Length = int.MaxValue)]
    public string[] Files { get; init; } = [];
}
```

## Combining Options and Positional Arguments

`ValueAttribute` and `OptionAttribute` can be applied to the same property simultaneously:

```csharp
class Options
{
    [Value(0), Option('f', "file")]
    public string FilePath { get; init; }
}
```

This way, all of the following command lines will assign the file path to the `FilePath` property:

```
demo.exe file.txt
demo.exe -f file.txt
demo.exe --file file.txt
```

## Required and Optional Options

In C# 11 and above, you can use the `required` modifier to mark required options:

```csharp
class Options
{
    [Option('i', "input")]
    public required string InputFile { get; init; }  // Required option
    
    [Option('o', "output")]
    public string? OutputFile { get; init; }         // Optional option
}
```

If a required option is not provided, a `RequiredPropertyNotAssignedException` exception will be thrown during parsing.

## Property Initial Values and Accessor Modifiers

When defining option types, you need to be aware of the relationship between property initial values and accessor modifiers (`init`, `required`):

```csharp
class Options
{
    // Incorrect example: When using init or required, default values will be ignored
    [Option('f', "format")]
    public string Format { get; init; } = "json";  // Default value won't take effect!
    
    // Correct example: Use set to preserve default values
    [Option('f', "format")]
    public string Format { get; set; } = "json";  // Default value will be correctly preserved
}
```

### Important Notes on Property Initial Values

1. **Behavior when using `init` or `required`**:
   - When a property includes the `required` or `init` modifier, the property's initial value will be ignored
   - If the command-line arguments don't provide a value for this option, the property will be set to `default(T)` (which is `null` for reference types)
   - This is determined by C# language features; if the command-line library were to overcome this limitation, it would need to handle all possible combinations of properties, which is obviously very wasteful

2. **Ways to preserve default values**:
   - If you need to provide default values for properties, use `{ get; set; }` instead of `{ get; init; }`

3. **Nullable types and warning handling**:
   - For non-required reference type properties, they should be marked as nullable (e.g., `string?`) to avoid nullable warnings
   - For value types (e.g., `int`, `bool`), if you want to preserve the default value rather than `null`, they should not be marked as nullable

Example:

```csharp
class OptionsBestPractice
{
    // Required option: Use required, no need to worry about default values
    [Option("input")]
    public required string InputFile { get; init; }
    
    // Optional option: Mark as nullable type to avoid warnings
    [Option("output")]
    public string? OutputFile { get; init; }
    
    // Option that needs default value: Use set instead of init
    [Option("format")]
    public string Format { get; set; } = "json";
    
    // Value type option: No need to mark as nullable
    [Option("count")]
    public int Count { get; set; } = 1;
}
```

## Command Handling and Verbs

You can use the command handler pattern to handle different commands (verbs), similar to `git commit`, `git push`, etc. DotNetCampus.CommandLine provides multiple ways to add command handlers:

### 1. Using Delegates to Handle Commands

The simplest way is to handle commands through delegates, separating command option types and handling logic:

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<AddOptions>(options => { /* Handle the add command */ })
    .AddHandler<RemoveOptions>(options => { /* Handle the remove command */ })
    .Run();
```

Use the `Verb` attribute to mark predicates when defining command option classes:

```csharp
[Verb("add")]
public class AddOptions
{
    [Value(0)]
    public string ItemToAdd { get; init; }
}

[Verb("remove")]
public class RemoveOptions
{
    [Value(0)]
    public string ItemToRemove { get; init; }
}
```

### 2. Using the ICommandHandler Interface

For more complex command handling logic, you can create classes that implement the `ICommandHandler` interface, encapsulating command options and handling logic together:

```csharp
[Verb("convert")]
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
        // Implement command handling logic
        Console.WriteLine($"Converting {InputFile} to {Format} format");
        // ...
        return Task.FromResult(0); // Return exit code
    }
}
```

Then add it directly to the command line parser:

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<ConvertCommandHandler>()
    .Run();
```

### 3. Using Assembly Auto-Discovery of Command Handlers

For more convenient management of a large number of commands without manually adding each one, you can use the assembly auto-discovery feature to automatically add all classes in the assembly that implement the `ICommandHandler` interface:

```csharp
// Define a partial class to mark auto-discovery of command handlers
[CollectCommandHandlersFromThisAssembly]
internal partial class AssemblyCommandHandler;

// Add all command handlers at the program entry point
var commandLine = CommandLine.Parse(args);
commandLine.AddHandlers<AssemblyCommandHandler>()
    .Run();
```

Typically, handler classes need to add the `[Verb]` attribute and implement the `ICommandHandler` interface, and they will be automatically discovered and added:

```csharp
[Verb("sample")]
internal class SampleCommandHandler : ICommandHandler
{
    [Option("SampleProperty")]
    public required string Option { get; init; }

    [Value(Length = int.MaxValue)]
    public string? Argument { get; init; }

    public Task<int> RunAsync()
    {
        // Implement command handling logic
        return Task.FromResult(0);
    }
}
```

Additionally, you can create a command handler without the `[Verb]` attribute as the default handler. There can be at most one command handler without the `[Verb]` attribute in the assembly, which will be used when no other commands match:

```csharp
// Default handler without [Verb] attribute
internal class DefaultCommandHandler : ICommandHandler
{
    [Option('h', "help")]
    public bool ShowHelp { get; init; }

    public Task<int> RunAsync()
    {
        // Handle default commands, such as displaying help information
        if (ShowHelp)
        {
            Console.WriteLine("Displaying help information...");
        }
        return Task.FromResult(0);
    }
}
```

This approach is particularly suitable for large applications or command-line tools with strong extensibility, allowing for the addition of new commands without modifying the entry code.

### Asynchronous Command Handling

For commands that need to execute asynchronously, you can use the `RunAsync` method:

```csharp
await commandLine.AddHandler<ImportOptions>(async options => 
{
    await ImportDataAsync(options);
    return 0;
})
.RunAsync();
```

## URL Protocol Support

DotNetCampus.CommandLine supports parsing URL protocol strings:

```
dotnet-campus://open/document.txt?readOnly=true&mode=Display&silence=true&startup-sessions=89EA9D26-6464-4E71-BD04-AA6516063D83
```

Features and usage of URL protocol parsing:

1. The URL path part (such as `open/document.txt` in the example) will be parsed as positional arguments or verb plus positional arguments
   - The first part of the path can serve as a verb (needs to be marked with the `[Verb]` attribute)
   - The subsequent path parts will be parsed as positional arguments
2. Query parameters (the part after `?`) will be parsed as named options
3. Collection type options can be passed multiple values by repeating parameter names, such as: `tags=csharp&tags=dotnet`
4. Special characters and non-ASCII characters in the URL will be automatically URL-decoded

## Naming Conventions and Best Practices

To ensure better compatibility and user experience, we recommend using the kebab-case style for naming long options:

```csharp
// Recommended
[Option('o', "output-file")]
public string OutputFile { get; init; }

// Not recommended
[Option('o', "OutputFile")]
public string OutputFile { get; init; }
```

Benefits of using kebab-case naming:

1. Provides clearer word separation information (e.g., can guess "DotNet-Campus" rather than "Dot-Net-Campus")
2. Resolves digital subordination issues (e.g., whether "Version2Info" is "Version2-Info" or "Version-2-Info")
3. Better compatibility with various command-line styles

## Source Generators, Interceptors, and Performance Optimization

DotNetCampus.CommandLine uses source code generator technology to significantly improve the performance of command-line parsing. The interceptors ([Interceptor](https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md)) make the performance improvement even more impressive.

### How Interceptors Work

When you call methods like `CommandLine.As<T>()` or `CommandLine.AddHandler<T>()`, the source generator automatically generates intercepting code that redirects the call to a high-performance code path generated at compile time. This significantly improves the performance of command-line argument parsing and object creation.

For example, when you write the following code:

```csharp
var options = CommandLine.Parse(args).As<Options>();
```

The source generator will intercept this call and automatically generate code similar to the following to replace the default way of implementing it by looking up creators in a dictionary (older versions used reflection):

```csharp
/// <summary>
/// Interceptor for <see cref="global::DotNetCampus.Cli.CommandLine.As{Options}()"/> method. Intercepts to improve performance.
/// </summary>
[global::System.Runtime.CompilerServices.InterceptsLocation(1, /* Program.Run4xInterceptor @Program.cs */ "G4GJAK7udHFnPkRUqV6VzzgRAABQcm9ncmFtLmNz")]
public static T CommandLine_As_DotNetCampusCliTestsFakesOptions<T>(this global::DotNetCampus.Cli.CommandLine commandLine)
    where T : global::DotNetCampus.Cli.Tests.Fakes.Options
{
    return (T)global::DotNetCampus.Cli.Tests.Fakes.OptionsBuilder.CreateInstance(commandLine);
}
```

### Examples of Source Generator Generated Code

Below is a simple command-line option type and its corresponding generated source code:

```csharp
// Type in user code
internal record DotNet03_MixedOptions
{
    [Option]
    public int Number { get; init; }

    [Option]
    public required string Text { get; init; }

    [Option]
    public bool Flag { get; init; }
}
```

Corresponding generated source:

```csharp
#nullable enable
namespace DotNetCampus.Cli.Tests;

/// <summary>
/// Helper for generating command-line options, verbs, or handler functions for <see cref="global::DotNetCampus.Cli.Tests.DotNet03_MixedOptions"/>.
/// </summary>
internal sealed class DotNet03_MixedOptionsBuilder
{
    public static object CreateInstance(global::DotNetCampus.Cli.CommandLine commandLine)
    {
        var caseSensitive = commandLine.DefaultCaseSensitive;
        var result = new global::DotNetCampus.Cli.Tests.DotNet03_MixedOptions
        {
            Number = commandLine.GetOption("number") ?? default,
            Text = commandLine.GetOption("text") ?? throw new global::DotNetCampus.Cli.Exceptions.RequiredPropertyNotAssignedException($"The command line arguments doesn't contain a required option '--text'. Command line: {commandLine}", "Text"),
            Flag = commandLine.GetOption("flag") ?? default,
            // There is no positional argument to be initialized.
        };
        // There is no option to be assigned.
        // There is no positional argument to be assigned.
        return result;
    }
}
```

Method call in code:

```csharp
_ = CommandLine.Parse(args, CommandLineParsingOptions.DotNet).As<Options>();
```

Corresponding generated source (interceptor):

```csharp
#nullable enable

namespace DotNetCampus.CommandLine.Compiler
{
    file static class Interceptors
    {
        /// <summary>
        /// Interceptor for <see cref="global::DotNetCampus.Cli.CommandLine.As{Options}()"/> method. Intercepts to improve performance.
        /// </summary>
        [global::System.Runtime.CompilerServices.InterceptsLocation(1, /* Program.Run4xInterceptor @Program.cs */ "G4GJAK7udHFnPkRUqV6VzzgRAABQcm9ncmFtLmNz")]
        [global::System.Runtime.CompilerServices.InterceptsLocation(1, /* Program.Run4xModule @Program.cs */ "G4GJAK7udHFnPkRUqV6VzxkSAABQcm9ncmFtLmNz")]
        public static T CommandLine_As_DotNetCampusCliTestsFakesOptions<T>(this global::DotNetCampus.Cli.CommandLine commandLine)
            where T : global::DotNetCampus.Cli.Tests.Fakes.Options
        {
            return (T)global::DotNetCampus.Cli.Tests.Fakes.OptionsBuilder.CreateInstance(commandLine);
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute : global::System.Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
            _ = version;
            _ = data;
        }
    }
}
```

Assembly command handler collection in code:

```csharp
[CollectCommandHandlersFromThisAssembly]
internal partial class AssemblyCommandHandler;
```

Corresponding generated source:

```csharp
#nullable enable
namespace DotNetCampus.Cli.Tests.Fakes;

/// <summary>
/// Provides a way to automatically collect and execute all command line handlers in this assembly.
/// </summary>
partial class AssemblyCommandHandler : global::DotNetCampus.Cli.Compiler.ICommandHandlerCollection
{
    public global::DotNetCampus.Cli.ICommandHandler? TryMatch(string? verb, global::DotNetCampus.Cli.CommandLine cl) => verb switch
    {
        null => throw new global::DotNetCampus.Cli.Exceptions.CommandVerbAmbiguityException($"Multiple command handlers match the same verb name 'null': AmbiguousOptions, CollectionOptions, ComparedOptions, DefaultVerbCommandHandler, DictionaryOptions, FakeCommandOptions, Options, PrimaryOptions, UnlimitedValueOptions, ValueOptions.", null),
        // Type EditOptions does not implement the ICommandHandler interface, so it cannot be dispatched uniformly and must be called by the developer separately.
        "Fake" => (global::DotNetCampus.Cli.ICommandHandler)global::DotNetCampus.Cli.Tests.Fakes.FakeVerbCommandHandlerBuilder.CreateInstance(cl),
        // Type PrintOptions does not implement the ICommandHandler interface, so it cannot be dispatched uniformly and must be called by the developer separately.
        // Type ShareOptions does not implement the ICommandHandler interface, so it cannot be dispatched uniformly and must be called by the developer separately.
        _ => null,
    };
}
```

## Performance Data

The source code generator implementation provides extremely high command line parsing performance:

| Method                                  | Mean            | Error         | StdDev        | Median          | Gen0   | Gen1   | Allocated |
|---------------------------------------- |----------------:|--------------:|--------------:|----------------:|-------:|-------:|----------:|
| 'parse  [] --flexible'                  |        39.16 ns |      0.402 ns |      0.357 ns |        39.15 ns | 0.0124 |      - |     208 B |
| 'parse  [] --gnu'                       |        38.22 ns |      0.518 ns |      0.459 ns |        38.30 ns | 0.0124 |      - |     208 B |
| 'parse  [] --posix'                     |        38.45 ns |      0.792 ns |      0.741 ns |        38.45 ns | 0.0124 |      - |     208 B |
| 'parse  [] --dotnet'                    |        42.14 ns |      0.878 ns |      2.588 ns |        42.06 ns | 0.0124 |      - |     208 B |
| 'parse  [] --powershell'                |        38.67 ns |      0.772 ns |      1.451 ns |        38.42 ns | 0.0124 |      - |     208 B |
| 'parse  [] -v=3.x -p=parser'            |        44.07 ns |      0.665 ns |      0.841 ns |        44.08 ns | 0.0220 |      - |     368 B |
| 'parse  [] -v=3.x -p=runtime'           |       365.36 ns |      7.186 ns |     13.319 ns |       361.47 ns | 0.0367 |      - |     616 B |
| 'parse  [PS1] --flexible'               |       907.15 ns |     17.887 ns |     38.504 ns |       899.46 ns | 0.1612 |      - |    2704 B |
| 'parse  [PS1] --dotnet'                 |       969.51 ns |     18.977 ns |     31.179 ns |       964.56 ns | 0.1612 |      - |    2704 B |
| 'parse  [PS1] -v=3.x -p=parser'         |       448.38 ns |      8.883 ns |     13.830 ns |       445.91 ns | 0.0715 |      - |    1200 B |
| 'parse  [PS1] -v=3.x -p=runtime'        |       835.83 ns |     16.055 ns |     38.774 ns |       830.59 ns | 0.0858 |      - |    1448 B |
| 'parse  [CMD] --flexible'               |       932.31 ns |     18.636 ns |     40.907 ns |       936.14 ns | 0.1612 |      - |    2704 B |
| 'parse  [CMD] --dotnet'                 |       877.96 ns |      8.846 ns |      9.832 ns |       877.67 ns | 0.1612 |      - |    2704 B |
| 'parse  [CMD] -v=3.x -p=parser'         |       438.09 ns |      8.591 ns |     11.469 ns |       433.77 ns | 0.0715 |      - |    1200 B |
| 'parse  [CMD] -v=3.x -p=runtime'        |       822.05 ns |     16.417 ns |     25.560 ns |       811.08 ns | 0.0858 |      - |    1448 B |
| 'parse  [GNU] --flexible'               |       880.14 ns |     17.627 ns |     36.794 ns |       878.35 ns | 0.1574 |      - |    2648 B |
| 'parse  [GNU] --gnu'                    |       811.59 ns |     13.691 ns |     20.492 ns |       805.61 ns | 0.1554 |      - |    2608 B |
| 'parse  [GNU] -v=3.x -p=parser'         |       492.48 ns |      9.757 ns |     11.615 ns |       491.95 ns | 0.0896 |      - |    1512 B |
| 'parse  [GNU] -v=3.x -p=runtime'        |       873.40 ns |     15.873 ns |     24.713 ns |       865.86 ns | 0.1049 |      - |    1760 B |
| 'handle [Edit,Print] --flexible'        |       693.30 ns |     13.894 ns |     28.066 ns |       681.77 ns | 0.2375 | 0.0019 |    3984 B |
| 'handle [Edit,Print] -v=3.x -p=parser'  |       949.15 ns |     18.959 ns |     25.952 ns |       939.97 ns | 0.2775 | 0.0038 |    4648 B |
| 'handle [Edit,Print] -v=3.x -p=runtime' |     6,232.90 ns |    122.601 ns |    217.924 ns |     6,190.80 ns | 0.2594 |      - |    4592 B |
| 'parse  [URL]'                          |     2,942.05 ns |     54.322 ns |     76.152 ns |     2,926.04 ns | 0.4578 |      - |    7704 B |
| 'parse  [URL] -v=3.x -p=parser'         |       121.43 ns |      2.457 ns |      5.496 ns |       121.10 ns | 0.0440 |      - |     736 B |
| 'parse  [URL] -v=3.x -p=runtime'        |       462.92 ns |      9.017 ns |     10.023 ns |       464.26 ns | 0.0587 |      - |     984 B |
| 'NuGet: CommandLineParser'              |   212,745.53 ns |  4,237.822 ns | 11,384.635 ns |   211,418.82 ns | 5.3711 |      - |   90696 B |
| 'NuGet: System.CommandLine'             | 1,751,023.59 ns | 34,134.634 ns | 50,034.108 ns | 1,727,339.45 ns | 3.9063 |      - |   84138 B |

Where:
1. `parse` indicates calling the `CommandLine.Parse` method
2. `handle` indicates calling the `CommandLine.AddHandler` method
3. Square brackets `[Xxx]` indicate the style of command-line arguments passed in
4. `--flexible`, `--gnu`, etc. indicate the parser style used when parsing the incoming command line (highest efficiency when matched)
5. `-v=3.x -p=parser` indicates the performance of manually written parsers passed in the old version (best performance, but the old version supports fewer command-line specifications, and many legal command formats are not supported)
6. `-v=3.x -p=runtime` indicates the performance of the old version using the default reflection parser
7. `NuGet: CommandLineParser` and `NuGet: System.CommandLine` indicate the performance when using the corresponding NuGet packages to parse command-line arguments
8. `parse [URL]` indicates the performance when parsing URL protocol strings

Thanks to source generators and interceptors, the new version:
1. Completes a parsing in about 0.8μs (microseconds) (Benchmark)
2. During application startup, completing one parsing only takes about 34μs
3. During application startup, including dll loading and type initialization, one parsing takes about 8ms (using AOT compilation can reduce it back to 34μs).
