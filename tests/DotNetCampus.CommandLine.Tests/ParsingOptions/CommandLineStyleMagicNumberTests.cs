using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingOptions;

[TestClass]
public class CommandLineStyleMagicNumberTests
{
#if DEBUG
    [TestMethod("魔法数字必须严格和实际样式匹配")]
    public void MagicNumber_MustMatchRealStyle()
    {
        CommandLineParsingOptions.VerifyMagicNumbers();
    }
#endif
}
