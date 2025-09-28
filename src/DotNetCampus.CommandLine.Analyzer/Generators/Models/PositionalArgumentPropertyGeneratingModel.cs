using DotNetCampus.Cli.Compiler;
using DotNetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

internal sealed record PositionalArgumentPropertyGeneratingModel : PropertyGeneratingModel
{
    private PositionalArgumentPropertyGeneratingModel(IPropertySymbol propertySymbol) : base(propertySymbol)
    {
    }

    public required int Index { get; init; }

    public required int Length { get; init; }

    public int PropertyIndex { get; set; } = -1;

    public static PositionalArgumentPropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
    {
        var valueAttribute = propertySymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<ValueAttribute>());
        if (valueAttribute is null)
        {
            return null;
        }

        var index = valueAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
        var length =
            // 优先从命名属性中拿。
            valueAttribute.NamedArguments
                .FirstOrDefault(a => a.Key == nameof(ValueAttribute.Length)).Value.Value?.ToString()
            // 其次从构造函数参数中拿。
            ?? valueAttribute.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString();

        return new PositionalArgumentPropertyGeneratingModel(propertySymbol)
        {
            Index = index is not null && int.TryParse(index, out var result) ? result : 0,
            Length = length is not null && int.TryParse(length, out var result2) ? result2 : 1,
        };
    }
}
