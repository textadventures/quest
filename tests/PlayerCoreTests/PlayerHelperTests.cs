using QuestViva.PlayerCore;

namespace QuestViva.PlayerCoreTests;

[TestClass]
public class PlayerHelperTests
{
    [TestMethod]
    [DataRow("style.css", "text/css")]
    [DataRow("data.json", "application/json")]
    [DataRow("notes.txt", "text/plain")]
    [DataRow("font.woff", "font/woff")]
    [DataRow("font.woff2", "font/woff2")]
    [DataRow("icon.svg", "image/svg+xml")]
    [DataRow("script.js", "application/javascript")]
    [DataRow("photo.jpg", "image/jpeg")]
    [DataRow("STYLE.CSS", "text/css")]
    public void TestGetContentType(string filename, string expectedContentType)
    {
        Assert.AreEqual(expectedContentType, PlayerHelper.GetContentType(filename));
    }

    [TestMethod]
    public void TestGetContentTypeUnknownExtension()
    {
        Assert.AreEqual("", PlayerHelper.GetContentType("archive.zip"));
    }
}
