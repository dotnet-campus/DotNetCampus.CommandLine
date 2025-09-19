using DotNetCampus.Cli.Temp40.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.Utils;

[TestClass]
public class NamingHelperTests
{
    #region MakePascalCase Tests

    [TestMethod("测试小写字符串转换为PascalCase")]
    public void MakePascalCase_LowerCase_ShouldReturnPascalCase()
    {
        // Arrange
        var oldName = "oldname";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("Oldname", newName);
    }

    [TestMethod("测试PascalCase字符串转换")]
    public void MakePascalCase_AlreadyPascalCase_ShouldReturnSame()
    {
        // Arrange
        var oldName = "OldName";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("OldName", newName);
    }

    [TestMethod("测试全大写字符串转换为PascalCase")]
    public void MakePascalCase_UpperCase_ShouldReturnPascalCase()
    {
        // Arrange
        var oldName = "OLDNAME";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("OLDNAME", newName);
    }

    [TestMethod("测试snake_case字符串转换为PascalCase")]
    public void MakePascalCase_SnakeCase_ShouldReturnPascalCase()
    {
        // Arrange
        var oldName = "old_name";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("OldName", newName);
    }

    [TestMethod("测试kebab-case字符串转换为PascalCase")]
    public void MakePascalCase_KebabCase_ShouldReturnPascalCase()
    {
        // Arrange
        var oldName = "old-name";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("OldName", newName);
    }

    [TestMethod("测试camelCase字符串转换为PascalCase")]
    public void MakePascalCase_CamelCase_ShouldReturnPascalCase()
    {
        // Arrange
        var oldName = "oldName";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("OldName", newName);
    }

    [TestMethod("测试包含数字的字符串转换为PascalCase")]
    public void MakePascalCase_WithNumbers_ShouldReturnPascalCase()
    {
        // Arrange
        var oldName = "old123name";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("Old123Name", newName);
    }

    [TestMethod("测试特殊字符的字符串转换为PascalCase")]
    public void MakePascalCase_WithSpecialChars_ShouldReturnPascalCase()
    {
        // Arrange
        var oldName = "old@name#test";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("OldNameTest", newName);
    }

    [TestMethod("测试空字符串转换为PascalCase")]
    public void MakePascalCase_EmptyString_ShouldReturnEmpty()
    {
        // Arrange
        var oldName = "";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("", newName);
    }

    [TestMethod("测试单字符字符串转换为PascalCase")]
    public void MakePascalCase_SingleChar_ShouldReturnPascalCase()
    {
        // Arrange
        var oldName = "a";

        // Act
        var newName = NamingHelper.MakePascalCase(oldName);

        // Assert
        Assert.AreEqual("A", newName);
    }

    #endregion

    #region MakeKebabCase Tests

    [TestMethod("测试PascalCase字符串转换为kebab-case")]
    public void MakeKebabCase_PascalCase_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "OldName";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("old-name", newName);
    }

    [TestMethod("测试camelCase字符串转换为kebab-case")]
    public void MakeKebabCase_CamelCase_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "oldName";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("old-name", newName);
    }

    [TestMethod("测试全大写字符串转换为kebab-case")]
    public void MakeKebabCase_UpperCase_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "OLDNAME";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("oldname", newName);
    }

    [TestMethod("测试snake_case字符串转换为kebab-case")]
    public void MakeKebabCase_SnakeCase_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "old_name";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("old-name", newName);
    }

    [TestMethod("测试已是kebab-case字符串的转换")]
    public void MakeKebabCase_AlreadyKebabCase_ShouldReturnSame()
    {
        // Arrange
        var oldName = "old-name";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("old-name", newName);
    }

    [TestMethod("测试isUpperSeparator=false参数的转换")]
    public void MakeKebabCase_WithoutUpperSeparator_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "OldName";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName, false);

        // Assert
        Assert.AreEqual("oldname", newName);
    }

    [TestMethod("测试toLower=false参数的转换")]
    public void MakeKebabCase_WithoutLowerCase_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "OldName";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName, true, false);

        // Assert
        Assert.AreEqual("Old-Name", newName);
    }

    [TestMethod("测试包含数字的字符串转换为kebab-case")]
    public void MakeKebabCase_WithNumbers_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "Old123Name";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("old123-name", newName);
    }

    [TestMethod("测试特殊字符的字符串转换为kebab-case")]
    public void MakeKebabCase_WithSpecialChars_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "Old@Name#Test";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("old-name-test", newName);
    }

    [TestMethod("测试空字符串转换为kebab-case")]
    public void MakeKebabCase_EmptyString_ShouldReturnEmpty()
    {
        // Arrange
        var oldName = "";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("", newName);
    }

    [TestMethod("测试单字符字符串转换为kebab-case")]
    public void MakeKebabCase_SingleChar_ShouldReturnSingleChar()
    {
        // Arrange
        var oldName = "A";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("a", newName);
    }

    [Ignore("连续大写的情况目前很难区分。如果后续有更好的方法区分了，可以再支持。")]
    [TestMethod("测试连续大写字母的处理")]
    public void MakeKebabCase_ConsecutiveUpperCase_ShouldReturnKebabCase()
    {
        // Arrange
        var oldName = "HTTPRequest";

        // Act
        var newName = NamingHelper.MakeKebabCase(oldName);

        // Assert
        Assert.AreEqual("http-request", newName);
    }

    #endregion

    #region CheckIsPascalCase Tests

    [TestMethod("测试检查标准PascalCase字符串")]
    public void CheckIsPascalCase_WithPascalCase_ShouldReturnTrue()
    {
        // Arrange
        var name = "PascalCaseName";

        // Act
        var result = NamingHelper.CheckIsPascalCase(name);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod("测试检查非PascalCase字符串")]
    public void CheckIsPascalCase_WithNonPascalCase_ShouldReturnFalse()
    {
        // Arrange
        var names = new[] { "camelCaseName", "kebab-case", "snake_case", "lowercase" /*, "UPPERCASE"*/ };

        foreach (var name in names)
        {
            // Act
            var result = NamingHelper.CheckIsPascalCase(name);

            // Assert
            Assert.IsFalse(result, $"The string '{name}' should not be recognized as PascalCase.");
        }
    }

    #endregion

    #region CheckIsKebabCase Tests

    [TestMethod("测试检查标准kebab-case字符串")]
    public void CheckIsKebabCase_WithKebabCase_ShouldReturnTrue()
    {
        // Arrange
        var name = "kebab-case-name";

        // Act
        var result = NamingHelper.CheckIsKebabCase(name);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod("测试检查非kebab-case字符串")]
    public void CheckIsKebabCase_WithNonKebabCase_ShouldReturnFalse()
    {
        // Arrange
        var names = new[] { "PascalCase", "camelCase", "snake_case", "UPPERCASE", "Invalid-Kebab-Case", "invalid--kebabcase" };

        foreach (var name in names)
        {
            // Act
            var result = NamingHelper.CheckIsKebabCase(name);

            // Assert
            Assert.IsFalse(result, $"The string '{name}' should not be recognized as kebab-case.");
        }
    }

    #endregion
}
