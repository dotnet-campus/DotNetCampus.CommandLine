using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli;

[Command("convert", Description = "Convert input values and demonstrate type parsing.")]
internal class ConvertHandler : ICommandHandler
{
    [Value(0, Description = "The input file to convert.")]
    public required string InputFile { get; init; }

    [Option('f', "format", Description = "Output format.")]
    public OutputFormat Format { get; init; } = OutputFormat.Text;

    [Option("columns", Description = "Columns to include in the output.")]
    public IReadOnlyList<string>? Columns { get; init; }

    [Option('n', "count", Description = "Maximum number of records to convert.")]
    public int? Count { get; init; }

    public Task<int> RunAsync()
    {
        Console.WriteLine($"Converting: {InputFile}");
        Console.WriteLine($"Format: {Format}");
        if (Count is { } count)
        {
            Console.WriteLine($"Max records: {count}");
        }
        if (Columns is { Count: > 0 } columns)
        {
            Console.WriteLine($"Columns: {string.Join(", ", columns)}");
        }
        return Task.FromResult(0);
    }
}

public enum OutputFormat
{
    Text,
    Json,
    Xml,
}
