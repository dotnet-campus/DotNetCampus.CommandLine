using System.Collections;
using System.Collections.Generic;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class DefaultValueTests
{
    [TestMethod]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Windows, DisplayName = "[Windows]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void Required_ThrowsException(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() => commandLine.As<OptionsWithRequired>());
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() => commandLine.As<OptionsWithRequiredInit>());
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() => commandLine.As<OptionsWithRequiredCollection>());
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() => commandLine.As<OptionsWithRequiredInitCollection>());
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() => commandLine.As<OptionsWithNullableRequired>());
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() => commandLine.As<OptionsWithNullableRequiredInit>());
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() => commandLine.As<OptionsWithNullableRequiredCollection>());
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() => commandLine.As<OptionsWithNullableRequiredInitCollection>());
    }

    [TestMethod]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Windows, DisplayName = "[Windows]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void WithoutInit_KeepsDefaultValue(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var options = commandLine.As<Options>();
        var optionsWithNullable = commandLine.As<OptionsWithNullable>();
        var optionsWithCollection = commandLine.As<OptionsWithCollection>();
        var optionsWithNullableCollection = commandLine.As<OptionsWithNullableCollection>();

        // Assert
        Assert.AreEqual("Default", options.Option);
        Assert.AreEqual("Default", optionsWithNullable.Option);
        CollectionAssert.AreEqual(new[] { "Default" }, (ICollection)optionsWithCollection.Option);
        CollectionAssert.AreEqual(new[] { "Default" }, (ICollection)optionsWithNullableCollection.Option!);
    }

    [TestMethod]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Windows, DisplayName = "[Windows]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void InitNullable_AssignsNull(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var optionsWithNullableInit = commandLine.As<OptionsWithNullableInit>();
        var optionsWithNullableInitCollection = commandLine.As<OptionsWithNullableInitCollection>();
        var optionsWithInitNullableValueType = commandLine.As<OptionsWithInitNullableValueType>();

        // Assert
        Assert.IsNull(optionsWithNullableInit.Option);
        Assert.IsNull(optionsWithNullableInitCollection.Option);
        Assert.IsNull(optionsWithInitNullableValueType.Option);
    }

    [TestMethod]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Windows, DisplayName = "[Windows]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void InitCollection_Empty(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var optionsWithCollection = commandLine.As<OptionsWithInitCollection>();

        // Assert
        Assert.IsEmpty(optionsWithCollection.Option);
    }

    [TestMethod]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Windows, DisplayName = "[Windows]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void InitString_Empty(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var optionsWithInit = commandLine.As<OptionsWithInit>();

        // Assert
        Assert.IsEmpty(optionsWithInit.Option);
    }

    [TestMethod]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Windows, DisplayName = "[Windows]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Windows, DisplayName = "[Windows] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void InitValueType_Default(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var optionsWithInitStruct = commandLine.As<OptionsWithInitValueType>();

        // Assert
        Assert.AreEqual(0, optionsWithInitStruct.Option);
    }

    public record OptionsWithRequired
    {
        [Option('o', "option")]
        public required string Option { get; set; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithRequiredInit
    {
        [Option('o', "option")]
        public required string Option { get; init; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithRequiredCollection
    {
        [Option('o', "option")]
        public required IReadOnlyList<string> Option { get; set; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithRequiredInitCollection
    {
        [Option('o', "option")]
        public required IReadOnlyList<string> Option { get; init; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithNullableRequired
    {
        [Option('o', "option")]
        public required string? Option { get; set; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithNullableRequiredInit
    {
        [Option('o', "option")]
        public required string? Option { get; init; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithNullableRequiredCollection
    {
        [Option('o', "option")]
        public required IReadOnlyList<string>? Option { get; set; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithNullableRequiredInitCollection
    {
        [Option('o', "option")]
        public required IReadOnlyList<string>? Option { get; init; }

        [Value(0)]
        public string? Value { get; set; }
    }

    public record Options
    {
        [Option('o', "option")]
        public string Option { get; set; } = "Default";

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithInit
    {
        [Option('o', "option")]
        public string Option { get; init; } = "Default";

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithCollection
    {
        [Option('o', "option")]
        public IReadOnlyList<string> Option { get; set; } = ["Default"];

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithInitCollection
    {
        [Option('o', "option")]
        public IReadOnlyList<string> Option { get; init; } = ["Default"];

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithNullable
    {
        [Option('o', "option")]
        public string? Option { get; set; } = "Default";

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithNullableInit
    {
        [Option('o', "option")]
        public string? Option { get; init; } = "Default";

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithNullableCollection
    {
        [Option('o', "option")]
        public IReadOnlyList<string>? Option { get; set; } = ["Default"];

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithNullableInitCollection
    {
        [Option('o', "option")]
        public IReadOnlyList<string>? Option { get; init; } = ["Default"];

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithInitNullableValueType
    {
        [Option('o', "option")]
        public int? Option { get; init; } = 42;

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithInitValueType
    {
        [Option('o', "option")]
        public int Option { get; init; } = 42;

        [Value(0)]
        public string? Value { get; set; }
    }
}
