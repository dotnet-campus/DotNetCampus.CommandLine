# Command Line Parser

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine provides a simple yet high-performance command line parsing functionality. Thanks to the power of source code generators, it now offers more efficient parsing capabilities and a more developer-friendly experience. All features are available under the DotNetCampus.Cli namespace.

## Quick Start

```csharp
class Program
{
    static void Main(string[] args)
    {
        // Create a new instance of CommandLine type from command line arguments
        var commandLine = CommandLine.Parse(args);

        // Parse the command line into an instance of Options type
        // The source generator will automatically handle the parsing for you, no need to manually create a parser
        var options = commandLine.As<Options>();

        // Now use your options object to implement other functionalities
    }
}
```

You need to define a type that maps command line arguments:

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

Then use different command line styles to populate instances of this type. The library supports multiple command line styles:

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

DotNetCampus.CommandLine supports multiple command line styles. You can specify which style to use when parsing:

```csharp
// Parse command line arguments using .NET CLI style
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

Supported styles include:

- `CommandLineStyle.Flexible` (default): Intelligently recognizes multiple styles
- `CommandLineStyle.GNU`: GNU standard compliant style
- `CommandLineStyle.POSIX`: POSIX standard compliant style
- `CommandLineStyle.DotNet`: .NET CLI style
- `CommandLineStyle.PowerShell`: PowerShell style

## Data Type Support

The library supports parsing of multiple data types:

1. **Basic Types**: Strings, integers, booleans, enums, etc.
2. **Collection Types**: Arrays, lists, read-only collections, immutable collections
3. **Dictionary Types**: IDictionary, IReadOnlyDictionary, ImmutableDictionary, etc.

### Boolean Type Options

For boolean type options, there are multiple ways to specify them in the command line:

- Just specify the option name to indicate `true`: `-s` or `--silence`
- Explicitly specify the value: `-s:true`, `-s=false`, `--silence:on`, `--silence=off`

### Collection Type Options

For collection type options, you can specify the same option multiple times or use semicolons to separate multiple values:

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

In addition to named options, you can use positional arguments by specifying the position using `ValueAttribute`:

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

In C# 11 and above, you can mark required options using the `required` modifier:

```csharp
class Options
{
    [Option('i', "input")]
    public required string InputFile { get; init; }  // Required option
    
    [Option('o', "output")]
    public string? OutputFile { get; init; }         // Optional option
}
```

If a required option is not provided, a `RequiredPropertyNotAssignedException` will be thrown during parsing.

## Property Initial Values and Accessors

When defining option types, it's important to understand the relationship between property initial values and accessor modifiers (`init`, `required`):

```csharp
class Options
{
    // Incorrect example: when using init or required, default values will be ignored
    [Option('f', "format")]
    public string Format { get; init; } = "json";  // Default value won't work!
    
    // Correct example: use set to retain the default value
    [Option('f', "format")]
    public string Format { get; set; } = "json";  // Default value will be preserved
}
```

### Important Notes About Property Initial Values

1. **Behavior with `init` or `required`**:
   - When a property includes `required` or `init` modifiers, the initial value will be ignored
   - If the option is not provided in command line arguments, the property will be set to `default(T)` (null for reference types)
   - This is determined by C# language design, not a limitation of the command line library

2. **How to preserve default values**:
   - If you need to provide a default value for a property, use `{ get; set; }` instead of `{ get; init; }`

3. **Nullable types and warning handling**:
   - For non-required reference type properties, mark them as nullable (e.g., `string?`) to avoid nullable warnings
   - For value types (e.g., `int`, `bool`), if you want to keep the default value rather than null, don't mark them as nullable

Example:

```csharp
class OptionsBestPractice
{
    // Required option: using required, no need to worry about default values
    [Option("input")]
    public required string InputFile { get; init; }
    
    // Optional option: mark as nullable type to avoid warnings
    [Option("output")]
    public string? OutputFile { get; init; }
    
    // Option needing default value: use set instead of init
    [Option("format")]
    public string Format { get; set; } = "json";
    
    // Value type option: no need to mark as nullable
    [Option("count")]
    public int Count { get; set; } = 1;
}
```

## Command Handling and Verbs

You can use the command handler pattern to handle different commands (verbs), similar to `git commit`, `git push`, etc. DotNetCampus.CommandLine provides several ways to add command handlers:

### 1. Using Delegates to Handle Commands

The simplest way is to handle commands through delegates, separating the command option type from the handling logic:

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<AddOptions>(options => { /* Handle add command */ })
    .AddHandler<RemoveOptions>(options => { /* Handle remove command */ })
    .Run();
```

Define command option classes with the `Verb` attribute to mark the verb:

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

For more complex command handling logic, you can create a class that implements the `ICommandHandler` interface, encapsulating both the command options and handling logic together:

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

### 3. Using Assembly Auto-discovery for Command Handlers

To manage a large number of commands more conveniently without having to manually add each one, you can use the assembly auto-discovery feature to automatically add all classes in the assembly that implement the `ICommandHandler` interface:

```csharp
// Define a partial class to mark for auto-discovery of command handlers
[CollectCommandHandlersFromThisAssembly]
internal partial class AssemblyCommandHandler;

// Add all command handlers at the program entry point
var commandLine = CommandLine.Parse(args);
commandLine.AddHandlers<AssemblyCommandHandler>()
    .Run();
```

Typically, handler classes need to have the `[Verb]` attribute and implement the `ICommandHandler` interface to be automatically discovered and added:

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

Additionally, you can create a command handler without the `[Verb]` attribute to serve as a default handler. There can be at most one command handler without the `[Verb]` attribute in the assembly, and it will be used when no other commands match:

```csharp
// Default handler without [Verb] attribute
internal class DefaultCommandHandler : ICommandHandler
{
    [Option('h', "help")]
    public bool ShowHelp { get; init; }

    public Task<int> RunAsync()
    {
        // Handle default command, such as displaying help information
        if (ShowHelp)
        {
            Console.WriteLine("Displaying help information...");
        }
        return Task.FromResult(0);
    }
}
```

This approach is particularly suitable for large applications or command line tools with strong extensibility, allowing you to add new commands without modifying the entry code.

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

1. The URL path part (e.g., `open/document.txt` in the example) is parsed as positional arguments or verb plus positional arguments
   - The first part of the path can serve as a verb (needs to be marked with the `[Verb]` attribute)
   - The subsequent path parts are parsed as positional arguments
2. Query parameters (the part after `?`) are parsed as named options
3. Collection type options can be passed by repeating parameter names, such as: `tags=csharp&tags=dotnet`
4. Special characters and non-ASCII characters in URLs are automatically URL decoded

## Naming Conventions and Best Practices

To ensure better compatibility and user experience, we recommend using kebab-case style for naming long options:

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
2. Resolves digit subordination issues (e.g., is "Version2Info" "Version2-Info" or "Version-2-Info")
3. Better compatibility with various command line styles

## Performance Data

The source code generator implementation has significantly improved command line parsing performance:

| Method                           |           Mean |        Error |       StdDev |   Gen0 |   Gen1 | Allocated |
| -------------------------------- | -------------: | -----------: | -----------: | -----: | -----: | --------: |
| 'parse  [] --flexible'           |       512.5 ns |      9.35 ns |      8.75 ns | 0.0792 |      - |    1328 B |
| 'parse  [] --gnu'                |       301.1 ns |      2.05 ns |      1.91 ns | 0.0434 |      - |     728 B |
| 'parse  [] --posix'              |       214.2 ns |      1.61 ns |      1.51 ns | 0.0291 |      - |     488 B |
| 'parse  [] --dotnet'             |       513.4 ns |      3.00 ns |      2.66 ns | 0.0792 |      - |    1328 B |
| 'parse  [] --powershell'         |       434.5 ns |      1.37 ns |      1.14 ns | 0.0648 |      - |    1088 B |
| 'parse  [PS1] --flexible'        |    10,478.6 ns |     86.91 ns |     81.29 ns | 0.4883 |      - |    8336 B |
| 'parse  [PS1] --powershell'      |     5,976.5 ns |     64.78 ns |     54.10 ns | 0.2594 |      - |    4440 B |
| 'parse  [CMD] --flexible'        |     6,098.2 ns |     35.36 ns |     33.08 ns | 0.2747 |      - |    4680 B |
| 'parse  [CMD] --powershell'      |     3,224.6 ns |     26.28 ns |     24.58 ns | 0.0954 |      - |    1624 B |
| 'parse  [GNU] --flexible'        |     6,550.1 ns |     64.40 ns |     60.24 ns | 0.2747 |      - |    4704 B |
| 'parse  [GNU] --gnu'             |     4,484.6 ns |     30.10 ns |     26.69 ns | 0.1373 |      - |    2416 B |
| 'handle [Edit,Print] --flexible' |     1,316.8 ns |      9.75 ns |      8.64 ns | 0.1373 |      - |    2304 B |
| 'parse  [URL]'                   |     4,795.2 ns |     38.33 ns |     33.98 ns | 0.5951 | 0.0076 |    9976 B |
| 'NuGet: CommandLineParser'       |   199,959.6 ns |  3,956.40 ns | 10,141.78 ns | 5.3711 |      - |   90696 B |
| 'NuGet: System.CommandLine'      | 1,728,238.4 ns | 13,403.14 ns | 11,881.54 ns | 3.9063 |      - |   84138 B |

Thanks to the use of source code generators, a single parsing operation takes only about 5000ns (approximately 0.005ms), which is significantly better than runtime reflection parsing methods.
