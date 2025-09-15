using System.Collections.Immutable;
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
                // 3. 拥有 [Command] 或 [Verb] 特性
                // 4. 拥有 [Option] 特性的属性
                // 5. 拥有 [Value] 特性的属性
                // 6. 拥有 [RawArguments] 特性的属性

                // 1. 实现 ICommandOptions 接口。
                var isOptions = typeSymbol.AllInterfaces.Any(i =>
                    i.IsSubclassOrImplementOf(["DotNetCampus.Cli.ICommandOptions"], true));
                // 2. 实现 ICommandHandler 接口。
                var isHandler = typeSymbol.AllInterfaces.Any(i =>
                    i.IsSubclassOrImplementOf(["DotNetCampus.Cli.ICommandHandler"], true));
                // 3. 拥有 [Command] 或 [Verb] 特性。
                var attribute = typeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<CommandAttribute>())
#pragma warning disable CS0618 // 类型或成员已过时
                                ?? typeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<VerbAttribute>())
#pragma warning restore CS0618 // 类型或成员已过时
                    ;
                // 4. 拥有 [Option] 特性的属性。
                var optionProperties = typeSymbol
                    .GetAttributedProperties(OptionPropertyGeneratingModel.TryParse);
                // 5. 拥有 [Value] 特性的属性。
                var valueProperties = typeSymbol
                    .GetAttributedProperties(ValuePropertyGeneratingModel.TryParse);
                // 6. 拥有 [RawArguments] 特性的属性。
                var rawArgumentsProperties = typeSymbol
                    .GetAttributedProperties(RawArgumentsPropertyGeneratingModel.TryParse);

                if (!isOptions && !isHandler && attribute is null && optionProperties.IsEmpty && valueProperties.IsEmpty && rawArgumentsProperties.IsEmpty)
                {
                    // 不是命令行选项类型。
                    return null;
                }

                var @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
                var commandNames = attribute?.ConstructorArguments.FirstOrDefault().Value?.ToString();
                var isPublic = typeSymbol.DeclaredAccessibility == Accessibility.Public;

                for (var i = 0; i < optionProperties.Length; i++)
                {
                    optionProperties[i].PropertyIndex = i;
                }
                for (var i = 0; i < valueProperties.Length; i++)
                {
                    valueProperties[i].PropertyIndex = i + optionProperties.Length;
                }

                return new CommandObjectGeneratingModel
                {
                    Namespace = @namespace,
                    CommandObjectType = typeSymbol,
                    IsPublic = isPublic,
                    CommandNames = commandNames,
                    IsHandler = isHandler,
                    OptionProperties = optionProperties,
                    ValueProperties = valueProperties,
                    RawArgumentsProperties = rawArgumentsProperties,
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

    public required string? CommandNames { get; init; }

    public required bool IsHandler { get; init; }

    public required ImmutableArray<OptionPropertyGeneratingModel> OptionProperties { get; init; }

    public required ImmutableArray<ValuePropertyGeneratingModel> ValueProperties { get; init; }

    public required ImmutableArray<RawArgumentsPropertyGeneratingModel> RawArgumentsProperties { get; init; }

    public string GetBuilderTypeName() => GetBuilderTypeName(CommandObjectType);

    public int GetCommandLevel() => CommandNames switch
    {
        null => 0,
        { } names => names.Count(x => x == ' ') + 1,
    };

    public string? GetKebabCaseCommandNames()
    {
        if (CommandNames is not { } commandNames)
        {
            return null;
        }
        return string.Join(" ", commandNames.Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => NamingHelper.MakeKebabCase(x, false, false)));
    }

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

    public required IReadOnlyList<string> ShortNames { get; init; }

    public required IReadOnlyList<string> LongNames { get; init; }

    public required bool? CaseSensitive { get; init; }

    public int PropertyIndex { get; set; } = -1;

    /// <summary>
    /// 返回开发者定义的长选项名称列表，按定义顺序返回。<br/>
    /// 如果没有定义，则返回 kebab-case 风格的属性名作为默认名称；
    /// 如果有定义，无论定义了什么，都视其为 kebab-case 风格的名称。
    /// </summary>
    public IReadOnlyList<string> GetOrdinalLongNames()
    {
        List<string> list = [];
        if (LongNames.Count is 0)
        {
            list.Add(NamingHelper.MakeKebabCase(PropertyName));
        }
        else
        {
            foreach (var longName in LongNames)
            {
                if (!string.IsNullOrEmpty(longName) && !list.Contains(longName, StringComparer.Ordinal))
                {
                    list.Add(longName);
                }
            }
        }
        return list;
    }

    public IReadOnlyList<string> GetPascalCaseLongNames()
    {
        List<string> list = [];
        if (LongNames.Count is 0)
        {
            list.Add(PropertyName);
        }
        else
        {
            foreach (var longName in LongNames)
            {
                if (!string.IsNullOrEmpty(longName))
                {
                    var pascalCase = NamingHelper.MakePascalCase(longName);
                    if (!list.Contains(pascalCase, StringComparer.Ordinal))
                    {
                        list.Add(pascalCase);
                    }
                }
            }
        }
        return list;
    }

    public IReadOnlyList<string> GetShortNames()
    {
        List<string> list = [];
        foreach (var shortName in ShortNames)
        {
            if (!string.IsNullOrEmpty(shortName) && !list.Contains(shortName, StringComparer.Ordinal))
            {
                list.Add(shortName);
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

        if (optionAttribute.ConstructorArguments.Length is 0)
        {
            // 必须至少有一个构造函数参数。
            return null;
        }

        List<string> shortNames = [];
        List<string> longNames = [];

        if (optionAttribute.ConstructorArguments.Length is 1)
        {
            // 只有一个构造函数参数时，要么是短名称（一定是字符），要么是长名称（一定是字符串）。
            var arg = optionAttribute.ConstructorArguments[0];
            if (arg.Type?.SpecialType is SpecialType.System_Char)
            {
                var shortName = arg.Value?.ToString();
                if (!string.IsNullOrEmpty(shortName))
                {
                    shortNames.Add(shortName!);
                }
            }
            else if (arg.Type?.SpecialType is SpecialType.System_String)
            {
                var longName = arg.Value?.ToString();
                if (!string.IsNullOrEmpty(longName))
                {
                    longNames.Add(longName!);
                }
            }
        }
        else if (optionAttribute.ConstructorArguments.Length is 2)
        {
            // 有两个构造函数参数时，第一个参数是短名称（字符、字符串、字符串数组），第二个参数是长名称（字符串、字符串数组）。
            var shortArg = optionAttribute.ConstructorArguments[0];
            if (shortArg.Type?.SpecialType is SpecialType.System_Char)
            {
                var shortName = shortArg.Value?.ToString();
                if (!string.IsNullOrEmpty(shortName))
                {
                    shortNames.Add(shortName!);
                }
            }
            else if (shortArg.Type?.SpecialType is SpecialType.System_String)
            {
                var shortName = shortArg.Value?.ToString();
                if (!string.IsNullOrEmpty(shortName))
                {
                    shortNames.Add(shortName!);
                }
            }
            else if (shortArg.Kind is TypedConstantKind.Array)
            {
                foreach (var value in shortArg.Values)
                {
                    var shortName = value.Value?.ToString();
                    if (!string.IsNullOrEmpty(shortName) && !shortNames.Contains(shortName, StringComparer.Ordinal))
                    {
                        shortNames.Add(shortName!);
                    }
                }
            }
            var longArg = optionAttribute.ConstructorArguments[1];
            if (longArg.Type?.SpecialType is SpecialType.System_String)
            {
                var longName = longArg.Value?.ToString();
                if (!string.IsNullOrEmpty(longName))
                {
                    longNames.Add(longName!);
                }
            }
            else if (longArg.Kind is TypedConstantKind.Array)
            {
                foreach (var value in longArg.Values)
                {
                    var longName = value.Value?.ToString();
                    if (!string.IsNullOrEmpty(longName) && !longNames.Contains(longName, StringComparer.Ordinal))
                    {
                        longNames.Add(longName!);
                    }
                }
            }
        }

        var caseSensitive = optionAttribute.NamedArguments.FirstOrDefault(a => a.Key == nameof(OptionAttribute.CaseSensitive)).Value.Value?.ToString();

        return new OptionPropertyGeneratingModel
        {
            PropertyName = propertySymbol.Name,
            Type = propertySymbol.Type,
            IsRequired = propertySymbol.IsRequired,
            IsInitOnly = propertySymbol.SetMethod?.IsInitOnly ?? false,
            IsNullable = propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated,
            IsValueType = propertySymbol.Type.IsValueType,
            ShortNames = shortNames,
            LongNames = longNames,
            CaseSensitive = caseSensitive is not null && bool.TryParse(caseSensitive, out var result) ? result : null,
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

    public int PropertyIndex { get; set; } = -1;

    public static ValuePropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
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

internal record RawArgumentsPropertyGeneratingModel
{
    public required string PropertyName { get; init; }

    public required ITypeSymbol Type { get; init; }

    public required bool IsRequired { get; init; }

    public required bool IsInitOnly { get; init; }

    public required bool IsNullable { get; init; }

    public static RawArgumentsPropertyGeneratingModel? TryParse(IPropertySymbol propertySymbol)
    {
        var rawArgumentsAttribute = propertySymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass!.IsAttributeOf<RawArgumentsAttribute>());
        if (rawArgumentsAttribute is null)
        {
            return null;
        }

        return new RawArgumentsPropertyGeneratingModel
        {
            PropertyName = propertySymbol.Name,
            Type = propertySymbol.Type,
            IsRequired = propertySymbol.IsRequired,
            IsInitOnly = propertySymbol.SetMethod?.IsInitOnly ?? false,
            IsNullable = propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated,
        };
    }
}

internal record AssemblyCommandsGeneratingModel
{
    public required string Namespace { get; init; }

    public required INamedTypeSymbol AssemblyCommandHandlerType { get; init; }
}

internal static class CommandModelExtensions
{
    public static CommandPropertyType AsCommandPropertyType(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.SpecialType is SpecialType.System_Boolean)
        {
            return CommandPropertyType.Boolean;
        }

        if (typeSymbol.SpecialType is SpecialType.System_Byte or
            SpecialType.System_SByte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Decimal)
        {
            return CommandPropertyType.Number;
        }

        if (typeSymbol.TypeKind is TypeKind.Enum)
        {
            return CommandPropertyType.Enum;
        }

        if (typeSymbol.SpecialType is SpecialType.System_String)
        {
            return CommandPropertyType.String;
        }

        if (typeSymbol is INamedTypeSymbol
            {
                IsGenericType: true, TypeArguments:
                [
                    { SpecialType: SpecialType.System_String },
                ],
            } namedTypeSymbol)
        {
            var genericTypeName = namedTypeSymbol.ConstructUnboundGenericType().ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            if (genericTypeName
                is "System.Collections.Generic.IList<>"
                or "System.Collections.Generic.IReadOnlyList<>"
                or "System.Collections.Generic.ICollection<>"
                or "System.Collections.Generic.IReadOnlyCollection<>"
                or "System.Collections.Generic.IEnumerable<>"
                or "System.Collections.Immutable.ImmutableArray<>"
                or "System.Collections.Immutable.ImmutableHashSet<>"
                or "System.Collections.ObjectModel.Collection<>"
                or "System.Collections.Generic.List<>")
            {
                return CommandPropertyType.List;
            }
        }

        if (typeSymbol is INamedTypeSymbol
            {
                IsGenericType: true, TypeArguments:
                [
                    { SpecialType: SpecialType.System_String },
                    { SpecialType: SpecialType.System_String },
                ],
            } namedTypeSymbol2)
        {
            var genericTypeName = namedTypeSymbol2.ConstructUnboundGenericType().ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            if (genericTypeName
                is "System.Collections.Generic.IDictionary<,>"
                or "System.Collections.Generic.IReadOnlyDictionary<,>"
                or "System.Collections.Immutable.ImmutableDictionary<,>"
                or "System.Collections.Generic.Dictionary<,>"
                or "System.Collections.Generic.KeyValuePair<,>")
            {
                return CommandPropertyType.Dictionary;
            }
        }

        return CommandPropertyType.String;
    }
}

internal enum CommandPropertyType
{
    Boolean,
    Number,
    Enum,
    String,
    List,
    Dictionary,
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

    public static ImmutableArray<TModel> GetAttributedProperties<TModel>(this ITypeSymbol typeSymbol,
        Func<IPropertySymbol, TModel?> propertyParser)
        where TModel : class
    {
        return typeSymbol
            .EnumerateBaseTypesRecursively() // 递归获取所有基类
            .Reverse() // （注意我们先给父类属性赋值，再给子类属性赋值）
            .SelectMany(x => x.GetMembers()) //                 的所有成员，
            .OfType<IPropertySymbol>() //                             然后取出属性，
            .Select(x => (PropertyName: x.Name, Model: propertyParser(x))) // 解析出 OptionPropertyGeneratingModel。
            .Where(x => x.Model is not null)
            .GroupBy(x => x.PropertyName) // 按属性名去重。
            .Select(x => x.Last().Model) // 随后，取子类的属性（去除父类的重名属性）。
            .Cast<TModel>()
            .ToImmutableArray();
    }
}
