using DotNetCampus.Cli.Utils;

namespace DotNetCampus.Cli;

partial record struct CommandLineStyle
{
    private const ushort FlexibleMagic = 0x98C7;
    private const ushort DotNetMagic = 0x9AE1;
    private const ushort GnuMagic = 0x8DE1;
    private const ushort PosixMagic = 0x89A2;
    private const ushort PowerShellMagic = 0x9ADA;
    private const ushort UrlMagic = 0x9043;

    /// <summary>
    /// 灵活风格。<br/>
    /// 在绝大多数情况下，接受用户输入各种风格的命令行参数（包括 <see cref="DotNet"/>、<see cref="PowerShell"/> 等）；<br/>
    /// 此风格给了用户最大的灵活性。但同时，作为开发者，你定义命令行选项时也应该尽可能避免不同风格间可能出现的歧义。<br/>
    /// 当然，绝大多数情况下，你都不会碰到可能歧义的情况。
    /// </summary>
    /// <remarks>
    /// 注意：在非 Windows 系统上使用 <see cref="Flexible"/> 可能在某些情况下出现解析歧义，<br/>
    /// 这是因为 Linux/macOS 系统中，`/` 字符是合法的文件名字符，<br/>
    /// 如果正好存在某个文件在 `/` 目录下，且文件名与某个选项名相同时，可能会被误解析为选项。
    /// </remarks>
    public static CommandLineStyle Flexible => new CommandLineStyle(FlexibleMagic)
    {
        Name = "Flexible",
        OptionValueSeparators = CommandSeparatorChars.Create(':', '='),
        CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
    };

    /// <summary>
    /// .NET CLI 风格。<br/>
    /// <list type="number">
    /// <item>命令和长选项采用 kebab-case 命名法，区分大小写</item>
    /// <item>长选项使用 -- 前缀，如 --option-name</item>
    /// <item>短选项使用 - 前缀，如 -o；支持多个字符的短选项（仍是一个选项），如 -tl</item>
    /// <item>选项和值之间使用这些分隔符之一：冒号(:)、等号(=)、空格( )</item>
    /// <item>布尔选项可以不带值，视为 true；也可以带 true/false、on/off、yes/no、1/0 等值</item>
    /// <item>位置参数按顺序解析，可与选项交叉出现；使用 -- 单独一项来标记位置参数的开始，后续所有参数均视为位置参数</item>
    /// <item>当值为集合时，可使用这些分隔符之一：逗号(,)、分号(;)，也可多次指定，如 --option value1 --option value2</item>
    /// <item>当值为字典时，使用等号(=)分隔键和值，如 --option key=value</item>
    /// </list>
    /// </summary>
    public static CommandLineStyle DotNet => new CommandLineStyle(DotNetMagic)
    {
        Name = "DotNet",
        OptionValueSeparators = CommandSeparatorChars.Create(':', '='),
        CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
    };

    /// <summary>
    /// GNU 风格。<br/>
    /// <list type="number">
    /// <item>命令和选项采用 kebab-case 命名法，区分大小写</item>
    /// <item>长选项使用 -- 前缀，如 --option-name</item>
    /// <item>短选项使用 - 前缀，如 -o；支持多个字符的短选项组合，如 -abc（等同于 -a -b -c）</item>
    /// <item>选项和值之间使用这些分隔符之一：等号(=)、空格( )；短选项还支持直接跟值，如 -o1.txt</item>
    /// <item>布尔选项可以不带值，视为 true；也可以带 true/false、on/off、yes/no、1/0 等值</item>
    /// <item>位置参数按顺序解析，可与选项交叉出现；使用 -- 单独一项来标记位置参数的开始，后续所有参数均视为位置参数</item>
    /// <item>当值为集合时，可使用这些分隔符之一：逗号(,)、分号(;)，也可多次指定，如 --option value1 --option value2</item>
    /// <item>当值为字典时，使用等号(=)分隔键和值，如 --option key=value</item>
    /// </list>
    /// </summary>
    public static CommandLineStyle Gnu => new CommandLineStyle(GnuMagic)
    {
        Name = "Gnu",
        OptionValueSeparators = CommandSeparatorChars.Create('='),
        CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
    };

    /// <summary>
    /// POSIX/UNIX 风格。<br/>
    /// <list type="number">
    /// <item>只支持短选项，采用单字符命名法，区分大小写</item>
    /// <item>短选项使用 - 前缀，如 -o；支持多个字符的短选项组合，如 -abc（等同于 -a -b -c）</item>
    /// <item>选项和值之间使用空格( ) 分隔；不支持其他分隔符</item>
    /// <item>布尔选项可以不带值，视为 true；也可以带 true/false、on/off、yes/no、1/0 等值</item>
    /// <item>位置参数按顺序解析，可与选项交叉出现；使用 -- 单独一项来标记位置参数的开始，后续所有参数均视为位置参数</item>
    /// <item>当值为集合时，可使用这些分隔符之一：逗号(,)、分号(;)，也可多次指定，如 -o value1 -o value2</item>
    /// <item>当值为字典时，使用等号(=)分隔键和值，如 -o key=value</item>
    /// </list>
    /// </summary>
    public static CommandLineStyle Posix => new CommandLineStyle(PosixMagic)
    {
        Name = "Posix",
        OptionValueSeparators = CommandSeparatorChars.Create(),
        CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
    };

    /// <summary>
    /// PowerShell 风格。<br/>
    /// <list type="number">
    /// <item>命令和选项采用 PascalCase 命名法，不区分大小写</item>
    /// <item>长选项使用 - 前缀，如 -OptionName</item>
    /// <item>短选项使用 - 前缀，如 -o；支持多个字符的短选项（仍是一个选项），如 -tl</item>
    /// <item>选项和值之间使用这些分隔符之一：冒号(:)、等号(=)、空格( )</item>
    /// <item>布尔选项可以不带值，视为 true；也可以带 true/false、on/off、yes/no、1/0 等值</item>
    /// <item>位置参数按顺序解析，可与选项交叉出现</item>
    /// <item>当值为集合时，可使用这些分隔符之一：逗号(,)、分号(;)，也可多次指定，如 -Option value1 -Option value2</item>
    /// <item>当值为字典时，使用等号(=)分隔键和值，如 -Option key=value</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// 注意：在非 Windows 系统上使用 <see cref="PowerShell"/> 可能在某些情况下出现解析歧义，<br/>
    /// 这是因为 Linux/macOS 系统中，`/` 字符是合法的文件名字符，<br/>
    /// 如果正好存在某个文件在 `/` 目录下，且文件名与某个选项名相同时，可能会被误解析为选项。
    /// </remarks>
    public static CommandLineStyle PowerShell => new CommandLineStyle(PowerShellMagic)
    {
        Name = "PowerShell",
        OptionValueSeparators = CommandSeparatorChars.Create(':', '='),
        CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
    };

    /// <summary>
    /// 内部使用。当发现命令行参数只有一个，且符合 URL 格式时，无论用户设置了哪种命令行风格，都会使用此风格进行解析。
    /// </summary>
    public static CommandLineStyle Url => new CommandLineStyle(UrlMagic)
    {
        Name = "Url",
        OptionValueSeparators = CommandSeparatorChars.Create('='),
        CollectionValueSeparators = CommandSeparatorChars.Create(',', ';'),
    };

#if DEBUG

    private static CommandLineStyle FlexibleDefinition => new CommandLineStyle
    {
        Name = "Flexible",
        CaseSensitive = false,
        SupportsLongOption = true,
        SupportsShortOption = true,
        SupportsShortOptionCombination = false,
        SupportsMultiCharShortOption = false,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsExplicitBooleanOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = false,
        NamingPolicy = CommandNamingPolicy.Both,
        OptionPrefix = CommandOptionPrefix.Any,
        UnknownOptionTakesValue = UnknownOptionBehavior.TakesOptionalValue,
    };

    private static CommandLineStyle DotNetDefinition => new CommandLineStyle
    {
        Name = "DotNet",
        CaseSensitive = true,
        SupportsLongOption = true,
        SupportsShortOption = true,
        SupportsShortOptionCombination = false,
        SupportsMultiCharShortOption = true,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsExplicitBooleanOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = false,
        NamingPolicy = CommandNamingPolicy.KebabCase,
        OptionPrefix = CommandOptionPrefix.DoubleDash,
        UnknownOptionTakesValue = UnknownOptionBehavior.TakesOptionalValue,
    };

    private static CommandLineStyle GnuDefinition => new CommandLineStyle
    {
        Name = "Gnu",
        CaseSensitive = true,
        SupportsLongOption = true,
        SupportsShortOption = true,
        SupportsShortOptionCombination = true,
        SupportsMultiCharShortOption = false,
        SupportsShortOptionValueWithoutSeparator = true,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsExplicitBooleanOptionValue = false,
        SupportsSpaceSeparatedCollectionValues = false,
        NamingPolicy = CommandNamingPolicy.KebabCase,
        OptionPrefix = CommandOptionPrefix.DoubleDash,
        UnknownOptionTakesValue = UnknownOptionBehavior.TakesOptionalValue,
    };

    private static CommandLineStyle PosixDefinition => new CommandLineStyle
    {
        Name = "Posix",
        CaseSensitive = true,
        SupportsLongOption = false,
        SupportsShortOption = true,
        SupportsShortOptionCombination = true,
        SupportsMultiCharShortOption = false,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsExplicitBooleanOptionValue = false,
        SupportsSpaceSeparatedCollectionValues = false,
        NamingPolicy = CommandNamingPolicy.PascalCase,
        // Posix 不支持长选项，使用 DoubleDash 的含义是 '-' 一定表示短选项。
        OptionPrefix = CommandOptionPrefix.DoubleDash,
        UnknownOptionTakesValue = UnknownOptionBehavior.TakesOptionalValue,
    };

    private static CommandLineStyle PowerShellDefinition => new CommandLineStyle
    {
        Name = "PowerShell",
        CaseSensitive = false,
        SupportsLongOption = true,
        SupportsShortOption = true,
        SupportsShortOptionCombination = false,
        SupportsMultiCharShortOption = true,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = true,
        SupportsExplicitBooleanOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = false,
        NamingPolicy = CommandNamingPolicy.PascalCase,
        OptionPrefix = CommandOptionPrefix.SlashOrDash,
        UnknownOptionTakesValue = UnknownOptionBehavior.TakesOptionalValue,
    };

    /// <summary>
    /// 内部使用。当发现命令行参数只有一个，且符合 URL 格式时，无论用户设置了哪种命令行风格，都会使用此风格进行解析。
    /// </summary>
    private static CommandLineStyle UrlDefinition => new CommandLineStyle
    {
        Name = "Url",
        CaseSensitive = false,
        SupportsLongOption = true,
        SupportsShortOption = false,
        SupportsShortOptionCombination = false,
        SupportsMultiCharShortOption = false,
        SupportsShortOptionValueWithoutSeparator = false,
        SupportsSpaceSeparatedOptionValue = false,
        SupportsExplicitBooleanOptionValue = true,
        SupportsSpaceSeparatedCollectionValues = false,
        NamingPolicy = CommandNamingPolicy.Both,
        OptionPrefix = CommandOptionPrefix.DoubleDash,
        UnknownOptionTakesValue = UnknownOptionBehavior.TakesOptionalValue,
    };

    /// <summary>
    /// 在单元测试里调用，以验证各种预定义的命令行风格没有被意外修改。
    /// </summary>
    public static void VerifyMagicNumbers()
    {
        var flexibleMagic = FlexibleDefinition.GetMagicNumber();
        var dotNetMagic = DotNetDefinition.GetMagicNumber();
        var gnuMagic = GnuDefinition.GetMagicNumber();
        var posixMagic = PosixDefinition.GetMagicNumber();
        var powerShellMagic = PowerShellDefinition.GetMagicNumber();
        var urlMagic = UrlDefinition.GetMagicNumber();
        if (flexibleMagic != FlexibleMagic || dotNetMagic != DotNetMagic ||
            gnuMagic != GnuMagic || posixMagic != PosixMagic ||
            powerShellMagic != PowerShellMagic || urlMagic != UrlMagic)
        {
            throw new InvalidOperationException($"""
The magic numbers of predefined command line styles have been changed. Please copy the code to update them:
```csharp
private const ushort FlexibleMagic = 0x{flexibleMagic:X4};
private const ushort DotNetMagic = 0x{dotNetMagic:X4};
private const ushort GnuMagic = 0x{gnuMagic:X4};
private const ushort PosixMagic = 0x{posixMagic:X4};
private const ushort PowerShellMagic = 0x{powerShellMagic:X4};
private const ushort UrlMagic = 0x{urlMagic:X4};
```
""");
        }
    }

#endif
}
