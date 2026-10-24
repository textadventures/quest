using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// The game's stylesheet (game.stylesheet) is edited as CSS on the game's Interface tab: it has to
// come back from each save exactly as written, line breaks and all, without gaining whitespace.
[TestClass]
public class GameStylesheetTests : EditorControllerTestBase
{
    private const string Css = "[data-qv-part=\"output\"] > p {\n    color: teal;\n}\n\nqv-player { --qv-link: #a0522d; }";

    [TestMethod]
    public async Task TheStylesheetSurvivesSavingTwice()
    {
        Controller.GetEditorData("game")!.SetAttribute("stylesheet", Css);
        var xml = Controller.Save();

        using var reloaded = new EditorController();
        Assert.IsTrue(await reloaded.Initialise(new GameData(System.Text.Encoding.UTF8.GetBytes(xml), "test.aslx")));
        var xml2 = reloaded.Save();
        using var reloaded2 = new EditorController();
        Assert.IsTrue(await reloaded2.Initialise(new GameData(System.Text.Encoding.UTF8.GetBytes(xml2), "test.aslx")));
        Assert.AreEqual(Css, reloaded2.GetEditorData("game")!.GetAttribute("stylesheet"));
    }
}
