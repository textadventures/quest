using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Regression coverage for a Discord report: cutting a room and immediately hitting Undo appeared
// to do nothing (or undo something unrelated), and the room stayed stuck in its dimmed "cut"
// state until a full Paste. Root cause: EditorController.CutElements only staged the clipboard -
// it recorded no IUndoAction, so UndoLogger.EndTransaction silently dropped the empty "Cut"
// transaction, and Undo fell through to whatever real transaction preceded the cut instead of
// cancelling it. See CutClipboardUndoAction / EditorController.GetCutElementKeys.
[TestClass]
public class CutPasteUndoTests
{
    [TestMethod]
    public async Task TestCut_IsItsOwnUndoStep_AndDoesNotUndoAnUnrelatedPriorChange()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");
        CollectionAssert.Contains(controller.GetUndoItems().ToArray(), "Create room 'aRoom'");

        controller.CutElements(["aRoom"]);
        CollectionAssert.AreEqual(new[] { "aRoom" }, controller.GetCutElementKeys().ToArray());

        // Before the fix, this Undo would fall through past the (empty, never-recorded) "Cut"
        // transaction and undo the room's own creation instead, destroying it.
        await controller.Undo();

        Assert.IsTrue(controller.ElementExists("aRoom"), "Undo right after Cut should only cancel the cut, not the room's creation");
        CollectionAssert.AreEqual(Array.Empty<string>(), controller.GetCutElementKeys().ToArray());

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestCut_Undo_Redo_ThenPaste_RestoresCutStateAndCompletes()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewRoom("anotherRoom", "game", "Another Room");

        controller.CutElements(["aRoom"]);
        await controller.Undo();
        CollectionAssert.AreEqual(Array.Empty<string>(), controller.GetCutElementKeys().ToArray());

        controller.Redo();
        CollectionAssert.AreEqual(new[] { "aRoom" }, controller.GetCutElementKeys().ToArray());

        Assert.IsTrue(controller.CanPaste("anotherRoom"));
        var pasted = controller.PasteElements("anotherRoom");
        Assert.AreEqual("aRoom", pasted);
        CollectionAssert.AreEqual(Array.Empty<string>(), controller.GetCutElementKeys().ToArray());

        controller.Uninitialise();
    }

    // Regression coverage for a follow-up Discord/user report: with the Cut-is-now-undoable fix
    // above in place, undoing a completed cut-paste was found to delete the pasted element
    // outright instead of restoring it. Root cause: Paste completes a cut by cloning with the
    // *same name* as the cut element (Element.Clone's lastelementscutout branch), so
    // Elements.Add silently displaces (evicts, doesn't destroy) the original under that name -
    // but only the newly created clone's creation was ever logged as an undo action, so Undo
    // freed the name by destroying the clone and left nothing to put back. Fixed in
    // ElementFactoryBase.CreateInternal, which now also logs the displaced element's eviction.
    [TestMethod]
    public async Task TestCut_Paste_ThenUndo_RestoresTheElement()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewRoom("anotherRoom", "game", "Another Room");

        controller.CutElements(["aRoom"]);
        var pasted = controller.PasteElements("anotherRoom");
        Assert.AreEqual("aRoom", pasted);
        Assert.IsTrue(controller.ElementExists("aRoom"));

        await controller.Undo();

        Assert.IsTrue(controller.ElementExists("aRoom"), "Undoing a cut-paste should restore the element, not delete it outright");

        controller.Uninitialise();
    }

    // Regression coverage for a further follow-up report: after the fixes above, Cut -> Paste ->
    // Undo correctly restored the element, but it no longer showed as "cut" (dimmed/italic) even
    // though undoing the Paste should land you back in exactly the state right after the Cut -
    // element cut, not yet pasted. Root cause: PasteElements cleared _lastelementscutout to false
    // as a plain field write after EndTransaction, so nothing ever logged that change as undoable
    // - Undoing "Paste" reversed the element creation but left the flag (and so
    // GetCutElementKeys) stuck at "not cut". A second Undo (undoing "Cut" itself, via
    // CutClipboardUndoAction) should then clear it for real.
    [TestMethod]
    public async Task TestCut_Paste_ThenUndo_ShowsElementAsCutAgain_AndSecondUndoClearsIt()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewRoom("anotherRoom", "game", "Another Room");

        controller.CutElements(["aRoom"]);
        controller.PasteElements("anotherRoom");
        CollectionAssert.AreEqual(Array.Empty<string>(), controller.GetCutElementKeys().ToArray());

        await controller.Undo(); // undoes "Paste"

        Assert.IsTrue(controller.ElementExists("aRoom"));
        CollectionAssert.AreEqual(new[] { "aRoom" }, controller.GetCutElementKeys().ToArray(),
            "Undoing the Paste should return to the 'cut, not yet pasted' state, so the element shows as cut again");

        await controller.Undo(); // undoes "Cut"

        CollectionAssert.AreEqual(Array.Empty<string>(), controller.GetCutElementKeys().ToArray(),
            "Undoing the Cut itself should clear the cut state");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestCut_Paste_Undo_Redo_ReappliesThePaste()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewRoom("anotherRoom", "game", "Another Room");

        controller.CutElements(["aRoom"]);
        controller.PasteElements("anotherRoom");
        await controller.Undo();
        controller.Redo();

        Assert.IsTrue(controller.ElementExists("aRoom"), "Redo should re-apply the paste, not leave the element missing");

        controller.Uninitialise();
    }

    // The displaced-element bug applies recursively too: Element.Clone passes the same
    // lastelementscutout flag down when it clones a cut element's children, so a child of a cut
    // room is displaced under its own original name as well when the room is pasted.
    [TestMethod]
    public async Task TestCut_PasteRoomWithChildObject_ThenUndo_RestoresBoth()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewRoom("aRoom", "game", "A Room");
        controller.CreateNewObject("innerThing", "aRoom", "Inner Thing");
        controller.CreateNewRoom("anotherRoom", "game", "Another Room");

        controller.CutElements(["aRoom"]);
        controller.PasteElements("anotherRoom");
        Assert.IsTrue(controller.ElementExists("aRoom"));
        Assert.IsTrue(controller.ElementExists("innerThing"));

        await controller.Undo();

        Assert.IsTrue(controller.ElementExists("aRoom"), "Undo should restore the cut room");
        Assert.IsTrue(controller.ElementExists("innerThing"), "Undo should also restore the cut room's child object, not just the room itself");

        controller.Uninitialise();
    }

    // Mirrors EditorControllerRoomsAndPagesTests/IncludedLibraryTests/FunctionFolderTests' own
    // helper of the same name - see FunctionFolderTests' file-level comment for why
    // EditorControllerTestBase can't be used for element-creating tests like this one.
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

        var ok = await controller.Initialise(new ByteArrayGameDataProvider(bytes, "test.aslx"));
        Assert.IsTrue(ok, $"Initialisation failed for template '{templateName}'");

        controller.UpdateTree();

        return controller;
    }
}
