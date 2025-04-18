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
$ demo.exe "C:/Users/lvyi/Desktop/demo.txt" -s --mode Edit --startup-sessions A B C
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

- `CommandLineStyle.Flexible`（默认）：智能识别多种风格
- `CommandLineStyle.GNU`：符合 GNU 规范的风格
- `CommandLineStyle.POSIX`：符合 POSIX 规范的风格
- `CommandLineStyle.DotNet`：.NET CLI 风格
- `CommandLineStyle.PowerShell`：PowerShell 风格

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

## 命令处理与谓词

你可以使用命令处理器模式处理不同的命令（谓词），类似于`git commit`、`git push`等。DotNetCampus.CommandLine 提供了多种添加命令处理器的方式：

### 1. 使用委托处理命令

最简单的方式是通过委托处理命令，将命令选项类型和处理逻辑分离：

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<AddOptions>(options => { /* 处理add命令 */ })
    .AddHandler<RemoveOptions>(options => { /* 处理remove命令 */ })
    .Run();
```

定义命令选项类时使用`Verb`特性标记谓词：

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

### 2. 使用 ICommandHandler 接口

对于更复杂的命令处理逻辑，你可以创建实现 `ICommandHandler` 接口的类，将命令选项和处理逻辑封装在一起：

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

通常，处理器类需要添加 `[Verb]` 特性并实现 `ICommandHandler` 接口，它就会被自动发现和添加：

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
        // 实现命令处理逻辑
        return Task.FromResult(0);
    }
}
```

此外，你也可以创建一个没有 `[Verb]` 特性的命令处理器作为默认处理器。在程序集中最多只能有一个没有 `[Verb]` 特性的命令处理器，它将在没有其他命令匹配时被使用：

```csharp
// 没有 [Verb] 特性的默认处理器
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

1. URL路径部分（如示例中的 `open/document.txt`）会被解析为位置参数或谓词加位置参数
   - 路径的第一部分可作为谓词（需标记 `[Verb]` 特性）
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

## 性能数据

源代码生成器实现使得命令行解析的性能得到大幅提升：

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

得益于源代码生成器的使用，完成一次解析只需要约 5000ns（约 0.005ms），大幅优于运行时反射解析方式。
