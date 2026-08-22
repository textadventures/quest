using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;
using QuestViva.Engine;

namespace QuestViva.EditorCoreTests;

// Coverage for the user-assigned Function "folder" feature (issue #2103's remaining ask, after
// #2113 added read-only library-filename grouping): EditorController.GetFunctionFolders /
// SetFunctionFolder. Doesn't use EditorControllerTestBase - SetFunctionFolder relies on the same
// MetaFields["sortindex"] change notification path as SwapElements (see
// EditorController.m_worldModel_ElementMetaFieldUpdated), which fires RemovedNode then a fresh
// AddedNode for every reordered element; EditorControllerTestBase's EditorTreeData.Add throws on
// a duplicate key and the base class never subscribes to RemovedNode at all, so exercising any
// reorder through it would crash. LoadTemplateController mirrors
// EditorControllerRoomsAndPagesTests/IncludedLibraryTests' own helper of the same name instead.
[TestClass]
public class FunctionFolderTests
{
    [TestMethod]
    public async Task SetFunctionFolder_AssignsAndGroupsAtEndOfFolder()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewFunction("Alpha");
        controller.CreateNewFunction("Beta");
        controller.CreateNewFunction("Gamma");

        CollectionAssert.AreEqual(Array.Empty<string>(), controller.GetFunctionFolders().ToArray());

        controller.SetFunctionFolder("Alpha", "Math");
        CollectionAssert.AreEqual(new[] { "Math" }, controller.GetFunctionFolders().ToArray());
        Assert.AreEqual("Math", GetFolder(controller, "Alpha"));

        controller.SetFunctionFolder("Beta", "Math");
        Assert.AreEqual("Math", GetFolder(controller, "Beta"));

        // Assigning to an existing folder relocates the function to sit immediately after the
        // folder's current last member, so the two Math functions end up contiguous by SortIndex
        // regardless of Gamma (never assigned a folder) sitting between them beforehand.
        var order = new[] { "Gamma", "Alpha", "Beta" }
            .Select(name => GetSortIndex(controller, name))
            .ToArray();
        Assert.IsTrue(order[0] < order[1], "Gamma should stay before the Math folder");
        Assert.IsTrue(order[1] < order[2], "Alpha should stay before Beta within the Math folder");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task SetFunctionFolder_DoesNotTouchLibraryFunctionSortIndexes()
    {
        // Regression test: SetFunctionFolder used to renumber the SortIndex of every Function
        // element from scratch, including every built-in library function (there can be hundreds
        // across Core.aslx/English.aslx/etc.) - each a needless write that triggered a full tree
        // reposition event, causing a multi-second UI freeze. It must now only touch the small
        // slice of user-authored functions actually shifted by the move.
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("Alpha");
        controller.CreateNewFunction("Beta");

        var libraryFunctions = controller.WorldModel.Elements.GetElements(ElementType.Function)
            .Where(e => e.MetaFields[MetaFieldDefinitions.Library])
            .ToList();
        Assert.IsTrue(libraryFunctions.Count > 0, "Expected the English template to include built-in library functions");
        var beforeSortIndexes = libraryFunctions.ToDictionary(e => e.Name, e => e.MetaFields[MetaFieldDefinitions.SortIndex]);

        controller.SetFunctionFolder("Alpha", "Math");
        controller.SetFunctionFolder("Beta", "Math");

        foreach (var function in libraryFunctions)
        {
            Assert.AreEqual(beforeSortIndexes[function.Name], function.MetaFields[MetaFieldDefinitions.SortIndex],
                $"Library function '{function.Name}' should not have been reordered");
        }

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task SetFunctionFolder_NewFolderName_AddsItToGetFunctionFolders()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("Alpha");

        controller.SetFunctionFolder("Alpha", "Brand New Folder");

        CollectionAssert.AreEqual(new[] { "Brand New Folder" }, controller.GetFunctionFolders().ToArray());

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task SetFunctionFolder_EmptyString_ClearsFolder()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("Alpha");
        controller.SetFunctionFolder("Alpha", "Math");

        controller.SetFunctionFolder("Alpha", "");

        Assert.IsTrue(string.IsNullOrEmpty(GetFolder(controller, "Alpha")));
        CollectionAssert.AreEqual(Array.Empty<string>(), controller.GetFunctionFolders().ToArray());

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task SetFunctionFolder_UndoRedo_RestoresFolder()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("Alpha");

        controller.SetFunctionFolder("Alpha", "Math");
        Assert.AreEqual("Math", GetFolder(controller, "Alpha"));

        await controller.Undo();
        Assert.IsTrue(string.IsNullOrEmpty(GetFolder(controller, "Alpha")), "Undo should clear the folder again");

        controller.Redo();
        Assert.AreEqual("Math", GetFolder(controller, "Alpha"), "Redo should restore the folder");

        controller.Uninitialise();
    }

    private static string GetFolder(EditorController controller, string key) =>
        controller.WorldModel.Elements.Get(key).Fields[FieldDefinitions.EditorFolder];

    private static int GetSortIndex(EditorController controller, string key) =>
        controller.WorldModel.Elements.Get(key).MetaFields[MetaFieldDefinitions.SortIndex];

    // Mirrors EditorControllerRoomsAndPagesTests.LoadTemplateController / IncludedLibraryTests'
    // own helper of the same name - every controller event needs a subscriber since several fire
    // unconditionally during element creation/reordering.
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
