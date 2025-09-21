using Microsoft.CodeAnalysis;

namespace DotNetCampus.CommandLine.Generators.Models;

internal abstract record PropertyGeneratingModel
{
    protected PropertyGeneratingModel(IPropertySymbol property)
    {
        PropertySymbol = property;
        PropertyName = property.Name;
        Type = property.Type;
        IsRequired = property.IsRequired;
        IsInitOnly = property.SetMethod?.IsInitOnly ?? false;
        IsNullable = property.Type.NullableAnnotation == NullableAnnotation.Annotated;
        IsValueType = property.Type.IsValueType;
    }

    public IPropertySymbol PropertySymbol { get; }

    public string PropertyName { get; }

    public ITypeSymbol Type { get; }

    public bool IsRequired { get; }

    public bool IsInitOnly { get; }

    public bool IsRequiredOrInit => IsRequired || IsInitOnly;

    public bool IsNullable { get; }

    public bool IsValueType { get; }
}
