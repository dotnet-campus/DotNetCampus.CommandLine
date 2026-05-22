using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Properties;

namespace DotNetCampus.Cli;

[Command(Description = nameof(LocalizableStrings.SampleCommandDescription))]
internal class DefaultOptions
{
    [RawArguments]
    public required string[] MainArgs { get; init; }

    [Option(Description = nameof(LocalizableStrings.SamplePropertyDescription))]
    public string? DefaultText { get; set; }

    [Option(Description = nameof(LocalizableStrings.SampleDirectoryPropertyDescription), ValueName = "directory_path")]
    public string? DefaultDirectory { get; set; }

    internal void Run()
    {
        if (DefaultText is { } text)
        {
            Console.WriteLine($"Text: {text}");
        }
        if (DefaultDirectory is { } dir)
        {
            Console.WriteLine($"Directory: {dir}");
        }
        if (MainArgs is { Length: > 0 })
        {
            Console.WriteLine($"Raw args: {string.Join(" ", MainArgs)}");
        }
    }
}
