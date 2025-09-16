using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

internal record AssemblyCommandsGeneratingModel
{
    public required string Namespace { get; init; }

    public required INamedTypeSymbol AssemblyCommandHandlerType { get; init; }
}
