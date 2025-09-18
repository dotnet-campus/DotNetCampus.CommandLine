# 命令行解析

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine 提供了简单而高性能的命令行解析功能，得益于源代码生成器的加持，它现在提供了更高效的解析能力和更友好的开发体验。所有功能都位于 DotNetCampus.Cli 命名空间下。

## 快速使用

```csharp
class Program
{
    static void Main(string[] args)
    {
        // 从命令行参数创建一个 CommandLine 类型的新实例
        var commandLine = CommandLine.Parse(args);

        // 将命令行解析为 Options 类型的实例
        // 源生成器会自动为你处理解析过程，无需手动创建解析器
        var options = commandLine.As<Options>();

        // 接下来，使用你的 options 对象编写其他的功能
    }
}
```

你需要定义一个包含命令行参数映射的类型：

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

然后在命令行中使用不同风格的命令填充这个类型的实例。库支持多种命令行风格：

### Windows PowerShell 风格

```powershell
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" -s -Mode Edit -StartupSessions A B C
```

### Windows CMD 风格

```cmd
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" /s /Mode Edit /StartupSessions A B C
```

### Linux/GNU 风格

```bash
$ demo.exe "C:/Users/lvyi/Desktop/demo.txt" -s --mode Edit --startup-sessions A --startup-sessions B --startup-sessions C
```

### .NET CLI 风格
```
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" -s:true --mode:Edit --startup-sessions:A;B;C
```

## 命令行风格

DotNetCampus.CommandLine 支持多种命令行风格，你可以在解析时指定使用哪种风格：

```csharp
// 使用 .NET CLI 风格解析命令行参数
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

支持的风格包括：

- `CommandLineStyle.Flexible`（默认）：智能识别多种风格，默认大小写不敏感，是 DotNet/GNU/PowerShell 风格的有效组合
  - 支持前面示例中所有风格的命令行参数，可正确解析
  - 完整支持 DotNet 风格的所有命令行功能（包括列表和字典）
  - 支持 GNU 风格中除短名称接参数（如 `-o1.txt`）和短名称缩写（如 `-abc` 表示 `-a -b -c`）外的所有功能
  - 由于 Posix 规则限制严格，Flexible 风格自然兼容 Posix 风格
  - DotNet 风格本身兼容 PowerShell 命令行风格，因此 Flexible 风格也支持 PowerShell 风格
- `CommandLineStyle.Gnu`：符合 GNU 规范的风格，默认大小写敏感
- `CommandLineStyle.Posix`：符合 POSIX 规范的风格，默认大小写敏感
- `CommandLineStyle.DotNet`：.NET CLI 风格，默认大小写不敏感
- `CommandLineStyle.PowerShell`：PowerShell 风格，默认大小写不敏感

## 数据类型支持

库支持多种数据类型的解析：

1. **基本类型**: 字符串、整数、布尔值、枚举等
2. **集合类型**: 数组、列表、只读集合、不可变集合
3. **字典类型**: IDictionary、IReadOnlyDictionary、ImmutableDictionary等

### 布尔类型选项

对于布尔类型的选项，在命令行中有多种指定方式：

- 仅指定选项名称，表示 `true`：`-s` 或 `--silence`
- 显式指定值：`-s:true`、`-s=false`、`--silence:on`、`--silence=off`

### 集合类型选项

对于集合类型的选项，可以通过多次指定同一选项，或使用分号分隔多个值：

```
demo.exe --files file1.txt --files file2.txt
demo.exe --files:file1.txt;file2.txt;file3.txt
```

### 字典类型选项

对于字典类型的选项，支持多种传入方式：

```
demo.exe --properties key1=value1 --properties key2=value2
demo.exe --properties:key1=value1;key2=value2
```

## 位置参数

除了命名选项外，你还可以使用位置参数，通过 `ValueAttribute` 指定参数的位置：

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

使用方式：

```
demo.exe input.txt output.txt --verbose
```

你也可以捕获多个位置参数到一个数组或集合中：

```csharp
class MultiFileOptions
{
    [Value(0, Length = int.MaxValue)]
    public string[] Files { get; init; } = [];
}
```

## 组合使用选项和位置参数

`ValueAttribute` 和 `OptionAttribute` 可以同时应用于同一个属性：

```csharp
class Options
{
    [Value(0), Option('f', "file")]
    public string FilePath { get; init; }
}
```

这样，以下命令行都会将文件路径赋值给 `FilePath` 属性：

```
demo.exe file.txt
demo.exe -f file.txt
demo.exe --file file.txt
```

## 必需选项与可选选项

在C# 11及以上版本中，可以使用`required`修饰符标记必需的选项：

```csharp
class Options
{
    [Option('i', "input")]
    public required string InputFile { get; init; }  // 必需选项
    
    [Option('o', "output")]
    public string? OutputFile { get; init; }         // 可选选项
}
```

如果未提供必需选项，解析时会抛出`RequiredPropertyNotAssignedException`异常。

## 属性初始值与访问器修饰符

在定义选项类型时，需要注意属性初始值与访问器修饰符（`init`、`required`）之间的关系：

```csharp
class Options
{
    // 错误示例：当使用 init 或 required 时，默认值将被忽略
    [Option('f', "format")]
    public string Format { get; init; } = "json";  // 默认值不会生效！
    
    // 正确示例：使用 set 以保留默认值
    [Option('f', "format")]
    public string Format { get; set; } = "json";  // 默认值会正确保留
}
```

### 关于属性初始值的重要说明

1. **使用 `init` 或 `required` 时的行为**：
   - 当属性包含 `required` 或 `init` 修饰符时，属性的初始值会被忽略
   - 如果命令行参数中未提供该选项的值，属性将被设置为 `default(T)`（对于引用类型为 `null`）
   - 这是由 C# 语言特性决定的，命令行库如果希望突破此限制需要针对所有属性排列组合进行处理，显然是非常浪费的

2. **保留默认值的方式**：
   - 如果需要为属性提供默认值，应使用 `{ get; set; }` 而非 `{ get; init; }`

3. **可空类型与警告处理**：
   - 对于非必需的引用类型属性，应将其标记为可空（如 `string?`）以避免可空警告
   - 对于值类型（如 `int`、`bool`），如果想保留默认值而非 `null`，不应将其标记为可空

示例：

```csharp
class OptionsBestPractice
{
    // 必需选项：使用 required，无需担心默认值
    [Option("input")]
    public required string InputFile { get; init; }
    
    // 可选选项：标记为可空类型以避免警告
    [Option("output")]
    public string? OutputFile { get; init; }
    
    // 需要默认值的选项：使用 set 而非 init
    [Option("format")]
    public string Format { get; set; } = "json";
    
    // 值类型选项：不需要标记为可空
    [Option("count")]
    public int Count { get; set; } = 1;
}
```

## 命令处理与命令

你可以使用命令处理器模式处理不同的命令，类似于`git commit`、`git push`等。DotNetCampus.CommandLine 提供了多种添加命令处理器的方式：

### 1. 使用委托处理命令

最简单的方式是通过委托处理命令，将命令选项类型和处理逻辑分离：

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<AddOptions>(options => { /* 处理add命令 */ })
    .AddHandler<RemoveOptions>(options => { /* 处理remove命令 */ })
    .Run();
```

定义命令选项类时使用`Command`特性标记命令：

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

### 2. 使用 ICommandHandler 接口

对于更复杂的命令处理逻辑，你可以创建实现 `ICommandHandler` 接口的类，将命令选项和处理逻辑封装在一起：

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
        // 实现命令处理逻辑
        Console.WriteLine($"Converting {InputFile} to {Format} format");
        // ...
        return Task.FromResult(0); // 返回退出代码
    }
}
```

然后直接添加到命令行解析器中：

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<ConvertCommandHandler>()
    .Run();
```

### 3. 使用程序集自动发现命令处理器

为了更方便地管理大量命令且无需手动逐个添加，可以使用程序集自动发现功能，自动添加程序集中所有实现了 `ICommandHandler` 接口的类：

```csharp
// 定义一个部分类用于标记自动发现命令处理器
[CollectCommandHandlersFromThisAssembly]
internal partial class AssemblyCommandHandler;

// 在程序入口添加所有命令处理器
var commandLine = CommandLine.Parse(args);
commandLine.AddHandlers<AssemblyCommandHandler>()
    .Run();
```

通常，处理器类需要添加 `[Command]` 特性并实现 `ICommandHandler` 接口，它就会被自动发现和添加：

```csharp
[Command("sample")]
internal class SampleCommandHandler : ICommandHandler
{
    [Option("SampleProperty")]
    public required string Option { get; init; }

    [Value(Length = int.MaxValue)]
    public string? Argument { get; init; }

    public Task<int> RunAsync()
    {
        // 实现命令处理逻辑
        return Task.FromResult(0);
    }
}
```

此外，你也可以创建一个没有 `[Command]` 特性的命令处理器作为默认处理器。在程序集中最多只能有一个没有 `[Command]` 特性的命令处理器，它将在没有其他命令匹配时被使用：

```csharp
// 没有 [Command] 特性的默认处理器
internal class DefaultCommandHandler : ICommandHandler
{
    [Option('h', "help")]
    public bool ShowHelp { get; init; }

    public Task<int> RunAsync()
    {
        // 处理默认命令，如显示帮助信息等
        if (ShowHelp)
        {
            Console.WriteLine("显示帮助信息...");
        }
        return Task.FromResult(0);
    }
}
```

这种方式特别适合大型应用或扩展性强的命令行工具，可以在不修改入口代码的情况下添加新命令。

### 异步命令处理

对于需要异步执行的命令处理，可以使用`RunAsync`方法：

```csharp
await commandLine.AddHandler<ImportOptions>(async options => 
{
    await ImportDataAsync(options);
    return 0;
})
.RunAsync();
```

## URL协议支持

DotNetCampus.CommandLine 支持解析 URL 协议字符串：

```
dotnet-campus://open/document.txt?readOnly=true&mode=Display&silence=true&startup-sessions=89EA9D26-6464-4E71-BD04-AA6516063D83
```

URL协议解析的特点和用法：

1. URL路径部分（如示例中的 `open/document.txt`）会被解析为位置参数或命令加位置参数
   - 路径的第一部分可作为命令（需标记 `[Command]` 特性）
   - 随后的路径部分会被解析为位置参数
2. 查询参数（`?` 后的部分）会被解析为命名选项
3. 集合类型选项可通过重复参数名传入多个值，如：`tags=csharp&tags=dotnet`
4. URL中的特殊字符和非ASCII字符会自动进行URL解码

## 命名约定与最佳实践

为确保更好的兼容性和用户体验，我们建议使用 kebab-case 风格命名长选项：

```csharp
// 推荐
[Option('o', "output-file")]
public string OutputFile { get; init; }

// 不推荐
[Option('o', "OutputFile")]
public string OutputFile { get; init; }
```

使用kebab-case命名的好处：

1. 提供更清晰的单词分割信息（如能猜出"DotNet-Campus"而不是"Dot-Net-Campus"）
2. 解决数字从属问题（如"Version2Info"是"Version2-Info"还是"Version-2-Info"）
3. 与多种命令行风格更好地兼容

## 源生成器、拦截器与性能优化

DotNetCampus.CommandLine 使用源代码生成器技术大幅提升了命令行解析的性能。其中的拦截器（[Interceptor](https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md)）让性能提升发挥得更淋漓尽致。

### 拦截器的工作原理

当你调用 `CommandLine.As<T>()` 或 `CommandLine.AddHandler<T>()` 等方法时，源生成器会自动生成拦截代码，将调用重定向到编译时生成的高性能代码路径。这使得命令行参数解析和对象创建的性能得到了大幅提升。

例如，当你编写以下代码时：

```csharp
var options = CommandLine.Parse(args).As<Options>();
```

源生成器会拦截这个调用，自动生成类似以下的代码来替代默认通过字典查找创建器的方式实现（旧版本曾使用过反射）：

```csharp
/// <summary>
/// <see cref="global::DotNetCampus.Cli.CommandLine.As{Options}()"/> 方法的拦截器。拦截以提高性能。
/// </summary>
[global::System.Runtime.CompilerServices.InterceptsLocation(1, /* Program.Run4xInterceptor @Program.cs */ "G4GJAK7udHFnPkRUqV6VzzgRAABQcm9ncmFtLmNz")]
public static T CommandLine_As_DotNetCampusCliTestsFakesOptions<T>(this global::DotNetCampus.Cli.CommandLine commandLine)
    where T : global::DotNetCampus.Cli.Tests.Fakes.Options
{
    return (T)global::DotNetCampus.Cli.Tests.Fakes.OptionsBuilder.CreateInstance(commandLine);
}
```

### 源生成器生成的代码示例

下面是一个简单的命令行选项类型及其对应生成的源代码示例：

```csharp
// 用户代码中的类型
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

对应生成的源：

```csharp
#nullable enable
namespace DotNetCampus.Cli.Tests;

/// <summary>
/// 辅助 <see cref="global::DotNetCampus.Cli.Tests.DotNet03_MixedOptions"/> 生成命令行选项、命令或处理函数的创建。
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

代码中的方法调用：

```csharp
_ = CommandLine.Parse(args, CommandLineParsingOptions.DotNet).As<Options>();
```

对应生成的源（拦截器）：

```csharp
#nullable enable

namespace DotNetCampus.Cli.Compiler
{
    file static class Interceptors
    {
        /// <summary>
        /// <see cref="global::DotNetCampus.Cli.CommandLine.As{Options}()"/> 方法的拦截器。拦截以提高性能。
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

代码中的程序集命令处理器搜集：

```csharp
[CollectCommandHandlersFromThisAssembly]
internal partial class AssemblyCommandHandler;
```

对应生成的源：

```csharp
#nullable enable
namespace DotNetCampus.Cli.Tests.Fakes;

/// <summary>
/// 提供一种辅助自动搜集并执行本程序集中所有命令行处理器的方式。
/// </summary>
partial class AssemblyCommandHandler : global::DotNetCampus.Cli.Compiler.ICommandHandlerCollection
{
    public global::DotNetCampus.Cli.ICommandHandler? TryMatch(string? command, global::DotNetCampus.Cli.CommandLine cl) => command switch
    {
        null => throw new global::DotNetCampus.Cli.Exceptions.CommandVerbAmbiguityException($"Multiple command handlers match the same command name 'null': AmbiguousOptions, CollectionOptions, ComparedOptions, DefaultCommandHandler, DictionaryOptions, FakeCommandOptions, Options, PrimaryOptions, UnlimitedValueOptions, ValueOptions.", null),
        // 类型 EditOptions 没有继承 ICommandHandler 接口，因此无法统一调度执行，只能由开发者单独调用。
        "Fake" => (global::DotNetCampus.Cli.ICommandHandler)global::DotNetCampus.Cli.Tests.Fakes.FakeCommandHandlerBuilder.CreateInstance(cl),
        // 类型 PrintOptions 没有继承 ICommandHandler 接口，因此无法统一调度执行，只能由开发者单独调用。
        // 类型 ShareOptions 没有继承 ICommandHandler 接口，因此无法统一调度执行，只能由开发者单独调用。
        _ => null,
    };
}
```

## 性能数据

源代码生成器实现提供了极高的命令行解析性能：

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

其中：
1. `parse` 表示调用的是 `CommandLine.Parse` 方法
2. `handle` 表示调用的是 `CommandLine.AddHandler` 方法
3. 中括号 `[Xxx]` 表示传入的命令行参数的风格
4. `--flexible` `--gnu` 等表示解析传入命令行时所使用的解析器风格（相匹配时效率最高）
5. `-v=3.x -p=parser` 表示旧版本手工编写解析器并传入时的性能（性能最好，不过旧版本支持的命令行规范较少，很多合法的命令写法并不支持）
6. `-v=3.x -p=runtime` 表示旧版本使用默认的反射解析器时的性能
7. `NuGet: CommandLineParser` 和 `NuGet: System.CommandLine` 表示使用对应名称的 NuGet 包解析命令行参数时的性能
8. `parse [URL]` 表示解析 URL 协议字符串时的性能

新版本得益于源生成器和拦截器：
1. 完成一次解析大约在 0.8μs（微秒）左右（Benchmark）
2. 在应用程序启动期间，完成一次解析只需要大约 34μs
3. 在应用程序启动期间，包含dll加载、类型初始化在内的解析一次大约8ms（使用 AOT 编译能重新降至 34μs）。
