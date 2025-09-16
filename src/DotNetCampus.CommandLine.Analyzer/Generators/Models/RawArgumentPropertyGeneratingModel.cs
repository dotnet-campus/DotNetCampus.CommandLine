using DotNetCampus.Cli.Compiler;
using DotNetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

internal sealed record RawArgumentPropertyGeneratingModel : PropertyGeneratingModel
{
    private RawArgumentPropertyGeneratingModel(IPropertySymbol propertySymbol) : base(propertySymbol)
    {
    }

    public static RawArgumentPropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
    {
        var rawArgumentsAttribute = propertySymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<RawArgumentsAttribute>());
        if (rawArgumentsAttribute is null)
        {
            return null;
        }

        return new RawArgumentPropertyGeneratingModel(propertySymbol)
        {
        };
    }
}
