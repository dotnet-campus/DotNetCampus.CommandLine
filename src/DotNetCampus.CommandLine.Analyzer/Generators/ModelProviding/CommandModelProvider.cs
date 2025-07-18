﻿using System.Collections.Immutable;
using System.Globalization;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Utils;
using DotNetCampus.CommandLine.Utils.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetCampus.CommandLine.Generators.ModelProviding;

internal static class CommandModelProvider
{
    public static IncrementalValuesProvider<CommandObjectGeneratingModel> SelectCommandObjects(this IncrementalGeneratorInitializationContext context)
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
                    i.IsSubclassOrImplementOf(["DotNetCampus.Cli.ICommandOptions"], true));
                // 2. 实现 ICommandHandler 接口。
                var isHandler = typeSymbol.AllInterfaces.Any(i =>
                    i.IsSubclassOrImplementOf(["DotNetCampus.Cli.ICommandHandler"], true));
                // 3. 拥有 [Verb] 特性。
                var attribute = typeSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<VerbAttribute>());
                // 4. 拥有 [Option] 特性的属性。
                var optionProperties = typeSymbol
                    .EnumerateBaseTypesRecursively()                                            // 递归获取所有基类
                    .Reverse()                                                                  // （注意我们先给父类属性赋值，再给子类属性赋值）
                    .SelectMany(x => x.GetMembers())                              //                 的所有成员，
                    .OfType<IPropertySymbol>()                                                  //                             然后取出属性，
                    .Select(OptionPropertyGeneratingModel.TryParse)                             // 解析出 OptionPropertyGeneratingModel。
                    .OfType<OptionPropertyGeneratingModel>()
                    .GroupBy(x => x.PropertyName)              // 按属性名去重。
                    .Select(x => x.Last())  // 随后，取子类的属性（去除父类的重名属性）。
                    .ToImmutableArray();
                // 5. 拥有 [Value] 特性的属性。
                var valueProperties = typeSymbol
                    .EnumerateBaseTypesRecursively()                                            // 递归获取所有基类
                    .Reverse()                                                                  // （注意我们先给父类属性赋值，再给子类属性赋值）
                    .SelectMany(x => x.GetMembers())                              //                 的所有成员，
                    .OfType<IPropertySymbol>()                                                  //                             然后取出属性，
                    .Select(ValuePropertyGeneratingModel.TryParse)                              // 解析出 ValuePropertyGeneratingModel。
                    .OfType<ValuePropertyGeneratingModel>()
                    .GroupBy(x => x.PropertyName)               // 按属性名去重。
                    .Select(x => x.Last())   // 随后，取子类的属性（去除父类的重名属性）。
                    .ToImmutableArray();

                if (!isOptions && !isHandler && attribute is null && optionProperties.IsEmpty && valueProperties.IsEmpty)
                {
                    // 不是命令行选项类型。
                    return null;
                }

                var @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
                var verbName = attribute?.ConstructorArguments[0].Value?.ToString();
                var isPublic = typeSymbol.DeclaredAccessibility == Accessibility.Public;

                return new CommandObjectGeneratingModel
                {
                    Namespace = @namespace,
                    CommandObjectType = typeSymbol,
                    IsPublic = isPublic,
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

internal record CommandObjectGeneratingModel
{
    private static readonly ImmutableArray<string> SupportedPostfixes = ["Options", "CommandOptions", "Handler", "CommandHandler", ""];

    public required string Namespace { get; init; }

    public required INamedTypeSymbol CommandObjectType { get; init; }

    public required bool IsPublic { get; init; }

    public required string? VerbName { get; init; }

    public required bool IsHandler { get; init; }

    public required ImmutableArray<OptionPropertyGeneratingModel> OptionProperties { get; init; }

    public required ImmutableArray<ValuePropertyGeneratingModel> ValueProperties { get; init; }

    public string GetBuilderTypeName() => GetBuilderTypeName(CommandObjectType);

    public static string GetBuilderTypeName(INamedTypeSymbol commandObjectType)
    {
        return $"{commandObjectType.Name}Builder";
    }
}

internal record OptionPropertyGeneratingModel
{
    public required string PropertyName { get; init; }

    public required ITypeSymbol Type { get; init; }

    public required bool IsRequired { get; init; }

    public required bool IsInitOnly { get; init; }

    public required bool IsNullable { get; init; }

    public required bool IsValueType { get; init; }

    public required char? ShortName { get; init; }

    public required string? LongName { get; init; }

    public required bool? CaseSensitive { get; init; }

    public required bool ExactSpelling { get; init; }

    public required ImmutableArray<string> Aliases { get; init; }

    public ImmutableArray<string> GetNormalizedLongNames()
    {
        if (ExactSpelling)
        {
            return [LongName ?? PropertyName];
        }

        return (CaseSensitive, LongName) switch
        {
            // 如果没有指定长名称，那么长名称就是根据属性名推测的，这时一定自动将其转换为 kebab-case 小写风格。
            (_, null) => [NamingHelper.MakeKebabCase(LongName ?? PropertyName, true, true)],

            // 如果指定了大小写敏感，那么在转换为 kebab-case 时，不转换大小写。
            (true, _) => [NamingHelper.MakeKebabCase(LongName, true, false)],

            // 如果指定了大小写不敏感，那么在转换为 kebab-case 时，统一转换为小写。
            (false, _) => [NamingHelper.MakeKebabCase(LongName, true, true)],

            // 如果没有在属性处指定大小写敏感，那么给出两个转换的候选，延迟到运行时再决定。
            (null, _) =>
            [
                ..new List<string>
                {
                    NamingHelper.MakeKebabCase(LongName, true, false),
                    NamingHelper.MakeKebabCase(LongName, true, true),
                }.Distinct(StringComparer.Ordinal),
            ],
        };
    }

    public string GetDisplayCommandOption()
    {
        var caseSensitive = CaseSensitive is true;

        if (LongName is { } longName)
        {
            return $"--{NamingHelper.MakeKebabCase(longName, !caseSensitive, !caseSensitive)}";
        }

        if (ShortName is { } shortName)
        {
            return $"-{shortName}";
        }

        return $"--{NamingHelper.MakeKebabCase(PropertyName, !caseSensitive, !caseSensitive)}";
    }

    public IReadOnlyList<string> GenerateAllNames(
        Func<string, string> shortNameCreator,
        Func<string, string> longNameCreator,
        Func<string, string, string> caseLongNameCreator,
        Func<string, string> aliasCreator)
    {
        var list = new List<string>();

        if (ShortName is { } shortName)
        {
            list.Add(shortNameCreator(shortName.ToString(CultureInfo.InvariantCulture)));
        }

        var longNames = GetNormalizedLongNames();
        if (longNames.Length is 1)
        {
            list.Add(longNameCreator(longNames[0]));
        }
        else if (longNames.Length is 2)
        {
            list.Add(caseLongNameCreator(longNames[0], longNames[1]));
        }

        if (Aliases is { Length: > 0 } aliases)
        {
            foreach (var alias in aliases)
            {
                list.Add(aliasCreator(alias));
            }
        }

        return list;
    }

    public static OptionPropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
    {
        var optionAttribute = propertySymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<OptionAttribute>());
        if (optionAttribute is null)
        {
            return null;
        }

        var longName = optionAttribute.ConstructorArguments.FirstOrDefault(x => x.Type?.SpecialType is SpecialType.System_String).Value?.ToString();
        var shortName = optionAttribute.ConstructorArguments.FirstOrDefault(x => x.Type?.SpecialType is SpecialType.System_Char).Value?.ToString();
        var caseSensitive = optionAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(OptionAttribute.CaseSensitive)).Value.Value?.ToString();
        var exactSpelling = optionAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(OptionAttribute.ExactSpelling)).Value.Value is true;
        var aliases = optionAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(OptionAttribute.Aliases)).Value switch
        {
            { Kind: TypedConstantKind.Array } typedConstant => typedConstant.Values.Select(a => a.Value?.ToString())
                .Where(a => !string.IsNullOrEmpty(a))
                .OfType<string>()
                .ToImmutableArray(),
            _ => [],
        };

        return new OptionPropertyGeneratingModel
        {
            PropertyName = propertySymbol.Name,
            Type = propertySymbol.Type,
            IsRequired = propertySymbol.IsRequired,
            IsInitOnly = propertySymbol.SetMethod?.IsInitOnly ?? false,
            IsNullable = propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated,
            IsValueType = propertySymbol.Type.IsValueType,
            ShortName = shortName?.Length == 1 ? shortName[0] : null,
            LongName = longName,
            CaseSensitive = caseSensitive is not null && bool.TryParse(caseSensitive, out var result) ? result : null,
            ExactSpelling = exactSpelling,
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

    public required bool IsValueType { get; init; }

    public required int? Index { get; init; }

    public required int? Length { get; init; }

    public static ValuePropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
    {
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
            IsInitOnly = propertySymbol.SetMethod?.IsInitOnly ?? false,
            IsNullable = propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated,
            IsValueType = propertySymbol.Type.IsValueType,
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

file static class Extensions
{
    public static IEnumerable<ITypeSymbol> EnumerateBaseTypesRecursively(this ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }
}
