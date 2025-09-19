# 命令列解析

| [English][en] | [简体中文][zh-hans] | [繁體中文][zh-hant] |
| ------------- | ------------------- | ------------------- |

[en]: /docs/en/README.md
[zh-hans]: /docs/zh-hans/README.md
[zh-hant]: /docs/zh-hant/README.md

DotNetCampus.CommandLine 提供簡單且高效能的命令列解析功能。得益於原始碼產生器（以及攔截器），它現在提供更高效率的解析能力與更友善的開發體驗。所有功能均位於 `DotNetCampus.Cli` 命名空間下。

## 快速使用

```csharp
class Program
{
    static void Main(string[] args)
    {
        // 從命令列參數建立一個新的 CommandLine 執行個體
        var commandLine = CommandLine.Parse(args);

        // 將命令列解析為 Options 型別的執行個體
        // 原始碼產生器會自動處理解析過程，無需手動建立解析器
        var options = commandLine.As<Options>();

        // 接下來，使用 options 物件撰寫其他功能
    }
}
```

你需要定義一個包含命令列參數對應的型別：

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

然後在命令列中使用不同風格的命令填充這個型別的執行個體。程式庫支援多種命令列風格：

| 風格            | 範例                                                                                       |
| --------------- | ------------------------------------------------------------------------------------------ |
| DotNet          | `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`         |
| PowerShell      | `demo.exe 1.txt 2.txt 3.txt -c 20 -TestName BenchmarkTest -DetailLevel High -Debug`        |
| CMD             | `demo.exe 1.txt 2.txt 3.txt /c 20 /TestName BenchmarkTest /DetailLevel High /Debug`        |
| Gnu             | `demo.exe 1.txt 2.txt 3.txt -c 20 --test-name BenchmarkTest --detail-level High --debug`   |
| 彈性 (Flexible) | `demo.exe 1.txt 2.txt 3.txt --count:20 /TestName BenchmarkTest --detail-level=High -Debug` |

## 命令列風格

DotNetCampus.CommandLine 支援多種命令列風格，你可以在解析時指定使用哪種風格：

```csharp
// 使用 .NET CLI 風格解析命令列參數
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

支援的風格包含：

- `CommandLineStyle.Flexible`（預設）：彈性風格，於各種風格間提供最大相容性，預設大小寫不敏感
- `CommandLineStyle.DotNet`：.NET CLI 風格，預設大小寫敏感
- `CommandLineStyle.Gnu`：符合 GNU 規範，預設大小寫敏感
- `CommandLineStyle.Posix`：符合 POSIX 規範，預設大小寫敏感
- `CommandLineStyle.PowerShell`：PowerShell 風格，預設大小寫不敏感

預設情況下，這些風格的詳細差異如下：

| 風格           | Flexible     | DotNet       | Gnu          | Posix      | PowerShell  | URL               |
| -------------- | ------------ | ------------ | ------------ | ---------- | ----------- | ----------------- |
| 大小寫         | 不敏感       | 敏感         | 敏感         | 敏感       | 不敏感      | 不敏感            |
| 長選項         | 支援         | 支援         | 支援         | 不支援     | 支援        | 支援              |
| 短選項         | 支援         | 支援         | 支援         | 支援       | 支援        | 不支援            |
| 選項值 `=`     | -o=value     | -o=value     | -o=value     |            |             | option=value      |
| 選項值 `:`     | -o:value     | -o:value     |              |            |             |                   |
| 選項值 空白    | -o value     | -o value     | -o value     | -o value   | -o value    |                   |
| 布林選項       | -o           | -o           | -o           | -o         | -o          | option            |
| 布林選項帶值   | -o=true      | -o=true      |              |            | -o:true     | option=true       |
| 布林值         | true/false   | true/false   | true/false   | true/false | true/false  | true/false        |
| 布林值         | yes/no       | yes/no       | yes/no       | yes/no     | yes/no      | yes/no            |
| 布林值         | on/off       | on/off       | on/off       | on/off     | on/off      | on/off            |
| 布林值         | 1/0          | 1/0          | 1/0          | 1/0        | 1/0         | 1/0               |
| 集合選項       | -o A -o B    | -o A -o B    | -o A -o B    | -o A -o B  | -o A -o B   | option=A&option=B |
| 集合選項 `,`   | -o A,B,C     | -o A,B,C     | -o A,B,C     | -o A,B,C   | -o A,B,C    | -o A,B,C          |
| 集合選項 `;`   | -o A;B;C     | -o A;B;C     | -o A;B;C     | -o A;B;C   | -o A;B;C    | -o A;B;C          |
| 集合選項 空白  | -o A B C     | -o A B C     |              |            | -o A B C    |                   |
| 字典選項       | -o:A=X;B=Y   | -o:A=X;B=Y   |              |            | -o:A=X;B=Y  |                   |
| 多短布林合併   | 不支援       | 不支援       | -abc         | -abc       | 不支援      | 不支援            |
| 單短選項多字元 | -ab          | -ab          | 不支援       | 不支援     | -ab         | 不支援            |
| 短選項直接帶值 | 不支援       | 不支援       | -o1.txt      | 不支援     | 不支援      | 不支援            |
| 長選項前綴     | `--` `-` `/` | `--`         | `--`         | 不支援     | `-` `/`     |                   |
| 短選項前綴     | `-` `/`      | `-`          | `-`          | `-`        | `-` `/`     |                   |
| 命名法         | --kebab-case | --kebab-case | --kebab-case |            |             | kebab-case        |
| 命名法         | -PascalCase  |              |              |            | -PascalCase |                   |
| 命名法         | -camelCase   |              |              |            | -camelCase  |                   |
| 命名法         | /PascalCase  |              |              |            | /PascalCase |                   |
| 命名法         | /camelCase   |              |              |            | /camelCase  |                   |

## 命名法

1. 在程式碼中定義選項時，應使用 kebab-case 命名法
   - [為什麼要這麼做？](https://github.com/dotnet-campus/DotNetCampus.CommandLine/blob/main/docs/analyzers/DCL101.md)
   - 若推測你寫的不是 kebab-case，會提供警告 DCL101
   - 你可以忽略該警告；無論實際字串為何，都當作 kebab-case（提供無歧義的單詞邊界資訊，見下例）
2. 當你定義了被視為 kebab-case 的字串後
   - 依據設定的解析風格，可使用 kebab-case / PascalCase / camelCase 三種風格

範例：

```csharp
[Command("open command-line")]
public class Options
{
    [Option('o', "option-name")]
    public required string OptionName { get; init; }
}
```

此處有兩個 kebab-case：`Command` 特性與 `Option` 特性。可接受：

- DotNet/Gnu：`demo.exe open command-line --option-name value`
- PowerShell：`demo.exe Open CommandLine -OptionName value`
- CMD：`demo.exe Open CommandLine /optionName value`

若改寫為其他風格，可能出現與預期不同（或是刻意的）結果：

```csharp
#pragma warning disable DCL101
[Command("Open CommandLine")]
public class Options
{
    // 分析器警告：OptionName 不是 kebab-case，可視需要抑制 DCL101。
    [Option('o', "OptionName")]
    public required string OptionName { get; init; }
}
#pragma warning restore DCL101
```

因為仍視為 kebab-case，於是可接受：

- DotNet/Gnu：`demo.exe Open CommandLine --OptionName value`
- PowerShell：`demo.exe Open CommandLine -OptionName value`
- CMD：`demo.exe Open CommandLine /optionName value`

## 資料型別

程式庫支援多種資料型別：

1. **基本型別**：字串、整數、布林、列舉等
2. **集合型別**：陣列、List、唯讀集合、不可變集合
3. **字典型別**：`IDictionary`、`IReadOnlyDictionary`、`ImmutableDictionary` 等

如何透過命令列傳入，詳見前面的大型表格。

## 必需選項與預設值

定義屬性時，可用下列標記：

1. 使用 `required` 標記選項為必需
2. 使用 `init` 標記選項為唯讀（初始化後不可改）
3. 使用 `?` 標記選項可為 null

實際指派的值依下表行為：

| required | init | 集合屬性 | nullable | 行為       | 說明                            |
| -------- | ---- | -------- | -------- | ---------- | ------------------------------- |
| 1        | _    | _        | _        | 擲出例外   | 必須傳入，缺少則擲出例外        |
| 0        | 1    | 1        | _        | 空集合     | 集合永不為 null，缺少則給空集合 |
| 0        | 1    | 0        | 1        | null       | 可為 null，缺少則給 null        |
| 0        | 1    | 0        | 0        | 預設值     | 不可為 null，缺少則 default(T)  |
| 0        | 0    | _        | _        | 保留初始值 | 非必需/非立即，保留定義時初始值 |

- 1 = 標記過
- 0 = 未標記
- _ = 不論

1. 可空行為對參考與值型別一致（差別只是 default 對參考型別為 null）
2. 缺少必需選項會擲出 `RequiredPropertyNotAssignedException`
3. 「保留初始值」表示可直接在屬性定義時給初值：

```csharp
// 注意：只有未使用 required 與 init 時，初值才會生效。
[Option('o', "option-name")]
public string OptionName { get; set; } = "Default Value";
```

## 命令與子命令

可使用命令處理器模式處理不同命令，類似 `git commit`、`git remote add`。提供多種方式：

### 1. 使用委派處理

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<AddOptions>(options => { /* 處理 add */ })
    .AddHandler<RemoveOptions>(options => { /* 處理 remove */ })
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

### 2. `ICommandHandler` 介面

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
        // 命令處理邏輯
        Console.WriteLine($"Converting {InputFile} to {Format} format");
        // ...
        return Task.FromResult(0); // 結束代碼
    }
}
```

```csharp
var commandLine = CommandLine.Parse(args);
commandLine
    .AddHandler<ConvertCommandHandler>()
    .AddHandler<FooHandler>()
    .AddHandler<BarHandler>(options => { /* 處理 remove */ })
    .Run();
```

### 說明

1. `[Command]` 支援多個單字，表示子命令（例：`[Command("remote add")]`）。
2. 未標記 `[Command]`，或標記為 null / 空字串，表示預設命令（`[Command("")]`）。
3. 多個處理器匹配同一命令會擲出 `CommandNameAmbiguityException`。
4. 若有任何處理器為非同步，必須使用 `RunAsync`（否則編譯失敗）。

## URL 協議支援

可解析 URL 協議字串：

```ini
// schema://command/subcommand/positional-argument1/positional-argument2?option1=value1&option2=value2
```

開頭示例命令列可寫成：

```ini
# `demo.exe 1.txt 2.txt -c:20 --test-name:BenchmarkTest --detail-level=High --debug`
dotnet-campus://1.txt/2.txt?count=20&test-name=BenchmarkTest&detail-level=High&debug
```

特別說明：

1. 集合型別可重複參數名：`tags=csharp&tags=dotnet`
2. URL 中特殊與非 ASCII 字元會自動進行解碼

## 原始碼產生器、攔截器與效能

使用原始碼產生器與攔截器大幅提升效能。

### 使用者程式碼範例

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

對應產生的原始碼：

```csharp
// 在 AI 翻譯完成後，人類將補充它。
```

## 效能數據

解析空白命令列參數：

| Method                        |         Mean |      Error |     StdDev |   Gen0 | Allocated |
| ----------------------------- | -----------: | ---------: | ---------: | -----: | --------: |
| 'parse [] -v=4.1 -p=flexible' |     27.25 ns |   0.485 ns |   0.454 ns | 0.0143 |     240 B |
| 'parse [] -v=4.1 -p=dotnet'   |     27.35 ns |   0.471 ns |   0.440 ns | 0.0143 |     240 B |
| 'parse [] -v=4.0 -p=flexible' |     97.16 ns |   0.708 ns |   0.628 ns | 0.0134 |     224 B |
| 'parse [] -v=4.0 -p=dotnet'   |     95.90 ns |   0.889 ns |   0.742 ns | 0.0134 |     224 B |
| 'parse [] -v=3.x -p=parser'   |     49.73 ns |   0.931 ns |   0.870 ns | 0.0239 |     400 B |
| 'parse [] -v=3.x -p=runtime'  | 19,304.17 ns | 194.337 ns | 162.280 ns | 0.4272 |    7265 B |

解析 GNU 風格命令列參數：

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

其中：

1. `parse` 表示呼叫 `CommandLine.Parse`
2. `handle` 表示呼叫 `CommandLine.AddHandler`
3. 中括號 `[Xxx]` 表示傳入參數風格
4. `--flexible`、`--gnu` 等表示解析使用的風格（匹配效率最高）
5. `-v=3.x -p=parser` 為舊版手寫解析器效能（最佳但語法支援少）
6. `-v=3.x -p=runtime` 為舊版反射解析器效能
7. `-v=4.0` 與 `-v=4.1` 顯示版本效能演進
8. `NuGet: ...` 為其他程式庫效能
9. `parse [URL]`（本文省略部分）為解析 URL 協議效能

作者觀察（@walterlv）：

1. 最快的是 [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework)；本庫性能非常接近，同量級。
2. 感謝其對零依賴、零配置、零反射、零分配的極致追求，激勵我們完成目前版本（`-v4.1`）。
3. 它主打極致性能，犧牲部分語法支援；我們主打「全功能 + 高性能」，因此位於同級別，很難超越它。依你的受眾與需求選擇適用方案。
