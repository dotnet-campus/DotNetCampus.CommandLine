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
    [DataRow(new string[] { }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value")]
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
    [DataRow(new string[] { }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value")]
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
    [DataRow(new string[] { }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void InitCollection_AlwaysNotNullEmpty(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var optionsWithInitCollection = commandLine.As<OptionsWithInitCollection>();
        var optionsWithNullableInitCollection = commandLine.As<OptionsWithNullableInitCollection>();

        // Assert
        Assert.IsNotNull(optionsWithInitCollection.Option);
        Assert.IsNotNull(optionsWithNullableInitCollection.Option);
        Assert.IsEmpty(optionsWithInitCollection.Option);
        Assert.IsEmpty(optionsWithNullableInitCollection.Option);
    }

    [TestMethod]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void NullableInitStruct_Null(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var optionsWithInitNullableStruct = commandLine.As<OptionsWithInitNullableStruct>();

        // Assert
        Assert.IsNull(optionsWithInitNullableStruct.Option);
    }

    [TestMethod]
    [DataRow(new string[] { }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible]")]
    [DataRow(new string[] { }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet]")]
    [DataRow(new string[] { }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu]")]
    [DataRow(new string[] { }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell]")]
    [DataRow(new[] { "test://" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Flexible, DisplayName = "[Flexible] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.DotNet, DisplayName = "[DotNet] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.Gnu, DisplayName = "[Gnu] value")]
    [DataRow(new[] { "value" }, TestCommandLineStyle.PowerShell, DisplayName = "[PowerShell] value")]
    [DataRow(new[] { "test://value" }, TestCommandLineStyle.Url, DisplayName = "[Url] test://value")]
    public void InitStruct_Default(string[] args, TestCommandLineStyle style)
    {
        // Arrange
        var commandLine = CommandLine.Parse(args, style.ToParsingOptions());

        // Act
        var optionsWithInitStruct = commandLine.As<OptionsWithInitStruct>();

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

    public record OptionsWithInitNullableStruct
    {
        [Option('o', "option")]
        public int? Option { get; init; } = 42;

        [Value(0)]
        public string? Value { get; set; }
    }

    public record OptionsWithInitStruct
    {
        [Option('o', "option")]
        public int Option { get; init; } = 42;

        [Value(0)]
        public string? Value { get; set; }
    }
}
