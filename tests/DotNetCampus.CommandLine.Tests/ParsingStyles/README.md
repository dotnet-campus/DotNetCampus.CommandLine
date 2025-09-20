# 规则一览

## 命令行风格

```csharp
// 使用 .NET CLI 风格解析命令行参数
var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);
```

支持的风格包括：

- `CommandLineStyle.Flexible`（默认）：灵活风格，在各种风格间提供最大的兼容性，默认大小写不敏感
- `CommandLineStyle.DotNet`：.NET CLI 风格，默认大小写敏感
- `CommandLineStyle.Gnu`：符合 GNU 规范的风格，默认大小写敏感
- `CommandLineStyle.Posix`：符合 POSIX 规范的风格，默认大小写敏感
- `CommandLineStyle.PowerShell`：PowerShell 风格，默认大小写不敏感

默认情况下，这些风格的详细区别如下：

| 风格             | Flexible     | DotNet       | Gnu          | Posix      | PowerShell  | URL               |
| ---------------- | ------------ | ------------ | ------------ | ---------- | ----------- | ----------------- |
| 大小写           | 不敏感       | 敏感         | 敏感         | 敏感       | 不敏感      | 不敏感            |
| 长选项           | 支持         | 支持         | 支持         | 不支持     | 支持        | 支持              |
| 短选项           | 支持         | 支持         | 支持         | 支持       | 支持        | 不支持            |
| 选项值 `=`       | -o=value     | -o=value     | -o=value     |            |             | option=value      |
| 选项值 `:`       | -o:value     | -o:value     |              |            |             |                   |
| 选项值 ` `       | -o value     | -o value     | -o value     | -o value   | -o value    |                   |
| 布尔选项         | -o           | -o           | -o           | -o         | -o          | option            |
| 布尔选项         | -o=true      | -o=true      |              |            | -o:true     | option=true       |
| 布尔值           | true/false   | true/false   | true/false   | true/false | true/false  | true/false        |
| 布尔值           | yes/no       | yes/no       | yes/no       | yes/no     | yes/no      | yes/no            |
| 布尔值           | on/off       | on/off       | on/off       | on/off     | on/off      | on/off            |
| 布尔值           | 1/0          | 1/0          | 1/0          | 1/0        | 1/0         | 1/0               |
| 集合选项         | -o A -o B    | -o A -o B    | -o A -o B    | -o A -o B  | -o A -o B   | option=A&option=B |
| 集合选项 `,`     | -o A,B,C     | -o A,B,C     | -o A,B,C     | -o A,B,C   | -o A,B,C    | -o A,B,C          |
| 集合选项 `;`     | -o A;B;C     | -o A;B;C     | -o A;B;C     | -o A;B;C   | -o A;B;C    | -o A;B;C          |
| 集合选项 ` `     | -o A B C     | -o A B C     |              |            | -o A B C    |                   |
| 字典选项         | -o:A=X;B=Y   | -o:A=X;B=Y   |              |            | -o:A=X;B=Y  |                   |
| 多短布尔选项合并 | 不支持       | 不支持       | -abc         | -abc       | 不支持      | 不支持            |
| 单短选项多字符   | -ab          | -ab          | 不支持       | 不支持     | -ab         | 不支持            |
| 短选项直接带值   | 不支持       | 不支持       | -o1.txt      | 不支持     | 不支持      | 不支持            |
| 长选项前缀       | `--` `-` `/` | `--`         | `--`         | 不支持     | `-` `/`     |                   |
| 短选项前缀       | `-` `/`      | `-`          | `-`          | `-`        | `-` `/`     |                   |
| 命名法           | --kebab-case | --kebab-case | --kebab-case |            |             | kebab-case        |
| 命名法           | -PascalCase  |              |              |            | -PascalCase |                   |
| 命名法           | -camelCase   |              |              |            | -camelCase  |                   |
| 命名法           | /PascalCase  |              |              |            | /PascalCase |                   |
| 命名法           | /camelCase   |              |              |            | /camelCase  |                   |

## 必需选项与默认值

当你定义一个属性的时候，有这些标记可用：

1. 使用 `required` 标记一个选项是必须的
1. 使用 `init` 标记一个选项是不可变的
1. 使用 `?` 标记一个选项是可空的

而具体会被赋成什么值取决于以下这些因素：

| required | init | 集合属性 | nullable | 行为     | 解释                                |
| -------- | ---- | -------- | -------- | -------- | ----------------------------------- |
| 1        | _    | _        | _        | 抛异常   | 要求必须传入，没有传就抛异常        |
| 0        | 1    | 1        | _        | 空集合   | 集合永不为 `null`，没传就赋值空集合 |
| 0        | 1    | 0        | 1        | `null`   | 可空，没有传就赋值 `null`           |
| 0        | 1    | 0        | 0        | 默认值   | 不可空，没有传就赋值默认值          |
| 0        | 0    | _        | _        | 保留初值 | 不要求必须或立即赋值的，保留初值    |

- 1 = 标记了
- 0 = 没标记
- _ = 无论有没有标记

1. 可空，无论是引用类型还是值类型，其行为完全一致。要硬说不同，就是那个「默认值」会导致引用类型得到 `null`。
2. 如果未提供必需选项，解析时会抛出`RequiredPropertyNotAssignedException`异常。
3. 上述行为的「保留初值」的意思是，你可以在定义这个属性的时候写一个初值，就像下面这样：

```csharp
// 请注意，这里的初值仅在没有 required 也没有 init 时才生效。
[Option('o', "option-name")]
public string OptionName { get; set; } = "Default Value"
```
