using System.Text;
using System.Xml.Linq;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Regression coverage for #2660: "listextend" fields (such as the Switch on/off verbs on Core's
// switchable type) are kept apart from ordinary attributes, and were lost both by "Copy into game"
// and by the Editor's own save, so they also vanished when added by hand in the code view.
[TestClass]
public class ListExtendSaveTests
{
    private const string SwitchVerbs = "[SwitchOn];[SwitchOff]";

    // Templates are resolved when a game loads, so a save writes the English template text
    private const string SavedSwitchVerbs = "Switch on;Switch off";

    [TestMethod]
    public async Task TestCopyIntoGame_KeepsListExtendFields()
    {
        var controller = await LoadController(NewGameFile());

        controller.MakeElementLocal("switchable");
        var saved = controller.Save();
        controller.Uninitialise();

        AssertSwitchableVerbs(saved);
    }

    [TestMethod]
    public async Task TestEditorSave_KeepsListExtendFieldsWrittenInGame()
    {
        // As if added in the code view: a game-local type with its own listextend fields
        var game = XDocument.Parse(NewGameFile());
        game.Root!.Add(new XElement("type", new XAttribute("name", "switchable"),
            new XElement("displayverbs", new XAttribute("type", "listextend"), SwitchVerbs),
            new XElement("inventoryverbs", new XAttribute("type", "listextend"), SwitchVerbs)));

        var controller = await LoadController(game.ToString());
        var saved = controller.Save();
        controller.Uninitialise();

        AssertSwitchableVerbs(saved);

        // ...and a second save and reload keeps them too
        var reloaded = await LoadController(saved);
        var savedAgain = reloaded.Save();
        reloaded.Uninitialise();

        AssertSwitchableVerbs(savedAgain);
    }

    [TestMethod]
    public async Task TestEditorSave_DoesNotWriteInheritedListExtendFields()
    {
        // An object inheriting the library switchable type gets its verbs from the type,
        // so the object itself shouldn't gain a copy of them
        var game = XDocument.Parse(NewGameFile());
        game.Root!.Add(new XElement("object", new XAttribute("name", "lamp"),
            new XElement("inherit", new XAttribute("name", "switchable"))));

        var controller = await LoadController(game.ToString());
        var saved = controller.Save();
        controller.Uninitialise();

        var lamp = XDocument.Parse(saved).Descendants("object")
            .Single(o => (string?) o.Attribute("name") == "lamp");
        Assert.IsNull(lamp.Element("displayverbs"));
        Assert.IsNull(lamp.Element("inventoryverbs"));
    }

    private static void AssertSwitchableVerbs(string saved)
    {
        var type = XDocument.Parse(saved).Root!.Elements("type")
            .Single(t => (string?) t.Attribute("name") == "switchable");

        foreach (var attribute in new[] { "displayverbs", "inventoryverbs" })
        {
            var field = type.Element(attribute);
            Assert.IsNotNull(field, $"switchable should keep its {attribute}");
            Assert.AreEqual("listextend", (string?) field.Attribute("type"));
            Assert.AreEqual(SavedSwitchVerbs, field.Value);
        }
    }

    private static string NewGameFile()
    {
        var template = EditorController.GetAvailableTemplates().Values.Single(t => t.TemplateName == "English");
        return EditorController.CreateNewGameFile(template.ResourceName, "Test");
    }

    private static async Task<EditorController> LoadController(string gameFile)
    {
        var controller = new EditorController();
        controller.ClearTree += (_, _) => { };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, _) => { };
        controller.RemovedNode += (_, _) => { };
        controller.RenamedNode += (_, _) => { };
        controller.RetitledNode += (_, _) => { };
        controller.ElementsUpdated += (_, _) => { };
        controller.Dirty += (_, _) => { };

        var ok = await controller.Initialise(new ByteArrayGameDataProvider(Encoding.UTF8.GetBytes(gameFile), "test.aslx"));
        Assert.IsTrue(ok);

        controller.UpdateTree();

        return controller;
    }
}
