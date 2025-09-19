namespace DotNetCampus.Cli.Tests.Fakes;

[dotnetCampus.Cli.Verb("Edit")]
[DotNetCampus.Cli.Temp40.Compiler.Command("Edit")]
public class EditOptions
{
    [dotnetCampus.Cli.Value(0), dotnetCampus.Cli.Option('f', "File")]
    [DotNetCampus.Cli.Temp40.Compiler.Value(0), DotNetCampus.Cli.Temp40.Compiler.Option('f', "File")]
    public string? FilePath { get; set; }
}

[dotnetCampus.Cli.Verb("Print")]
[DotNetCampus.Cli.Temp40.Compiler.Command("Print")]
public class PrintOptions
{
    [DotNetCampus.Cli.Temp40.Compiler.Value(0), DotNetCampus.Cli.Temp40.Compiler.Option('f', "File")]
    public string? FilePath { get; set; }

    [DotNetCampus.Cli.Temp40.Compiler.Option('p', "Printer")]
    public string? Printer { get; set; }
}

[dotnetCampus.Cli.Verb("Share")]
[DotNetCampus.Cli.Temp40.Compiler.Command("Share")]
public class ShareOptions
{
    [DotNetCampus.Cli.Temp40.Compiler.Option('t', "Target")]
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
