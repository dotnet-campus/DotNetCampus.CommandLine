using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli;

internal class EditOptionsBase
{
    [Option('f', "file", Description = "The file to edit.")]
    public required string FilePath { get; init; }

    [Option("read-only", Description = "Open in read-only mode.")]
    public bool? ReadOnly { get; init; }
}

[Command("edit", Description = "Open a file for editing.")]
internal class EditHandler : EditOptionsBase, ICommandHandler
{
    [Option('l', "line", Description = "Jump to line number.")]
    public int? Line { get; init; }

    [Option(["e", "E"], ["encoding", "enc"], Description = "File encoding.")]
    public string? Encoding { get; init; }

    public Task<int> RunAsync()
    {
        Console.WriteLine($"Editing: {FilePath}");
        if (ReadOnly is true)
        {
            Console.WriteLine("(read-only)");
        }
        if (Line is { } line)
        {
            Console.WriteLine($"Line: {line}");
        }
        if (Encoding is { } encoding)
        {
            Console.WriteLine($"Encoding: {encoding}");
        }
        return Task.FromResult(0);
    }
}
