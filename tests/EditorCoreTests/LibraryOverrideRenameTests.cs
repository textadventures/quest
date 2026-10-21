using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Regression coverage for #2555: "Copy into game" on a library function and then renaming the copy
// used to leave the library original gone from the live model (and the tree) until the game was
// closed and reopened, since the copy was the original element with its library flag cleared.
// The copy is now a separate element that displaces the original, which Elements remembers and
// restores when the copy is renamed, matching what a save and reload produces.
[TestClass]
public class LibraryOverrideRenameTests
{
    private const string LibraryFunction = "WearGarment";

    [TestMethod]
    public async Task TestRenamingLocalCopy_LeavesLibraryOriginalInPlace()
    {
        var (controller, nodes) = await LoadController();

        controller.MakeElementLocal(LibraryFunction);
        Assert.IsFalse(IsLibrary(controller, LibraryFunction), "The copy should be a local element");

        Rename(controller, LibraryFunction, "NPCWearGarment");

        Assert.IsTrue(controller.ElementExists("NPCWearGarment"));
        Assert.IsTrue(controller.ElementExists(LibraryFunction), "The library original should still exist");
        Assert.IsTrue(IsLibrary(controller, LibraryFunction));
        Assert.IsFalse(IsLibrary(controller, "NPCWearGarment"));
        CollectionAssert.Contains(nodes, LibraryFunction);
        CollectionAssert.Contains(nodes, "NPCWearGarment");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestUndoRename_ShadowsLibraryOriginalAgain()
    {
        var (controller, nodes) = await LoadController();

        controller.MakeElementLocal(LibraryFunction);
        Rename(controller, LibraryFunction, "NPCWearGarment");

        await controller.Undo();

        Assert.IsFalse(controller.ElementExists("NPCWearGarment"));
        Assert.IsTrue(controller.ElementExists(LibraryFunction));
        Assert.IsFalse(IsLibrary(controller, LibraryFunction), "The local copy should shadow the original again");
        Assert.AreEqual(1, nodes.Count(n => n == LibraryFunction));

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task TestUndoMakeLocal_RestoresLibraryOriginal()
    {
        var (controller, _) = await LoadController();

        controller.MakeElementLocal(LibraryFunction);
        await controller.Undo();

        Assert.IsTrue(controller.ElementExists(LibraryFunction));
        Assert.IsTrue(IsLibrary(controller, LibraryFunction));

        controller.Uninitialise();
    }

    private static bool IsLibrary(EditorController controller, string key)
    {
        return ((IEditorDataExtendedAttributeInfo) controller.GetEditorData(key)!).IsLibraryElement;
    }

    private static void Rename(EditorController controller, string key, string newName)
    {
        controller.StartTransaction("Set name");
        var result = controller.GetEditorData(key)!.SetAttribute("name", newName);
        controller.EndTransaction();
        Assert.IsTrue(result.Valid);
    }

    // The tree is simulated as the set of node keys the controller has told the UI about,
    // upserting on AddedNode as the real consumers do.
    private static async Task<(EditorController, List<string>)> LoadController()
    {
        var template = EditorController.GetAvailableTemplates().Values.Single(t => t.TemplateName == "English");
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
        var bytes = Encoding.UTF8.GetBytes(initialFileText);

        var nodes = new List<string>();
        var controller = new EditorController();
        var filters = new FilterOptions();
        filters.Set("libraries", true);
        controller.UpdateFilterOptions(filters);
        controller.ClearTree += (_, _) => nodes.Clear();
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, e) =>
        {
            if (!nodes.Contains(e.Key)) nodes.Add(e.Key);
        };
        controller.RemovedNode += (_, e) => nodes.Remove(e.Key);
        controller.RenamedNode += (_, e) =>
        {
            var i = nodes.IndexOf(e.OldName);
            if (i >= 0) nodes[i] = e.NewName;
        };
        controller.RetitledNode += (_, _) => { };
        controller.ElementsUpdated += (_, _) => { };
        controller.Dirty += (_, _) => { };

        var ok = await controller.Initialise(new GameData(bytes, "test.aslx"));
        Assert.IsTrue(ok);

        controller.UpdateTree();

        return (controller, nodes);
    }
}
