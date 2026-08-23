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

    // Coverage for the ported-but-previously-unexposed v5 desktop editor method that scopes the
    // "Add Object here" create modal's parent picker to just the target object's own ancestor
    // chain (root-first), rather than every object in the game (that's GetMovePossibleParents,
    // used by "Move to" instead).
    [TestMethod]
    public async Task TestGetPossibleNewObjectParentsForCurrentSelection_NestedObject_ReturnsAncestorChainRootFirst()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewObject("box", "game", "A Box");
        controller.CreateNewObject("innerThing", "box", "Inner Thing");

        var parents = controller.GetPossibleNewObjectParentsForCurrentSelection("innerThing").ToArray();

        CollectionAssert.AreEqual(new[] { "game", "box", "innerThing" }, parents);

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestGetPossibleNewObjectParentsForCurrentSelection_Room_ReturnsAncestorChainToo()
    {
        // A Room is ObjectType.Object underneath (see the ObjectType enum - Room/Object/Page are
        // all just inherited-type distinctions layered over the same core Object type, IsRoom's
        // own comment above says as much), so the ancestor-chain method returns a real chain for
        // a Room just as it does for a plain Object - "Add Object here" from a Room can offer a
        // parent picker too (e.g. to create the new object as a top-level sibling instead).
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");

        var parents = controller.GetPossibleNewObjectParentsForCurrentSelection("aRoom").ToArray();

        CollectionAssert.AreEqual(new[] { "game", "aRoom" }, parents);

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
