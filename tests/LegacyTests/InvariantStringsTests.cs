using System.Globalization;
using QuestViva.Legacy;

namespace QuestViva.LegacyTests;

[TestClass]
public class InvariantStringsTests
{
    // ru-RU and el-GR have non-1252 ANSI code pages, so VB's Chr/Asc map bytes above 127 differently there
    [TestMethod]
    [DataRow("en-US")]
    [DataRow("sv-SE")]
    [DataRow("ru-RU")]
    [DataRow("el-GR")]
    public void TestIndependentOfCulture(string culture)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);

            Assert.AreEqual('ý', InvariantStrings.Chr(253));
            Assert.AreEqual('þ', InvariantStrings.Chr(254));
            Assert.AreEqual('–', InvariantStrings.Chr(150));
            Assert.AreEqual('A', InvariantStrings.Chr(65));
            Assert.AreEqual(253, InvariantStrings.Asc("ýx"));
            Assert.AreEqual(150, InvariantStrings.Asc("–"));

            for (var i = 0; i <= 255; i++)
            {
                Assert.AreEqual(i, InvariantStrings.Asc(InvariantStrings.Chr(i).ToString()));
            }

            Assert.AreEqual(" 5", InvariantStrings.Str(5));
            Assert.AreEqual(" 0", InvariantStrings.Str(0));
            Assert.AreEqual(" 100.4", InvariantStrings.Str(100.4));
            Assert.AreEqual("-10.5", InvariantStrings.Str(-10.5));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
