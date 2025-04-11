# 命令行解析

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

dotnetCampus.CommandLine 提供了簡單而高性能的命令行解析功能，得益於源代碼生成器的加持，它現在提供了更高效的解析能力和更友好的開發體驗。所有功能都位於 dotnetCampus.Cli 命名空間下。

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
    public string FilePath { get; init; }

    [Option('s', "silence")]
    public bool IsSilence { get; init; }

    [Option('m', "mode")]
    public string StartMode { get; init; }

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

dotnetCampus.CommandLine 支持多種命令行風格，你可以在解析時指定使用哪種風格：

```csharp
// 使用 .NET CLI 風格解析命令行參數
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

支持的風格包括：

- `CommandLineStyle.Flexible`（默認）：智能識別多種風格
- `CommandLineStyle.GNU`：符合 GNU 規範的風格
- `CommandLineStyle.POSIX`：符合 POSIX 規範的風格
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

## 命令處理與謂詞

你可以使用命令處理器模式處理不同的命令（謂詞），類似於`git commit`、`git push`等：

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

dotnetCampus.CommandLine 支持解析 URL 協議字符串：

```
dotnetCampus://open/document.txt?readOnly=true&mode=Display&silence=true&startup-sessions=89EA9D26-6464-4E71-BD04-AA6516063D83
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
