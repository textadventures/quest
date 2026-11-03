using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// What the editor sends a running Preview (#2650 part 5): the plain values of the game's Display
// and Interface tab attributes, the tabs marked <liveinterface/>.
[TestClass]
public class LiveInterfaceAttributesTests
{
    private static async Task<EditorController> LoadController(string templateName)
    {
        var template = EditorController.GetAvailableTemplates().Values.Single(t => t.TemplateName == templateName);
        var bytes = Encoding.UTF8.GetBytes(EditorController.CreateNewGameFile(template.ResourceName, "Test"));

        var controller = new EditorController();
        controller.ClearTree += (_, _) => { };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, _) => { };
        Assert.IsTrue(await controller.Initialise(new GameData(bytes, "test.aslx")), $"Initialisation failed for '{templateName}'");
        return controller;
    }

    [TestMethod]
    public async Task CoversTheDisplayAndInterfaceTabs_AndNothingElse()
    {
        using var controller = await LoadController("English");
        var attributes = controller.GetLiveInterfaceAttributes();

        foreach (var name in new[] { "accentcolour", "interfacefont", "defaultforeground", "layoutpreset", "showpanes", "stylesheet", "customwidth" })
        {
            Assert.IsTrue(attributes.ContainsKey(name), $"{name} is missing");
        }

        // The Setup tab's, and the game's scripts, aren't interface settings.
        Assert.IsFalse(attributes.ContainsKey("gamename"));
        Assert.IsFalse(attributes.ContainsKey("inituserinterface"));
    }

    [TestMethod]
    public async Task GivesEachValue_IncludingInheritedOnes()
    {
        using var controller = await LoadController("English");
        var game = controller.GetEditorData("game")!;
        game.SetAttribute("layoutpreset", "bottombar");
        game.SetAttribute("stylesheet", "p { color: teal; }");

        var attributes = controller.GetLiveInterfaceAttributes();

        Assert.AreEqual("bottombar", attributes["layoutpreset"]);
        Assert.AreEqual("p { color: teal; }", attributes["stylesheet"]);
        Assert.IsInstanceOfType<bool>(attributes["showpanes"]);
        Assert.IsInstanceOfType<int>(attributes["customwidth"]);
    }

    [TestMethod]
    public async Task AGamebookHasItsDisplayTab()
    {
        using var controller = await LoadController("Gamebook");
        Assert.IsTrue(controller.GetLiveInterfaceAttributes().ContainsKey("defaultforeground"));
    }
}
