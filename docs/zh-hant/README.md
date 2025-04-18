# 命令行解析

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine 提供了簡單而高性能的命令行解析功能，得益於源代碼生成器的加持，它現在提供了更高效的解析能力和更友好的開發體驗。所有功能都位於 DotNetCampus.Cli 命名空間下。

## 快速使用

```csharp
class Program
{
    static void Main(string[] args)
    {
        // 從命令行參數創建一個 CommandLine 類型的新實例
        var commandLine = CommandLine.Parse(args);

        // 將命令行解析為 Options 類型的實例
        // 源生成器會自動為你處理解析過程，無需手動創建解析器
        var options = commandLine.As<Options>();

        // 接下來，使用你的 options 對象編寫其他的功能
    }
}
```

你需要定義一個包含命令行參數映射的類型：

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

然後在命令行中使用不同風格的命令填充這個類型的實例。庫支持多種命令行風格：

### Windows PowerShell 風格

```powershell
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" -s -Mode Edit -StartupSessions A B C
```

### Windows CMD 風格

```cmd
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" /s /Mode Edit /StartupSessions A B C
```

### Linux/GNU 風格

```bash
$ demo.exe "C:/Users/lvyi/Desktop/demo.txt" -s --mode Edit --startup-sessions A B C
```

### .NET CLI 風格
```
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" -s:true --mode:Edit --startup-sessions:A;B;C
```

## 命令行風格

DotNetCampus.CommandLine 支持多種命令行風格，你可以在解析時指定使用哪種風格：

```csharp
// 使用 .NET CLI 風格解析命令行參數
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

支持的風格包括：

- `CommandLineStyle.Flexible`（默認）：智能識別多種風格
- `CommandLineStyle.Gnu`：符合 GNU 規範的風格
- `CommandLineStyle.Posix`：符合 POSIX 規範的風格
- `CommandLineStyle.DotNet`：.NET CLI 風格
- `CommandLineStyle.PowerShell`：PowerShell 風格

## 數據類型支持

庫支持多種數據類型的解析：

1. **基本類型**: 字符串、整數、布爾值、枚舉等
2. **集合類型**: 數組、列表、只讀集合、不可變集合
3. **字典類型**: IDictionary、IReadOnlyDictionary、ImmutableDictionary等

### 布爾類型選項

對於布爾類型的選項，在命令行中有多種指定方式：

- 僅指定選項名稱，表示 `true`：`-s` 或 `--silence`
- 顯式指定值：`-s:true`、`-s=false`、`--silence:on`、`--silence=off`

### 集合類型選項

對於集合類型的選項，可以通過多次指定同一選項，或使用分號分隔多個值：

```
demo.exe --files file1.txt --files file2.txt
demo.exe --files:file1.txt;file2.txt;file3.txt
```

### 字典類型選項

對於字典類型的選項，支持多種傳入方式：

```
demo.exe --properties key1=value1 --properties key2=value2
demo.exe --properties:key1=value1;key2=value2
```

## 位置參數

除了命名選項外，你還可以使用位置參數，通過 `ValueAttribute` 指定參數的位置：

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

你也可以捕獲多個位置參數到一個數組或集合中：

```csharp
class MultiFileOptions
{
    [Value(0, Length = int.MaxValue)]
    public string[] Files { get; init; } = [];
}
```

## 組合使用選項和位置參數

`ValueAttribute` 和 `OptionAttribute` 可以同時應用於同一個屬性：

```csharp
class Options
{
    [Value(0), Option('f', "file")]
    public string FilePath { get; init; }
}
```

這樣，以下命令行都會將文件路徑賦值給 `FilePath` 屬性：

```
demo.exe file.txt
demo.exe -f file.txt
demo.exe --file file.txt
```

## 必需選項與可選選項

在C# 11及以上版本中，可以使用`required`修飾符標記必需的選項：

```csharp
class Options
{
    [Option('i', "input")]
    public required string InputFile { get; init; }  // 必需選項
    
    [Option('o', "output")]
    public string? OutputFile { get; init; }         // 可選選項
}
```

如果未提供必需選項，解析時會拋出`RequiredPropertyNotAssignedException`異常。

## 屬性初始值與訪問器修飾符

在定義選項類型時，需要注意屬性初始值與訪問器修飾符（`init`、`required`）之間的關係：

```csharp
class Options
{
    // 錯誤示例：當使用 init 或 required 時，默認值將被忽略
    [Option('f', "format")]
    public string Format { get; init; } = "json";  // 默認值不會生效！
    
    // 正確示例：使用 set 以保留默認值
    [Option('f', "format")]
    public string Format { get; set; } = "json";  // 默認值會正確保留
}
```

### 關於屬性初始值的重要說明

1. **使用 `init` 或 `required` 時的行為**：
   - 當屬性包含 `required` 或 `init` 修飾符時，屬性的初始值會被忽略
   - 如果命令行參數中未提供該選項的值，屬性將被設置為 `default(T)`（對於引用類型為 `null`）
   - 這是由 C# 語言特性決定的，命令行庫如果希望突破此限制需要針對所有屬性排列組合進行處理，顯然是非常浪費的

2. **保留默認值的方式**：
   - 如果需要為屬性提供默認值，應使用 `{ get; set; }` 而非 `{ get; init; }`

3. **可空類型與警告處理**：
   - 對於非必需的引用類型屬性，應將其標記為可空（如 `string?`）以避免可空警告
   - 對於值類型（如 `int`、`bool`），如果想保留默認值而非 `null`，不應將其標記為可空

示例：

```csharp
class OptionsBestPractice
{
    // 必需選項：使用 required，無需擔心默認值
    [Option("input")]
    public required string InputFile { get; init; }
    
    // 可選選項：標記為可空類型以避免警告
    [Option("output")]
    public string? OutputFile { get; init; }
    
    // 需要默認值的選項：使用 set 而非 init
    [Option("format")]
    public string Format { get; set; } = "json";
    
    // 值類型選項：不需要標記為可空
    [Option("count")]
    public int Count { get; set; } = 1;
}
```

## 命令處理與謂詞

你可以使用命令處理器模式處理不同的命令（謂詞），類似於`git commit`、`git push`等。DotNetCampus.CommandLine 提供了多種添加命令處理器的方式：

### 1. 使用委託處理命令

最簡單的方式是通過委託處理命令，將命令選項類型和處理邏輯分離：

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<AddOptions>(options => { /* 處理add命令 */ })
    .AddHandler<RemoveOptions>(options => { /* 處理remove命令 */ })
    .Run();
```

定義命令選項類時使用`Verb`特性標記謂詞：

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

對於更複雜的命令處理邏輯，你可以創建實現 `ICommandHandler` 接口的類，將命令選項和處理邏輯封裝在一起：

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
        // 實現命令處理邏輯
        Console.WriteLine($"Converting {InputFile} to {Format} format");
        // ...
        return Task.FromResult(0); // 返回退出碼
    }
}
```

然後直接添加到命令行解析器中：

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<ConvertCommandHandler>()
    .Run();
```

### 3. 使用程序集自動發現命令處理器

為了更方便地管理大量命令且無需手動逐個添加，可以使用程序集自動發現功能，自動添加程序集中所有實現了 `ICommandHandler` 接口的類：

```csharp
// 定義一個部分類用於標記自動發現命令處理器
[CollectCommandHandlersFromThisAssembly]
internal partial class AssemblyCommandHandler;

// 在程序入口添加所有命令處理器
var commandLine = CommandLine.Parse(args);
commandLine.AddHandlers<AssemblyCommandHandler>()
    .Run();
```

通常，處理器類需要添加 `[Verb]` 特性並實現 `ICommandHandler` 接口，它就會被自動發現和添加：

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
        // 實現命令處理邏輯
        return Task.FromResult(0);
    }
}
```

此外，你也可以創建一個沒有 `[Verb]` 特性的命令處理器作為默認處理器。在程序集中最多只能有一個沒有 `[Verb]` 特性的命令處理器，它將在沒有其他命令匹配時被使用：

```csharp
// 沒有 [Verb] 特性的默認處理器
internal class DefaultCommandHandler : ICommandHandler
{
    [Option('h', "help")]
    public bool ShowHelp { get; init; }

    public Task<int> RunAsync()
    {
        // 處理默認命令，如顯示幫助信息等
        if (ShowHelp)
        {
            Console.WriteLine("顯示幫助信息...");
        }
        return Task.FromResult(0);
    }
}
```

這種方式特別適合大型應用或擴展性強的命令行工具，可以在不修改入口代碼的情況下添加新命令。

### 異步命令處理

對於需要異步執行的命令處理，可以使用`RunAsync`方法：

```csharp
await commandLine.AddHandler<ImportOptions>(async options => 
{
    await ImportDataAsync(options);
    return 0;
})
.RunAsync();
```

## URL協議支持

DotNetCampus.CommandLine 支持解析 URL 協議字符串：

```
dotnet-campus://open/document.txt?readOnly=true&mode=Display&silence=true&startup-sessions=89EA9D26-6464-4E71-BD04-AA6516063D83
```

URL協議解析的特點和用法：

1. URL路徑部分（如示例中的 `open/document.txt`）會被解析為位置參數或謂詞加位置參數
   - 路徑的第一部分可作為謂詞（需標記 `[Verb]` 特性）
   - 隨後的路徑部分會被解析為位置參數
2. 查詢參數（`?` 後的部分）會被解析為命名選項
3. 集合類型選項可通過重複參數名傳入多個值，如：`tags=csharp&tags=dotnet`
4. URL中的特殊字符和非ASCII字符會自動進行URL解碼

## 命名約定與最佳實踐

為確保更好的兼容性和用戶體驗，我們建議使用 kebab-case 風格命名長選項：

```csharp
// 推薦
[Option('o', "output-file")]
public string OutputFile { get; init; }

// 不推薦
[Option('o', "OutputFile")]
public string OutputFile { get; init; }
```

使用kebab-case命名的好處：

1. 提供更清晰的單詞分割信息（如能猜出"DotNet-Campus"而不是"Dot-Net-Campus"）
2. 解決數字從屬問題（如"Version2Info"是"Version2-Info"還是"Version-2-Info"）
3. 與多種命令行風格更好地兼容

## 性能數據

源代碼生成器實現使得命令行解析的性能得到大幅提升：

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

得益於源代碼生成器的使用，完成一次解析只需要約 5000ns（約 0.005ms），大幅優於運行時反射解析方式。
