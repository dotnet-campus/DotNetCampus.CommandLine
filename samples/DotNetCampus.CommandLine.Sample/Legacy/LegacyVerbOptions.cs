using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli.Legacy;

[dotnetCampus.Cli.Verb("Edit")]
[Command("Edit")]
public class EditOptions
{
    [dotnetCampus.Cli.Value(0), dotnetCampus.Cli.Option('f', "File")]
    [Value(0), Option('f', "File")]
    public string? FilePath { get; set; }
}

[dotnetCampus.Cli.Verb("Print")]
[Command("Print")]
public class PrintOptions
{
    [Value(0), Option('f', "File")]
    public string? FilePath { get; set; }

    [Option('p', "Printer")]
    public string? Printer { get; set; }
}

[dotnetCampus.Cli.Verb("Share")]
[Command("Share")]
public class ShareOptions
{
    [Option('t', "Target")]
    public string? Target { get; set; }
}

public class SelfWrittenEditOptionsParser : dotnetCampus.Cli.CommandLineOptionParser<EditOptions>
{
    public SelfWrittenEditOptionsParser()
    {
        var options = new EditOptions();
        Verb = "Edit";
        AddMatch(0, value => options.FilePath = value);
        AddMatch('f', value => options.FilePath = value);
        AddMatch("File", value => options.FilePath = value);
        SetResult(() => options);
    }
}

public class SelfWrittenPrintOptionsParser : dotnetCampus.Cli.CommandLineOptionParser<PrintOptions>
{
    public SelfWrittenPrintOptionsParser()
    {
        var options = new PrintOptions();
        Verb = "Print";
        AddMatch(0, value => options.FilePath = value);
        AddMatch('f', value => options.FilePath = value);
        AddMatch("File", value => options.FilePath = value);
        AddMatch('p', value => options.Printer = value);
        AddMatch("Printer", value => options.Printer = value);
        SetResult(() => options);
    }
}

public class SelfWrittenShareOptionsParser : dotnetCampus.Cli.CommandLineOptionParser<ShareOptions>
{
    public SelfWrittenShareOptionsParser()
    {
        var options = new ShareOptions();
        Verb = "Share";
        AddMatch('t', value => options.Target = value);
        AddMatch("Target", value => options.Target = value);
        SetResult(() => options);
    }
}
