# 命令行解析

[English][en]|[简体中文][zh-chs]|[繁體中文][zh-cht]
-|-|-

[en]: /README.md
[zh-chs]: /docs/zh-chs/README.md
[zh-cht]: /docs/zh-cht/README.md

dotnetCampus.CommandLine 中提供了簡單而高性能的命令行解析功能，在 dotnetCampus.Cli 命名空間下。

## 快速使用

```csharp
class Program
{
    static void Main(string[] args)
    {
        // 從命令行參數創建一個 CommandLine 類型的新實例。
        var commandLine = CommandLine.Parse(args);

        // 將 CommandLine 類型的當作一個命令行參數類型選項來使用。
        var options = commandLine.As<Options>(new OptionsParser());

        // 接下來，使用你的選項對象編寫其他的功能。
    }
}
```

而這個 `Options` 類型的定義如下：

```csharp
class Options
{
    [Value(0)]
    public string FilePath { get; }

    [Option('s', "Silence")]
    public bool IsSilence { get; }

    [Option('m', "Mode")]
    public string StartMode { get; }

    [Option("StartupSessions")]
    public IReadonlyList<string> StartupSessions { get; }

    public Options(
        string filePath,
        bool isSilence,
        string startMode,
        IReadonlyList<string> startupSessions)
    {
        FilePath = filePath;
        IsSilence = isSilence;
        StartMode = startMode;
        StartupSessions = startupSessions;
    }
}
```

於是，在命令行中可以使用不同風格的命令填充這個類型的實例。

Windows 風格：

```powershell
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" -s -Mode Edit -StartupSessions A B C
```

```cmd
> demo.exe "C:\Users\lvyi\Desktop\demo.txt" /s /Mode Edit /StartupSessions A B C
```

Linux 風格：

```bash
$ demo.exe "C:/Users/lvyi/Desktop/demo.txt" -s --mode Edit --startup-sessions A B C
```

以上不同風格不能混合使用。

對於 `bool` 類型的屬性，在命令行中既可以在選項後傳入 `true` /`True` /`false` /`False` 也可以不傳。如果不傳，則表示 `true`。

另外，`ValueAttribute` 和 `OptionAttribute` 可以出現在同一個屬性上。這時如果發現了不帶選項的值，將填充到 `ValueAttribute` 的屬性上;而一旦之後發現了此 `OptionsAttribute` 指定的短名稱或者長名稱，會將新的值覆蓋再次設置到此屬性上。

```csharp
[Value(0), Option('f', "File")]
public string FilePath { get; }
```

## 需要注意

在命令行中輸入參數時，無論哪種風格，命令行都是區分大小寫的。對於選項（`-`，`/` 或者 `--` 開頭）如果大小寫錯誤，此選項和後面附帶的值都將被忽略;對於值（不帶 `-` 或者 `/` 開頭），值將按照命令行中的原生大小寫傳入 `Options` 類型的實例中。

在 `Options` 類型中定義屬性時，短名稱是可選指定的，但一旦指定則必須是一個字符;長名稱是必須指定的，而且命名必須滿足 PascalCase 命名規則且不帶連字符。詳細要求可在編寫自己的 `Options` 類型時閱讀 `OptionAttribute` 的註釋。

## 多種命令行參數與謂詞

你可以為你的命令行參數指定謂詞，每一種謂詞都可以有自己的一組獨特的參數類型。

```csharp
var commandLine = CommandLine.Parse(args);
commandLine.AddHandler<EditOptions>(options => 0)
    .AddHandler<PrintOptions>(options => 0).Run();
```

而 `EditOptions` 和 `PrintOptions` 的定義如下，區別在於類型標記了謂詞。

```csharp
[Verb("Edit")]
public class EditOptions
{
    [Value(0), Option('f', "File")] public string FilePath { get; set; }
}

[Verb("Print")]
public class PrintOptions
{
    [Value(0), Option('f', "File")] public string FilePath { get; set; }
    [Option('p', "Printer")] public string Printer { get; set; }
}
```

你也可以在 `Handle` 中使用不標謂詞的參數類型，但是這樣的參數最多只允許有一個，會作為沒有任何謂詞匹配上時使用的默認參數類型。

另外，`Handle` 方法有對應的 `HandleAsync` 異步版本，用於處理異步的任務。

## 關於解析器

在前面的示例程序中，我們傳入了一個解析器的實例 `new OptionsParser()`。如果你不在乎性能（實際上也花不了多少性能），那麼不必傳入，命令行解析器可以針對你的選項類型自動生成一個運行時解析器。但是如果你在乎性能，那麼你可能需要自己編寫（將來會自動生成）。本文文末附有各種不同用法的性能數據。

如果你打算自己編寫了，那麼繼續閱讀這一小節。

這個類的編寫是標準化的，請按照註釋以及下文的規範來操作，不要編寫額外的任何校驗代碼或類型轉換代碼因為傳入解析器之前所有參數已全部校驗完畢;而且將來接入自動生成解析器後，你編寫的個性化邏輯可能丟失。

解析器的示例代碼有兩種，一種使用原生接口，性能更好;另一種使用基類，但編寫所需的代碼更少建議手寫的話使用基類，如果能生成代碼，則使用接口。

```csharp
public class OptionsParser : ICommandLineOptionParser<Options>
{
    private string _filePath;
    private bool _isSilence;
    private string _startMode;
    private IReadonlyList<string> _startupSessions;

    public void SetValue(int index, string value)
    {
        switch (index)
        {
            case 0:
                _filePath = value;
                break;
        }
    }

    public void SetValue(char shortName, bool value)
    {
        switch (shortName)
        {
            case 's':
                _isSilence = value;
                break;
        }
    }

    public void SetValue(char shortName, string value)
    {
        switch (shortName)
        {
            case 'm':
                _startMode = value;
                break;
        }
    }

    public void SetValue(char shortName, IReadOnlyList<string> values)
    {
    }

    public void SetValue(string longName, bool value)
    {
        switch (longName)
        {
            case "Silence":
                _isSilence = value;
                break;
        }
    }

    public void SetValue(string longName, string value)
    {
        switch (longName)
        {
            case "Mode":
                _startMode = value;
                break;
        }
    }

    public void SetValue(string longName, IReadOnlyList<string> values)
    {
        switch (longName)
        {
            case "StartupSession":
                _startupSession = value;
                break;
        }
    }

    public Options Commit()
    {
        return new Options(_filePath, _isSilence, _startMode, _startupSession);
    }
}
```

```csharp
public class SelfWrittenShareOptionsParser : CommandLineOptionParser<Options>
{
    public SelfWrittenShareOptionsParser()
    {
        string filePath = null;
        bool isSilence = false;
        string startMode = null;
        IReadonlyList<string> startupSessions = null;

        AddMatch(0, value => filePath = value);
        AddMatch('s', value => isSilence = value);
        AddMatch("Silence", value => isSilence = value);
        AddMatch('m', value => startMode = value);
        AddMatch("Mode", value => startMode = value);
        AddMatch("StartupSessions", value => startupSessions = value);

        SetResult(() => new Options(filePath, isSilence, startMode, startupSession));
    }
}
```

## 支持協議解析

`CommandLine` 支持解析URL協議中的字符串。

```
@"dotnetCampus://open/?file=C:\Users\lvyi\Desktop\%E9%87%8D%E5%91%BD%E5%90%8D%E8%AF%95%E9%AA%8C.enbx&mode=Display&silence=true&startupSessions=89EA9D26-6464-4E71-BD04-AA6516063D83"
```

請注意，解析URL有如下限制：

1.你的 `Options` 類型中所有的 `ValueAttribute` 都將無法被賦值;
1.你的 `Options` 類型中不允許出現標記有 `OptionAttribute` 的字符串集合屬性。

另外，URL 解析中的選項名稱也是大小寫敏感的。當你在 `Options` 類型中正確使用PascalCase 風格定義了選項的長名稱後，你在 URL 中既可以使用 PascalCase 風格也可以使用 camelCase 風格。

## 性能數據


|                       Method |          Mean |        Error |       StdDev |  Ratio | RatioSD |
|----------------------------- |--------------:|-------------:|-------------:|-------:|--------:|
|                  ParseNoArgs |      95.20 ns |     1.828 ns |     1.956 ns |   0.09 |    0.00 |
|              ParseNoArgsAuto |     763.12 ns |    14.702 ns |    19.117 ns |   0.69 |    0.02 |
|                 ParseWindows |   1,116.76 ns |    24.612 ns |    23.022 ns |   1.00 |    0.00 |
|             ParseWindowsAuto |   1,974.78 ns |    37.120 ns |    44.189 ns |   1.76 |    0.06 |
|          ParseWindowsRuntime |  96,378.30 ns | 1,900.205 ns | 2,725.217 ns |  86.40 |    2.99 |
| ParseWindowsImmutableRuntime |  96,200.14 ns | 1,677.293 ns | 1,568.941 ns |  86.17 |    2.15 |
|                  HandleVerbs |   1,530.32 ns |    33.916 ns |    31.725 ns |   1.37 |    0.05 |
|           HandleVerbsRuntime |  26,888.69 ns |   660.595 ns |   734.250 ns |  24.18 |    0.71 |
|                     ParseCmd |   1,153.53 ns |    26.479 ns |    27.192 ns |   1.04 |    0.03 |
|                 ParseCmdAuto |   1,915.15 ns |    21.508 ns |    17.960 ns |   1.71 |    0.04 |
|                   ParseLinux |   1,763.82 ns |    33.752 ns |    43.887 ns |   1.58 |    0.05 |
|               ParseLinuxAuto |   2,556.28 ns |    47.460 ns |    42.072 ns |   2.29 |    0.06 |
|                     ParseUrl |   4,800.81 ns |    86.862 ns |    72.534 ns |   4.29 |    0.07 |
|                 ParseUrlAuto |   6,274.80 ns |   125.106 ns |   205.553 ns |   5.65 |    0.25 |
|            CommandLineParser | 136,090.91 ns | 1,072.509 ns |   895.594 ns | 121.71 |    2.66 |

總結來說：完成一次解析只需要1091ns，也就是大約10 tick。

說明：

- NoArgs 表示沒有傳入參數
- Auto 表示自動查找 Parser 而不是手動傳入
- Runtime 表示使用運行時解析器
- Handle 表示進行多謂詞匹配
- CommandLineParser 是使用的 CommandLineParser 庫作為對照
- 測試使用的參數：
    - Windows 風格：`"C:\Users\lvyi\Desktop\重命名试验.enbx" -Cloud -Iwb -m Display -s -p Outside -StartupSession 89EA9D26-6464-4E71-BD04-AA6516063D83`
    - Cmd 風格：`"C:\Users\lvyi\Desktop\重命名试验.enbx" /Cloud /Iwb /m Display /s /p Outside /StartupSession 89EA9D26-6464-4E71-BD04-AA6516063D83`
    - Linux 風格：`"C:\Users\lvyi\Desktop\重命名试验.enbx" --cloud --iwb -m Display -s -p Outside --startup-session 89EA9D26-6464-4E71-BD04-AA6516063D83`
    - Url 風格：`walterlv://open/?file=C:\Users\lvyi\Desktop\%E9%87%8D%E5%91%BD%E5%90%8D%E8%AF%95%E9%AA%8C.enbx&cloud=true&iwb=true&silence=true&placement=Outside&startupSession=89EA9D26-6464-4E71-BD04-AA6516063D83`
