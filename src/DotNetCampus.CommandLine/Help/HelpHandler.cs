using System.Text;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Localizations;

namespace DotNetCampus.Cli.Help;

/// <summary>
/// 帮助文本构建器。
/// </summary>
public class HelpHandler : IHelpHandler
{
    /// <summary>
    /// 帮助的配置信息。
    /// </summary>
    public HelpConfigurations? Configurations { get; init; }

    /// <summary>
    /// 要显示命令行帮助的命令行风格。是开发者期望的风格。
    /// </summary>
    public CommandLineStyle Style { get; init; }

    /// <summary>
    /// 帮助文本中选项/命令/位置参数名称列的最大宽度。超过此宽度的项，其描述将换到下一行显示。
    /// </summary>
    private int MaxColumnWidth => Configurations?.MaxColumnWidth ?? 30;

    /// <inheritdoc />
    public void Handle(MatchedCommand matchedCommand, ICommandObjectMetadata? defaultCommandMetadata,
        IReadOnlyList<ICommandObjectMetadata> subCommandMetadataList)
    {
        var matchedHelp = matchedCommand.Type switch
        {
            // 用户传入的参数，对应了一个现有的子命令。
            MatchedCommandType.Command => matchedCommand.Metadata?.GetHelp(),
            // 用户传入的参数，没有对应任何已注册的子命令，但有已注册的默认命令可用。
            MatchedCommandType.Default => null,
            // 用户传入的参数，没有对应任何已注册的子命令，也没有已注册的默认命令可用。
            _ => null,
        };

        string helpText;
        if (matchedHelp is not null)
        {
            // 特定子命令的帮助。
            helpText = BuildCommandHelp(matchedHelp);
        }
        else
        {
            // 根帮助 + 所有子命令的帮助。
            var defaultHelp = defaultCommandMetadata?.GetHelp();
            var commandHelpList = new List<CommandHelpMetadata>();
            foreach (var metadata in subCommandMetadataList)
            {
                if (metadata.GetHelp() is { } h)
                {
                    commandHelpList.Add(h);
                }
            }
            helpText = BuildRootHelp(defaultHelp, commandHelpList);
        }

        if (Configurations?.HelpMessageWriter is { } writer)
        {
            writer(helpText);
        }
        else
        {
            Console.Out.WriteLine(helpText);
        }
    }

    /// <summary>
    /// 构建根帮助文本，即用户输入 app --help 时的帮助文本。
    /// </summary>
    /// <param name="defaultCommandMetadata">默认帮助元数据（如果没有注册默认命令，则为 <see langword="null"/>）。</param>
    /// <param name="subCommandMetadataList">所有子命令的帮助元数据列表。</param>
    /// <returns>用于输出到控制台的帮助文本。</returns>
    private string BuildRootHelp(
        CommandHelpMetadata? defaultCommandMetadata,
        IReadOnlyList<CommandHelpMetadata> subCommandMetadataList)
    {
        var builder = new StringBuilder();

        // 1. 程序描述
        var hasDescription = BuildDescription(builder, defaultCommandMetadata);
        if (hasDescription)
        {
            builder.AppendLine();
        }

        // 2. 基本用法示例
        var hasUsage = BuildUsage(builder, null, defaultCommandMetadata, subCommandMetadataList);
        if (hasUsage)
        {
            builder.AppendLine();
        }

        // 3. 子命令
        var hasCommands = BuildCommands(builder, subCommandMetadataList);
        if (hasCommands)
        {
            builder.AppendLine();
        }

        // 4. 位置参数
        var hasPositionalArguments = BuildPositionalArguments(builder, defaultCommandMetadata);
        if (hasPositionalArguments)
        {
            builder.AppendLine();
        }

        // 5. 选项
        var hasOptions = BuildOptions(builder, defaultCommandMetadata);
        if (hasOptions)
        {
            builder.AppendLine();
        }

        return builder.ToString();
    }

    /// <summary>
    /// 构建子命令帮助文本，即用户输入 app command --help 时的帮助文本。
    /// </summary>
    /// <param name="help">用户输入的命令行参数所匹配的特定子命令的帮助元数据。</param>
    /// <returns>用于输出到控制台的帮助文本。</returns>
    private string BuildCommandHelp(CommandHelpMetadata help)
    {
        var builder = new StringBuilder();

        var hasDescription = BuildDescription(builder, help);
        if (hasDescription)
        {
            builder.AppendLine();
        }

        var hasUsage = BuildUsage(builder, help.CommandName, help, []);
        if (hasUsage)
        {
            builder.AppendLine();
        }

        var hasPositionalArguments = BuildPositionalArguments(builder, help);
        if (hasPositionalArguments)
        {
            builder.AppendLine();
        }

        var hasOptions = BuildOptions(builder, help);
        if (hasOptions)
        {
            builder.AppendLine();
        }

        return builder.ToString();
    }

    /// <summary>
    /// 派生类重写此方法时，构建程序描述信息。
    /// </summary>
    /// <param name="builder">用于构建帮助文本的 <see cref="StringBuilder"/>。</param>
    /// <param name="defaultCommandMetadata">默认命令的元数据，如果没有注册默认命令，则此参数为 <see langword="null"/>。</param>
    /// <returns>如果存在描述信息，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    protected virtual bool BuildDescription(StringBuilder builder, CommandHelpMetadata? defaultCommandMetadata)
    {
        if (defaultCommandMetadata?.Description is not { } description)
        {
            return false;
        }

        builder.AppendLine(ResolveLocalization(description));
        return true;
    }

    /// <summary>
    /// 派生类重写此方法时，构建用法信息（如 <c>用法：app [选项] &lt;命令&gt;</c>）。
    /// </summary>
    /// <param name="builder">用于构建帮助文本的 <see cref="StringBuilder"/>。</param>
    /// <param name="commandName">当前正在显示帮助的子命令名称。为 <see langword="null"/> 时表示根帮助。</param>
    /// <param name="defaultCommandMetadata">默认命令的元数据，如果没有注册默认命令，则此参数为 <see langword="null"/>。</param>
    /// <param name="subCommandMetadataList">所有子命令的帮助元数据列表。</param>
    /// <returns>如果存在用法信息，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    protected virtual bool BuildUsage(StringBuilder builder,
        string? commandName, CommandHelpMetadata? defaultCommandMetadata, IReadOnlyList<CommandHelpMetadata> subCommandMetadataList)
    {
        var defaultHasOptions = defaultCommandMetadata?.Options.Count > 0;
        var hasSubCommands = subCommandMetadataList.Count > 0;
        var hasPositionalArguments = defaultCommandMetadata?.PositionalArguments.Count > 0;
        var hasUsage = defaultHasOptions || hasSubCommands || hasPositionalArguments;
        if (!hasUsage)
        {
            return false;
        }

        builder.Append(Lang.Current.DotNetCampus.CommandLine.Help.UsageHeader);
        builder.Append(GetProgramName());
        if (commandName is not null)
        {
            builder.Append(' ');
            builder.Append(commandName);
        }
        if (defaultHasOptions)
        {
            builder.Append(' ');
            builder.Append(Lang.Current.DotNetCampus.CommandLine.Help.UsageOptions);
        }
        if (hasSubCommands)
        {
            builder.Append(' ');
            builder.Append(Lang.Current.DotNetCampus.CommandLine.Help.UsageCommand);
        }
        if (hasPositionalArguments)
        {
            builder.Append(' ');
            builder.Append(Lang.Current.DotNetCampus.CommandLine.Help.UsagePositionalArguments);
        }
        builder.AppendLine();
        return true;
    }

    /// <summary>
    /// 派生类重写此方法时，构建子命令列表信息。
    /// </summary>
    /// <param name="builder">用于构建帮助文本的 <see cref="StringBuilder"/>。</param>
    /// <param name="subCommandMetadataList">所有子命令的帮助元数据列表。</param>
    /// <returns>如果存在子命令，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    protected virtual bool BuildCommands(StringBuilder builder, IReadOnlyList<CommandHelpMetadata> subCommandMetadataList)
    {
        if (subCommandMetadataList.Count <= 0)
        {
            return false;
        }

        builder.AppendLine(Lang.Current.DotNetCampus.CommandLine.Help.CommandHeader.ToString());

        var maxColumnWidth = MaxColumnWidth;
        var columnWidth = 0;
        foreach (var sub in subCommandMetadataList)
        {
            var len = sub.CommandName!.Length;
            if (len <= maxColumnWidth && len > columnWidth)
            {
                columnWidth = len;
            }
        }

        foreach (var subCommandMetadata in subCommandMetadataList)
        {
            var name = subCommandMetadata.CommandName!;
            var prefix = $"  {name}";

            if (name.Length > maxColumnWidth)
            {
                builder.AppendLine(prefix);
                if (subCommandMetadata.Description is { } description)
                {
                    builder.Append(new string(' ', columnWidth + 4));
                    builder.AppendLine(ResolveLocalization(description));
                }
            }
            else
            {
                builder.Append(prefix.PadRight(columnWidth + 4));
                if (subCommandMetadata.Description is { } description)
                {
                    builder.Append(ResolveLocalization(description));
                }
                builder.AppendLine();
            }
        }
        return true;
    }

    /// <summary>
    /// 派生类重写此方法时，构建位置参数列表信息。
    /// </summary>
    /// <param name="builder">用于构建帮助文本的 <see cref="StringBuilder"/>。</param>
    /// <param name="defaultCommandMetadata">默认命令的元数据，如果没有注册默认命令，则此参数为 <see langword="null"/>。</param>
    /// <returns>如果存在位置参数，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    protected virtual bool BuildPositionalArguments(StringBuilder builder, CommandHelpMetadata? defaultCommandMetadata)
    {
        if (!(defaultCommandMetadata?.PositionalArguments.Count > 0))
        {
            return false;
        }

        builder.AppendLine(Lang.Current.DotNetCampus.CommandLine.Help.PositionalArgumentsHeader);

        var maxColumnWidth = MaxColumnWidth;
        var columnWidth = 0;
        foreach (var pos in defaultCommandMetadata.PositionalArguments)
        {
            var len = pos.Name.Length + 2; // 2 = [ + ]
            if (len <= maxColumnWidth && len > columnWidth)
            {
                columnWidth = len;
            }
        }

        foreach (var positionalArgument in defaultCommandMetadata.PositionalArguments)
        {
            var nameDisplay = $"[{positionalArgument.Name}]";
            var prefix = $"  {nameDisplay}";

            if (nameDisplay.Length > maxColumnWidth)
            {
                builder.AppendLine(prefix);
                if (positionalArgument.Description is { } description)
                {
                    builder.Append(new string(' ', columnWidth + 4));
                    builder.AppendLine(ResolveLocalization(description));
                }
            }
            else
            {
                builder.Append(prefix.PadRight(columnWidth + 4));
                if (positionalArgument.Description is { } description)
                {
                    builder.Append(ResolveLocalization(description));
                }
                builder.AppendLine();
            }
        }
        return true;
    }

    /// <summary>
    /// 派生类重写此方法时，构建选项列表信息。末尾会自动追加 <c>-h|--help</c> 选项。
    /// </summary>
    /// <param name="builder">用于构建帮助文本的 <see cref="StringBuilder"/>。</param>
    /// <param name="defaultCommandMetadata">默认命令的元数据，如果没有注册默认命令，则此参数为 <see langword="null"/>。</param>
    /// <returns>始终返回 <see langword="true"/>，因为至少会输出 <c>--help</c> 选项。</returns>
    protected virtual bool BuildOptions(StringBuilder builder, CommandHelpMetadata? defaultCommandMetadata)
    {
        if (!(defaultCommandMetadata?.Options.Count > 0))
        {
            builder.AppendLine(Lang.Current.DotNetCampus.CommandLine.Help.OptionsHeader);
            AppendHelpOption(builder, 0);
            return true;
        }

        builder.AppendLine(Lang.Current.DotNetCampus.CommandLine.Help.OptionsHeader);

        var maxColumnWidth = MaxColumnWidth;
        var columnWidth = 0;

        var optionDisplays = new List<(string NamePart, bool IsRequired, string? Description)>();
        foreach (var option in defaultCommandMetadata.Options)
        {
            var namePart = FormatOptionName(option);
            optionDisplays.Add((namePart, option.IsRequired, option.Description));

            if (namePart.Length <= maxColumnWidth && namePart.Length > columnWidth)
            {
                columnWidth = namePart.Length;
            }
        }

        var helpNamePart = "-h|--help";
        if (helpNamePart.Length <= maxColumnWidth && helpNamePart.Length > columnWidth)
        {
            columnWidth = helpNamePart.Length;
        }

        foreach (var (namePart, isRequired, description) in optionDisplays)
        {
            AppendOptionLine(builder, namePart, isRequired, description, columnWidth, maxColumnWidth);
        }

        AppendHelpOption(builder, columnWidth);
        return true;
    }

    /// <summary>
    /// 获取程序名，默认为进程名。
    /// </summary>
    protected virtual string GetProgramName()
    {
#if NET6_0_OR_GREATER
        var processName = Environment.ProcessPath;
        if (processName is not null)
        {
            return Path.GetFileNameWithoutExtension(processName);
        }
#endif
        return Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
    }

    /// <summary>
    /// 派生类重写此方法时，对帮助文本中的描述进行本地化处理。
    /// </summary>
    /// <param name="rawText">原始文本，即开发者在 <see cref="CommandLineAttribute.Description"/> 中指定的值。</param>
    /// <returns>本地化后的文本。如果未设置本地化委托，则原样返回。</returns>
    protected virtual string ResolveLocalization(string rawText)
    {
        return Configurations?.HelpTextLocalizer?.Invoke(rawText) ?? rawText;
    }

    private string FormatOptionName(OptionHelpInfo option)
    {
        var sb = new StringBuilder();
        var first = true;
        foreach (var shortName in option.ShortNames)
        {
            if (!first) sb.Append('|');
            sb.Append('-');
            sb.Append(shortName);
            first = false;
        }
        foreach (var longName in option.LongNames)
        {
            if (!first) sb.Append('|');
            sb.Append("--");
            sb.Append(longName);
            first = false;
        }

        var valuePlaceholder = GetValuePlaceholder(option);
        if (valuePlaceholder is not null)
        {
            sb.Append(' ');
            sb.Append(valuePlaceholder);
        }

        return sb.ToString();
    }

    private static string? GetValuePlaceholder(OptionHelpInfo option)
    {
        if (option.ValueName is { } valueName)
        {
            return option.ValueType is OptionValueType.List or OptionValueType.Dictionary
                ? $"<{valueName}>..."
                : $"<{valueName}>";
        }

        return option.ValueType switch
        {
            OptionValueType.Boolean => null,
            OptionValueType.List => "<value>...",
            OptionValueType.Dictionary => "<key>=<value>...",
            _ => "<value>",
        };
    }

    private void AppendOptionLine(StringBuilder builder, string namePart, bool isRequired, string? description, int columnWidth, int maxColumnWidth)
    {
        var prefix = $"  {namePart}";

        if (namePart.Length > maxColumnWidth)
        {
            builder.AppendLine(prefix);
            builder.Append(new string(' ', columnWidth + 4));
        }
        else
        {
            builder.Append(prefix.PadRight(columnWidth + 4));
        }

        if (isRequired)
        {
            builder.Append(Lang.Current.DotNetCampus.CommandLine.Help.Required).Append(' ');
        }
        if (description is not null)
        {
            builder.Append(ResolveLocalization(description));
        }
        builder.AppendLine();
    }

    private void AppendHelpOption(StringBuilder builder, int columnWidth)
    {
        var helpNamePart = "-h|--help";
        var prefix = $"  {helpNamePart}";
        builder.Append(prefix.PadRight(columnWidth + 4));
        builder.AppendLine(Lang.Current.DotNetCampus.CommandLine.Help.HelpDescription);
    }
}
