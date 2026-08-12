using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Regression coverage for the Exits editor's destination dropdown incorrectly offering dialogue
// Pages as exit targets (see CorePages.aslx / EditorController.CreateNewPage). EditorController.IsRoom
// is the helper WasmEditorBridge.GetExitsData now filters on, so rooms - and only rooms - are
// mirrored here directly since GetExitsData itself lives in the browser-wasm-only WasmEditor project.
[TestClass]
public class EditorControllerRoomsAndPagesTests
{
    [TestMethod]
    public async Task TestIsRoom()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewObject("anObject", "game", "An Object");
        controller.CreateNewPage("aPage", "game", "A Page");

        Assert.IsTrue(controller.IsRoom("aRoom"));
        Assert.IsFalse(controller.IsRoom("anObject"));
        Assert.IsFalse(controller.IsRoom("aPage"), "A dialogue Page should not be considered a room");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestIsDialoguePage()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewObject("anObject", "game", "An Object");
        controller.CreateNewPage("aPage", "game", "A Page");

        Assert.IsFalse(controller.IsDialoguePage("aRoom"));
        Assert.IsFalse(controller.IsDialoguePage("anObject"));
        Assert.IsTrue(controller.IsDialoguePage("aPage"));

        controller.Uninitialise();
    }

    private static async Task<EditorController> LoadTemplateController(string templateName)
    {
        var templates = EditorController.GetAvailableTemplates();
        var template = templates.Values.Single(t => t.TemplateName == templateName);
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
        var bytes = Encoding.UTF8.GetBytes(initialFileText);

        var controller = new EditorController();
        // Mirrors IncludedLibraryTests.LoadTemplateController / WasmEditorBridge.AttachControllerEvents -
        // several of these are invoked unconditionally during element creation, so every one needs a
        // subscriber once the controller is fully initialised.
        controller.ClearTree += (_, _) => { };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, _) => { };
        controller.RemovedNode += (_, _) => { };
        controller.RenamedNode += (_, _) => { };
        controller.RetitledNode += (_, _) => { };
        controller.ElementsUpdated += (_, _) => { };
        controller.Dirty += (_, _) => { };

        var ok = await controller.Initialise(new ByteArrayGameDataProvider(bytes, "test.aslx"));
        Assert.IsTrue(ok, $"Initialisation failed for template '{templateName}'");

        controller.UpdateTree();

        return controller;
    }
}
