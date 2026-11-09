using System.Text;
using System.Xml.Linq;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Undoing a deletion used to add the element back at the end of its type's list, so the saved game
// file listed it somewhere else. For <include>s that changes the order libraries load in.
[TestClass]
public class UndoDeleteOrderTests
{
    [TestMethod]
    public async Task UndoDeletingAnIncludedLibrary_PutsItBackInItsPlace()
    {
        var controller = await LoadTemplateController();
        var a = controller.CreateNewIncludedLibrary("a.aslx");
        var b = controller.CreateNewIncludedLibrary("b.aslx");
        controller.CreateNewIncludedLibrary("c.aslx");
        var before = Includes(controller);
        CollectionAssert.IsSubsetOf(new[] { "a.aslx", "b.aslx", "c.aslx" }, before);

        controller.DeleteElement(b, true);
        CollectionAssert.DoesNotContain(Includes(controller), "b.aslx");

        await controller.Undo();
        CollectionAssert.AreEqual(before, Includes(controller));

        controller.DeleteElement(a, true);
        await controller.Undo();
        CollectionAssert.AreEqual(before, Includes(controller));

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task UndoDeletingFunctions_PutsThemBackInTheirPlace()
    {
        var controller = await LoadTemplateController();
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.CreateNewFunction("f3");
        controller.CreateNewFunction("f4");
        var before = Names(controller, "function");
        CollectionAssert.AreEqual(new[] { "f1", "f2", "f3", "f4" }, before);

        // Two deletions in one transaction, undone together
        controller.StartTransaction("Delete two");
        controller.DeleteElement("f3", false);
        controller.DeleteElement("f1", false);
        controller.EndTransaction();
        CollectionAssert.AreEqual(new[] { "f2", "f4" }, Names(controller, "function"));

        await controller.Undo();
        CollectionAssert.AreEqual(before, Names(controller, "function"));

        controller.Uninitialise();
    }

    // Objects are saved in tree (SortIndex) order, so this passed before too. It checks that
    // restoring the children, which were deleted first and so come back last, still works.
    [TestMethod]
    public async Task UndoDeletingARoomWithChildren_PutsEverythingBackInItsPlace()
    {
        var controller = await LoadTemplateController();
        controller.CreateNewRoom("r1", null, null);
        controller.CreateNewRoom("r2", null, null);
        controller.CreateNewObject("o1", "r2", null);
        controller.CreateNewObject("o2", "r2", null);
        controller.CreateNewObject("o3", "r1", null);
        controller.CreateNewRoom("r3", null, null);
        var before = Names(controller, "object");

        controller.DeleteElement("r2", true);
        CollectionAssert.DoesNotContain(Names(controller, "object"), "o1");

        await controller.Undo();
        CollectionAssert.AreEqual(before, Names(controller, "object"));

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task DeleteUndoRedoUndo_KeepsTheOriginalOrder()
    {
        var controller = await LoadTemplateController();
        controller.CreateNewIncludedLibrary("a.aslx");
        var b = controller.CreateNewIncludedLibrary("b.aslx");
        controller.CreateNewIncludedLibrary("c.aslx");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.CreateNewFunction("f3");
        var includes = Includes(controller);
        var functions = Names(controller, "function");

        controller.DeleteElement(b, true);
        controller.DeleteElement("f2", true);
        var afterDelete = (Includes(controller), Names(controller, "function"));

        await controller.Undo();
        await controller.Undo();
        CollectionAssert.AreEqual(includes, Includes(controller));
        CollectionAssert.AreEqual(functions, Names(controller, "function"));

        controller.Redo();
        controller.Redo();
        CollectionAssert.AreEqual(afterDelete.Item1, Includes(controller));
        CollectionAssert.AreEqual(afterDelete.Item2, Names(controller, "function"));

        await controller.Undo();
        await controller.Undo();
        CollectionAssert.AreEqual(includes, Includes(controller));
        CollectionAssert.AreEqual(functions, Names(controller, "function"));

        controller.Uninitialise();
    }

    // Redoing a creation that was undone after later elements were added and their creation undone
    // too: each comes back where it was.
    [TestMethod]
    public async Task UndoThenRedoCreatingElements_KeepsTheirOrder()
    {
        var controller = await LoadTemplateController();
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.CreateNewFunction("f3");
        var before = Names(controller, "function");

        await controller.Undo();
        await controller.Undo();
        CollectionAssert.AreEqual(new[] { "f1" }, Names(controller, "function"));

        controller.Redo();
        controller.Redo();
        CollectionAssert.AreEqual(before, Names(controller, "function"));

        controller.Uninitialise();
    }

    private static List<string> Includes(EditorController controller) =>
        XDocument.Parse(controller.Save()).Root!.Elements("include")
            .Select(e => (string)e.Attribute("ref")!).ToList();

    private static List<string> Names(EditorController controller, string elementName) =>
        XDocument.Parse(controller.Save()).Root!.Descendants(elementName)
            .Select(e => (string)e.Attribute("name")!).ToList();

    private static async Task<EditorController> LoadTemplateController()
    {
        var templates = EditorController.GetAvailableTemplates();
        var template = templates.Values.Single(t => t.TemplateName == "English");
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
        var bytes = Encoding.UTF8.GetBytes(initialFileText);

        var controller = new EditorController();
        // Some of these events are invoked without a null check, so each needs a subscriber, as in
        // WasmEditorBridge.AttachControllerEvents.
        controller.ClearTree += (_, _) => { };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, _) => { };
        controller.RemovedNode += (_, _) => { };
        controller.RenamedNode += (_, _) => { };
        controller.RetitledNode += (_, _) => { };
        controller.ElementsUpdated += (_, _) => { };
        controller.Dirty += (_, _) => { };

        Assert.IsTrue(await controller.Initialise(new GameData(bytes, "test.aslx")));
        controller.UpdateTree();
        return controller;
    }
}
