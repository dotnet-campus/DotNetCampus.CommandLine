using DotNetCampus.Cli.Compiler;
using DotNetCampus.CommandLine.Generators.Models;
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
                    .GetAttributedProperties(OptionalArgumentPropertyGeneratingModel.TryParse);
                // 5. 拥有 [Value] 特性的属性。
                var valueProperties = typeSymbol
                    .GetAttributedProperties(PositionalArgumentPropertyGeneratingModel.TryParse);
                // 6. 拥有 [RawArguments] 特性的属性。
                var rawArgumentsProperties = typeSymbol
                    .GetAttributedProperties(RawArgumentPropertyGeneratingModel.TryParse);

                if (!isOptions && !isHandler && attribute is null
                    && optionProperties.Count is 0 && valueProperties.Count is 0 && rawArgumentsProperties.Count is 0)
                {
                    // 不是命令行选项类型。
                    return null;
                }

                var @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
                var commandNames = attribute?.ConstructorArguments.FirstOrDefault().Value?.ToString();
                var useFullStackParser = attribute?.NamedArguments
                    .FirstOrDefault(kv => kv.Key == "ExperimentalUseFullStackParser").Value.Value as bool? ?? false;
                var isPublic = typeSymbol.DeclaredAccessibility == Accessibility.Public;

                for (var i = 0; i < optionProperties.Count; i++)
                {
                    optionProperties[i].PropertyIndex = i;
                }
                for (var i = 0; i < valueProperties.Count; i++)
                {
                    valueProperties[i].PropertyIndex = i + optionProperties.Count;
                }

                return new CommandObjectGeneratingModel
                {
                    Namespace = @namespace,
                    CommandObjectType = typeSymbol,
                    UseFullStackParser = useFullStackParser,
                    IsPublic = isPublic,
                    CommandNames = commandNames,
                    IsHandler = isHandler,
                    OptionProperties = optionProperties,
                    PositionalArgumentProperties = valueProperties,
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

file static class Extensions
{
    public static IReadOnlyList<TModel> GetAttributedProperties<TModel>(this ITypeSymbol typeSymbol,
        Func<IPropertySymbol, TModel?> propertyParser)
        where TModel : class
    {
        return typeSymbol
            .EnumerateBaseTypesRecursively() // 递归获取所有基类
            .Reverse() // （注意我们先给父类属性赋值，再给子类属性赋值）
            .SelectMany(x => x.GetMembers()) // 的所有成员，
            .OfType<IPropertySymbol>() // 然后取出属性，
            .Select(x => (PropertyName: x.Name, Model: propertyParser(x))) // 解析出 OptionPropertyGeneratingModel。
            .Where(x => x.Model is not null)
            .GroupBy(x => x.PropertyName) // 按属性名去重。
            .Select(x => x.Last().Model) // 随后，取子类的属性（去除父类的重名属性）。
            .Cast<TModel>()
            .ToList();
    }

    private static IEnumerable<ITypeSymbol> EnumerateBaseTypesRecursively(this ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }
}
