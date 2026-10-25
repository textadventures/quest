using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// A string attribute with line breaks in it has to come back from each save exactly as it was
// written. The game writer used to pad it with the element's indentation, so that the closing
// tag lined up, and the padding became part of the value, so it grew by an indent each save.
[TestClass]
public class MultilineStringSaveTests : EditorControllerTestBase
{
    private const string Text = "First line\n    indented line\n\nlast line";

    private static async Task<EditorController> Reload(string xml)
    {
        var controller = new EditorController();
        Assert.IsTrue(await controller.Initialise(new GameData(System.Text.Encoding.UTF8.GetBytes(xml), "test.aslx")));
        return controller;
    }

    [TestMethod]
    public async Task AMultilineStringSurvivesSavingTwice()
    {
        Controller.GetEditorData("game")!.SetAttribute("notes", Text);

        using var once = await Reload(Controller.Save());
        using var twice = await Reload(once.Save());

        Assert.AreEqual(Text, twice.GetEditorData("game")!.GetAttribute("notes"));
    }
}
