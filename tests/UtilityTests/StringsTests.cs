using QuestViva.Utility;
using TextAdventures.Utility;

namespace UtilityTests;

[TestClass]
public sealed class StringsTests
{
    [TestMethod]
    public void TestCapFirst()
    {
        Assert.AreEqual("Test", Strings.CapFirst("test"));
        Assert.AreEqual("T", Strings.CapFirst("t"));
        Assert.AreEqual(string.Empty, Strings.CapFirst(string.Empty));
        Assert.AreEqual(null, Strings.CapFirst(null));
    }
}