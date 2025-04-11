using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Properties;

#pragma warning disable CS0618 // 类型或成员已过时

namespace dotnetCampus.Cli;

[Verb("sample-options", LocalizableDescription = nameof(LocalizableStrings.SampleVerbDescription))]
internal class SampleOptions
{
    [Option(LocalizableDescription = nameof(LocalizableStrings.SamplePropertyDescription))]
    public string? SampleText { get; set; }

    [Option(LocalizableDescription = nameof(LocalizableStrings.SampleFilePropertyDescription))]
    public FileInfo? SampleFile { get; set; }

    internal void Run()
    {
        Console.WriteLine("示例行为执行……");
    }
}
