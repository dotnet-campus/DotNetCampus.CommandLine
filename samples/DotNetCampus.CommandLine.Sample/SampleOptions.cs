using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Properties;

#pragma warning disable CS0618 // 类型或成员已过时

namespace DotNetCampus.Cli;

[Command("sample-options", LocalizableDescription = nameof(LocalizableStrings.SampleCommandDescription))]
internal class SampleOptions
{
    [Option(LocalizableDescription = nameof(LocalizableStrings.SamplePropertyDescription))]
    public string? SampleText { get; set; }

    [Option(LocalizableDescription = nameof(LocalizableStrings.SampleFilePropertyDescription))]
    public string? SampleFile { get; set; }

    internal void Run()
    {
        Console.WriteLine("示例行为执行……");
    }
}
