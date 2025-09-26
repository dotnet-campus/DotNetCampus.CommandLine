using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace DotNetCampus.Cli.Tests;

/// <summary>
/// 测试命令行参数解析器在类型继承场景下的行为。
/// </summary>
[TestClass]
public class InheritanceCommandLineParserTests
{
    private CommandLineParsingOptions Flexible { get; } = CommandLineParsingOptions.Flexible;

    #region 1. 基本继承测试
    [TestMethod("1.1. 父类可选属性可被正确赋值")]
    public void ParentOptionalProperty_Assigned()
    {
        // Arrange
        string[] args = ["--parent-value", "parent-test", "--child-value", "required-child-value"];
        string? parentValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<ChildWithOptionalParentProps>(o => parentValue = o.ParentValue)
            .Run();

        // Assert
        Assert.AreEqual("parent-test", parentValue);
    }

    [TestMethod("1.2. 子类必需属性可被正确赋值")]
    public void ChildRequiredProperty_Assigned()
    {
        // Arrange
        string[] args = ["--child-value", "child-test", "--parent-value", "parent-test"];
        string? childValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<ChildWithOptionalParentProps>(o => childValue = o.ChildValue)
            .Run();

        // Assert
        Assert.AreEqual("child-test", childValue);
    }

    [TestMethod("1.3. 父类和子类属性同时赋值")]
    public void ParentAndChild_BothPropertiesAssigned()
    {
        // Arrange
        string[] args = ["--parent-value", "parent-test", "--child-value", "child-test"];
        string? parentValue = null;
        string? childValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<ChildWithOptionalParentProps>(o =>
            {
                parentValue = o.ParentValue;
                childValue = o.ChildValue;
            })
            .Run();

        // Assert
        Assert.AreEqual("parent-test", parentValue);
        Assert.AreEqual("child-test", childValue);
    }

    [TestMethod("1.4. 子类可选，父类必需时，必须为父类属性赋值")]
    public void RequiredParentProperty_MustBeAssigned()
    {
        // Arrange
        string[] args = ["--parent-required", "parent-value", "--child-value", "child-test"];
        string? parentValue = null;
        string? childValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<ChildWithRequiredParentProps>(o =>
            {
                parentValue = o.ParentRequired;
                childValue = o.ChildValue;
            })
            .Run();

        // Assert
        Assert.AreEqual("parent-value", parentValue);
        Assert.AreEqual("child-test", childValue);
    }

    [TestMethod("1.5. 子类可选，父类必需时，不赋值父类属性会抛出异常")]
    public void RequiredParentProperty_ThrowsException_WhenNotAssigned()
    {
        // Arrange
        string[] args = ["--child-value", "child-test"];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, Flexible).ToRunner()
                .AddHandler<ChildWithRequiredParentProps>(_ => { })
                .Run();
        });
    }

    [TestMethod("1.6. 子类必需，父类可选时，只需为子类属性赋值")]
    public void RequiredChildProperty_OptionalParentProperty()
    {
        // Arrange
        string[] args = ["--child-required", "child-value"];
        string? childValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<ChildWithRequiredProps>(o => childValue = o.ChildRequired)
            .Run();

        // Assert
        Assert.AreEqual("child-value", childValue);
    }

    [TestMethod("1.7. 子类必需，父类可选时，不赋值子类属性会抛出异常")]
    public void RequiredChildProperty_ThrowsException_WhenNotAssigned()
    {
        // Arrange
        string[] args = ["--parent-value", "parent-test"];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, Flexible).ToRunner()
                .AddHandler<ChildWithRequiredProps>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 2. 多层次继承测试

    [TestMethod("2.1. 深层次继承关系中的属性都能被正确赋值")]
    public void DeepInheritance_AllPropertiesAssigned()
    {
        // Arrange
        string[] args = [
            "--grandparent-value", "grand-test",
            "--parent-value", "parent-test",
            "--child-value", "child-test"
        ];
        string? grandparentValue = null;
        string? parentValue = null;
        string? childValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<GrandChildOptions>(o =>
            {
                grandparentValue = o.GrandparentValue;
                parentValue = o.ParentValue;
                childValue = o.ChildValue;
            })
            .Run();

        // Assert
        Assert.AreEqual("grand-test", grandparentValue);
        Assert.AreEqual("parent-test", parentValue);
        Assert.AreEqual("child-test", childValue);
    }

    [TestMethod("2.2. 深层次继承关系中，必需属性必须被赋值")]
    public void DeepInheritance_RequiredProperties_MustBeAssigned()
    {
        // Arrange
        string[] args = [
            "--grandparent-required", "grand-required",
            "--parent-required", "parent-required",
            "--child-required", "child-required"
        ];
        string? grandparentRequired = null;
        string? parentRequired = null;
        string? childRequired = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<GrandChildWithRequiredProps>(o =>
            {
                grandparentRequired = o.GrandparentRequired;
                parentRequired = o.ParentRequired;
                childRequired = o.ChildRequired;
            })
            .Run();

        // Assert
        Assert.AreEqual("grand-required", grandparentRequired);
        Assert.AreEqual("parent-required", parentRequired);
        Assert.AreEqual("child-required", childRequired);
    }

    [TestMethod("2.3. 深层次继承关系中，缺少任何必需属性都会抛出异常")]
    public void DeepInheritance_MissingRequiredProperty_ThrowsException()
    {
        // Arrange - 缺少父类必需属性
        string[] args = [
            "--grandparent-required", "grand-required",
            "--child-required", "child-required"
        ];

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, Flexible).ToRunner()
                .AddHandler<GrandChildWithRequiredProps>(_ => { })
                .Run();
        });
    }

    #endregion

    #region 3. 属性覆盖测试

    [TestMethod("3.1. 父类和子类具有相同名称的属性时，子类属性优先")]
    public void SameName_ChildOverridesParent()
    {
        // Arrange
        string[] args = ["--same-name-value", "child-override"];
        string? sameNameValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<OverrideOptions>(o => sameNameValue = o.SameNameValue)
            .Run();

        // Assert
        Assert.AreEqual("child-override", sameNameValue);
    }

    [TestMethod("3.2. 父类和子类中具有不同类型但相同名称的属性时，正确解析")]
    public void DifferentType_SameName_CorrectlyParsed()
    {
        // Arrange
        string[] args = ["--type-value=123"];
        int? typeValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<DifferentTypeOptions>(o => typeValue = o.TypeValue)
            .Run();

        // Assert
        Assert.AreEqual(123, typeValue);
    }

    #endregion

    #region 4. 默认值测试

    [TestMethod("4.1. 父类属性有默认值时，子类实例能正确获取默认值")]
    public void ParentDefaultValues_InheritedByChild()
    {
        // Arrange
        string[] args = ["--child-value", "child-test"];
        string? defaultValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<ChildWithDefaultValues>(o => defaultValue = o.DefaultValue)
            .Run();

        // Assert
        Assert.AreEqual("default-value", defaultValue);
    }

    [TestMethod("4.2. 同名属性时，子类默认值覆盖父类默认值")]
    public void SameNameProperty_ChildDefaultOverridesParentDefault()
    {
        // Arrange
        string[] args = [];
        string? defaultValue = null;

        // Act
        CommandLine.Parse(args, Flexible).ToRunner()
            .AddHandler<ChildWithOverrideDefaultValues>(o => defaultValue = o.DefaultValue)
            .Run();

        // Assert
        Assert.AreEqual("child-default", defaultValue);
    }

    #endregion
}

#region 测试用数据模型

// 基础父类-可选属性
internal record ParentWithOptionalProps
{
    [Option("parent-value")]
    public string? ParentValue { get; init; }
}

// 带有子类必需属性的子类
internal record ChildWithOptionalParentProps : ParentWithOptionalProps
{
    [Option("child-value")]
    public required string ChildValue { get; init; }
}

// 基础父类-必需属性
internal record ParentWithRequiredProps
{
    [Option("parent-required")]
    public required string ParentRequired { get; init; }
}

// 必需父类属性 + 可选子类属性
internal record ChildWithRequiredParentProps : ParentWithRequiredProps
{
    [Option("child-value")]
    public string? ChildValue { get; init; }
}

// 可选父类属性 + 必需子类属性
internal record ChildWithRequiredProps : ParentWithOptionalProps
{
    [Option("child-required")]
    public required string ChildRequired { get; init; }
}

// 多层继承 - 所有可选
internal record ParentOptions : ParentWithOptionalProps
{
    [Option("parent-value")]
    public new string? ParentValue { get; init; }
}

internal record ChildOptions : ParentOptions
{
    [Option("child-value")]
    public string? ChildValue { get; init; }
}

internal record GrandChildOptions : ChildOptions
{
    [Option("grandparent-value")]
    public string? GrandparentValue { get; init; }
}

// 多层继承 - 所有必需
internal record GrandParentWithRequiredProps
{
    [Option("grandparent-required")]
    public required string GrandparentRequired { get; init; }
}

internal record ParentWithRequiredPropsFromGrandParent : GrandParentWithRequiredProps
{
    [Option("parent-required")]
    public required string ParentRequired { get; init; }
}

internal record GrandChildWithRequiredProps : ParentWithRequiredPropsFromGrandParent
{
    [Option("child-required")]
    public required string ChildRequired { get; init; }
}

// 属性覆盖测试
internal record ParentWithSameNameProp
{
    [Option("same-name-value")]
    public string? SameNameValue { get; init; } = "parent-default";
}

internal record OverrideOptions : ParentWithSameNameProp
{
    [Option("same-name-value")]
    public new required string SameNameValue { get; init; }
}

// 不同类型属性测试
internal record ParentWithStringType
{
    [Option("type-value")]
    public string? TypeValue { get; init; }
}

internal record DifferentTypeOptions : ParentWithStringType
{
    [Option("type-value")]
    public new required int TypeValue { get; init; }
}

// 默认值测试
internal record ParentWithDefaultValue
{
    [Option("default-value")]
    public string DefaultValue { get; set; } = "default-value";
}

internal record ChildWithDefaultValues : ParentWithDefaultValue
{
    [Option("child-value")]
    public string? ChildValue { get; init; }
}

internal record ParentWithDefaultValueToOverride
{
    [Option("default-value")]
    public string DefaultValue { get; set; } = "parent-default";
}

internal record ChildWithOverrideDefaultValues : ParentWithDefaultValueToOverride
{
    [Option("default-value")]
    public new string DefaultValue { get; set; } = "child-default";
}

#endregion
