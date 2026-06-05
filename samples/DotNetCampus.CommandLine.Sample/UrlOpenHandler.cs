using DotNetCampus.Cli.Compiler;

namespace DotNetCampus.Cli;

[Command("open", Description = "Open a URL or protocol link.")]
internal class UrlOpenHandler : ICommandHandler<AppState>
{
    [Value(0, Description = "The URL or path to open.")]
    public string? Target { get; init; }

    [Option("fragment", Description = "URL fragment identifier.")]
    public string? Fragment { get; init; }

    [Option("ref", Description = "A reference parameter from the URL query.")]
    public string? Ref { get; init; }

    public Task<int> RunAsync(AppState state)
    {
        Console.WriteLine($"[{state.AppName}] Opening: {Target}");
        if (Fragment is { } fragment)
        {
            Console.WriteLine($"Fragment: #{fragment}");
        }
        if (Ref is { } r)
        {
            Console.WriteLine($"Ref: {r}");
        }
        return Task.FromResult(0);
    }
}

internal class AppState
{
    public required string AppName { get; init; }
}
