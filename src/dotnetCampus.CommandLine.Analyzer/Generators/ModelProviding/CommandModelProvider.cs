using System.Collections.Immutable;
using dotnetCampus.Cli.Compiler;
using dotnetCampus.Cli.Utils;
using dotnetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.CommandLine.Generators.ModelProviding;

internal static class CommandModelProvider
{
    public static IncrementalValuesProvider<CommandOptionsGeneratingModel> SelectCommandOptions(this IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider((node, ct) =>
            {
                if (node is not ClassDeclarationSyntax and not RecordDeclarationSyntax)
                {
                    // 必须是类型或记录。
                    return false;
                }

                return true;
            }, (c, ct) =>
            {
                if (c.SemanticModel.GetDeclaredSymbol(c.Node, ct) is not INamedTypeSymbol typeSymbol)
                {
                    return null;
                }

                if (typeSymbol.IsAbstract)
                {
                    // 抽象类通常是辅助开发用的，不能被 new 出来。
                    return null;
                }

                // 判断是否符合命令行选项五个特征中的任何一个：
                // 1. 实现 ICommandOptions 接口
                // 2. 实现 ICommandHandler 接口
                // 3. 拥有 [Verb] 特性
                // 4. 拥有 [Option] 特性的属性
                // 5. 拥有 [Value] 特性的属性

                // 1. 实现 ICommandOptions 接口。
                var isOptions = typeSymbol.AllInterfaces.Any(i =>
                    i.IsSubclassOrImplementOf(["dotnetCampus.Cli.ICommandOptions"], true));
                // 2. 实现 ICommandHandler 接口。
                var isHandler = typeSymbol.AllInterfaces.Any(i =>
                    i.IsSubclassOrImplementOf(["dotnetCampus.Cli.ICommandHandler"], true));
                // 3. 拥有 [Verb] 特性。
                var attribute = typeSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<VerbAttribute>());
                // 4. 拥有 [Option] 特性的属性。
                var optionProperties = typeSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(OptionPropertyGeneratingModel.TryParse)
                    .OfType<OptionPropertyGeneratingModel>()
                    .ToImmutableArray();
                // 5. 拥有 [Value] 特性的属性。
                var valueProperties = typeSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(ValuePropertyGeneratingModel.TryParse)
                    .OfType<ValuePropertyGeneratingModel>()
                    .ToImmutableArray();

                if (!isOptions && !isHandler && attribute is null && optionProperties.IsEmpty && valueProperties.IsEmpty)
                {
                    // 不是命令行选项类型。
                    return null;
                }

                var @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
                var verbName = attribute?.ConstructorArguments[0].Value?.ToString();

                return new CommandOptionsGeneratingModel
                {
                    Namespace = @namespace,
                    OptionsType = typeSymbol,
                    VerbName = verbName,
                    IsHandler = isHandler,
                    OptionProperties = optionProperties,
                    ValueProperties = valueProperties,
                };
            })
            .Where(m => m is not null)
            .Select((m, ct) => m!);
    }

    public static IncrementalValuesProvider<AssemblyCommandsGeneratingModel> SelectAssemblyCommands(this IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(typeof(CollectCommandHandlersFromThisAssemblyAttribute).FullName!, (node, ct) =>
        {
            if (node is not ClassDeclarationSyntax cds)
            {
                // 必须是类型。
                return false;
            }

            return true;
        }, (c, ct) =>
        {
            var typeSymbol = c.TargetSymbol;
            var rootNamespace = typeSymbol.ContainingNamespace.ToDisplayString();
            var typeName = typeSymbol.Name;
            var attribute = typeSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<CollectCommandHandlersFromThisAssemblyAttribute>());

            return new AssemblyCommandsGeneratingModel
            {
                Namespace = rootNamespace,
                AssemblyCommandHandlerType = (INamedTypeSymbol)typeSymbol,
            };
        });
    }
}

internal record CommandOptionsGeneratingModel
{
    private static readonly ImmutableArray<string> SupportedPostfixes = ["Options", "CommandOptions", "Handler", "CommandHandler", ""];

    public required string Namespace { get; init; }

    public required INamedTypeSymbol OptionsType { get; init; }

    public required string? VerbName { get; init; }

    public required bool IsHandler { get; init; }

    public required ImmutableArray<OptionPropertyGeneratingModel> OptionProperties { get; init; }

    public required ImmutableArray<ValuePropertyGeneratingModel> ValueProperties { get; init; }

    public string GetVerbCreatorTypeName()
    {
        if (VerbName is { } verbName)
        {
            return $"{NamingHelper.MakePascalCase(verbName)}VerbCreator";
        }

        foreach (var postfix in SupportedPostfixes.Where(postfix => OptionsType.Name.EndsWith(postfix, StringComparison.Ordinal)))
        {
            return $"{OptionsType.Name.Substring(0, OptionsType.Name.Length - postfix.Length)}VerbCreator";
        }

        // 由于集合中最后有一个空字符串，所以此返回将永远不会进来。
        throw new ArgumentException("Member Error.", nameof(SupportedPostfixes));
    }
}

internal record OptionPropertyGeneratingModel
{
    public required string PropertyName { get; init; }

    public required ITypeSymbol Type { get; init; }

    public required bool IsRequired { get; init; }

    public required bool IsInitOnly { get; init; }

    public required bool IsNullable { get; init; }

    public required char? ShortName { get; init; }

    public required string? LongName { get; init; }

    public required bool IgnoreCase { get; init; }

    public required ImmutableArray<string> Aliases { get; init; }

    public string GetNormalizedLongName()
    {
        return NamingHelper.MakeKebabCase(LongName ?? PropertyName);
    }

    public string GetDisplayCommandOption()
    {
        if (LongName is { } longName)
        {
            return $"--{NamingHelper.MakeKebabCase(longName)}";
        }

        if (ShortName is { } shortName)
        {
            return $"-{shortName}";
        }

        return $"--{NamingHelper.MakeKebabCase(PropertyName)}";
    }

    public static OptionPropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
    {
        if (propertySymbol.SetMethod is null)
        {
            return null;
        }
        var optionAttribute = propertySymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<OptionAttribute>());
        if (optionAttribute is null)
        {
            return null;
        }

        var longName = optionAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
        var shortName = optionAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(OptionAttribute.ShortName)).Value.Value?.ToString();
        var ignoreCase = optionAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(OptionAttribute.IgnoreCase)).Value.Value?.ToString();
        var aliases = optionAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(OptionAttribute.Aliases)).Value switch
        {
            { Value: null } => new ImmutableArray<string>(),
            var value => value.Values.Select(a => a.Value?.ToString())
                .Where(a => !string.IsNullOrEmpty(a))
                .OfType<string>()
                .ToImmutableArray(),
        };

        return new OptionPropertyGeneratingModel
        {
            PropertyName = propertySymbol.Name,
            Type = propertySymbol.Type,
            IsRequired = propertySymbol.IsRequired,
            IsInitOnly = propertySymbol.SetMethod.IsInitOnly,
            IsNullable = propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated,
            ShortName = shortName?.Length == 1 ? shortName[0] : null,
            LongName = longName,
            IgnoreCase = ignoreCase is not null && bool.TryParse(ignoreCase, out var result) && result,
            Aliases = aliases,
        };
    }
}

internal record ValuePropertyGeneratingModel
{
    public required string PropertyName { get; init; }

    public required ITypeSymbol Type { get; init; }

    public required bool IsRequired { get; init; }

    public required bool IsInitOnly { get; init; }

    public required bool IsNullable { get; init; }

    public required int? Index { get; init; }

    public required int? Length { get; init; }

    public static ValuePropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
    {
        if (propertySymbol.SetMethod is null)
        {
            return null;
        }
        var valueAttribute = propertySymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<ValueAttribute>());
        if (valueAttribute is null)
        {
            return null;
        }

        var index = valueAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
        var length = valueAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(ValueAttribute.Length)).Value.Value?.ToString();

        return new ValuePropertyGeneratingModel
        {
            PropertyName = propertySymbol.Name,
            Type = propertySymbol.Type,
            IsRequired = propertySymbol.IsRequired,
            IsInitOnly = propertySymbol.SetMethod.IsInitOnly,
            IsNullable = propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated,
            Index = index is not null && int.TryParse(index, out var result) ? result : null,
            Length = length is not null && int.TryParse(length, out var result2) ? result2 : null,
        };
    }
}

internal record AssemblyCommandsGeneratingModel
{
    public required string Namespace { get; init; }

    public required INamedTypeSymbol AssemblyCommandHandlerType { get; init; }
}
