using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Which tree elements can be copied, and where a copy can be pasted. Copy follows Quest 5's
// CanCopy (#2586 asked for functions, which Viva's tree only offered for rooms/objects/pages);
// Paste only goes where the clipboard's element type belongs, since a copied object used to
// be pasteable onto any non-object node and land as a new top-level room.
[TestClass]
public class CopyPasteRulesTests
{
    [TestMethod]
    public async Task TestCanCopy_FollowsQuest5Rules()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("myFunction");
        controller.CreateNewTimer("myTimer");
        controller.CreateNewRoom("aRoom", "game", "A Room");
        var exit = controller.CreateNewExit("aRoom");
        var command = controller.CreateNewCommand("aRoom");
        var javascript = controller.CreateNewJavascript("test.js");

        foreach (var key in new[] { "myFunction", "myTimer", "aRoom", exit, command, "player" })
        {
            Assert.IsTrue(controller.CanCopy(key), $"'{key}' should be copyable");
        }

        Assert.IsFalse(controller.CanCopy("game"));
        Assert.IsFalse(controller.CanCopy(javascript));
        Assert.IsFalse(controller.CanCopy("noSuchElement"));

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestCopiedFunction_PastesOntoFunctionsOnly_AsAUniquelyNamedCopy()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("template");
        controller.CreateNewFunction("other");
        controller.CreateNewRoom("aRoom", "game", "A Room");

        controller.CopyElements(["template"]);

        Assert.IsTrue(controller.CanPaste("_functions"), "the Functions header");
        Assert.IsTrue(controller.CanPaste("other"), "another function");
        Assert.IsFalse(controller.CanPaste("aRoom"), "a room");
        Assert.IsFalse(controller.CanPaste("_objects"), "the Objects header");
        Assert.IsFalse(controller.CanPaste("_timers"), "the Timers header");

        var pasted = controller.PasteElements("_functions");
        Assert.IsNotNull(pasted);
        Assert.AreNotEqual("template", pasted);
        Assert.IsTrue(controller.ElementExists("template"), "copying must leave the original in place");
        Assert.IsTrue(controller.ElementExists(pasted));

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestCopiedObject_DoesNotPasteOntoNonObjectNodes()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("myFunction");
        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewRoom("anotherRoom", "game", "Another Room");
        var exit = controller.CreateNewExit("anotherRoom");

        controller.CopyElements(["aRoom"]);

        Assert.IsTrue(controller.CanPaste("anotherRoom"));
        Assert.IsTrue(controller.CanPaste("_objects"));
        Assert.IsFalse(controller.CanPaste("myFunction"), "a function");
        Assert.IsFalse(controller.CanPaste("_functions"), "the Functions header");
        Assert.IsFalse(controller.CanPaste(exit), "an exit, which can't contain objects");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestCopiedExit_PastesIntoAnotherRoom()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewRoom("anotherRoom", "game", "Another Room");
        var exit = controller.CreateNewExit("aRoom");

        controller.CopyElements([exit]);

        Assert.IsTrue(controller.CanPaste("anotherRoom"));
        var pasted = controller.PasteElements("anotherRoom");
        Assert.IsNotNull(pasted);
        Assert.IsTrue(controller.ElementExists(exit), "copying must leave the original in place");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestGamebook_PlayerCannotBeCopied_PagesPasteOntoPages()
    {
        var controller = await LoadTemplateController("Gamebook");
        controller.CreateNewPage("page9", null, null);
        controller.CreateNewFunction("myFunction");

        Assert.IsFalse(controller.CanCopy("player"));
        Assert.IsTrue(controller.CanCopy("page9"));

        controller.CopyElements(["page9"]);
        Assert.IsTrue(controller.CanPaste("_objects"), "the Pages header");
        Assert.IsTrue(controller.CanPaste("page9"), "a page");
        Assert.IsFalse(controller.CanPaste("myFunction"), "a function");

        controller.Uninitialise();
    }

    // Mirrors CutPasteUndoTests' helper of the same name - see FunctionFolderTests' file-level
    // comment for why EditorControllerTestBase can't be used for element-creating tests like these.
    private static async Task<EditorController> LoadTemplateController(string templateName)
    {
        var templates = EditorController.GetAvailableTemplates();
        var template = templates.Values.Single(t => t.TemplateName == templateName);
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
        var bytes = Encoding.UTF8.GetBytes(initialFileText);

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

        var ok = await controller.Initialise(new GameData(bytes, "test.aslx"));
        Assert.IsTrue(ok, $"Initialisation failed for template '{templateName}'");

        controller.UpdateTree();

        return controller;
    }
}
