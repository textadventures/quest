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

    [TestMethod]
    public async Task CanMoveFunctionUpDown_False_ForOutsiderSteppingIntoMultiMemberFolder()
    {
        // Reproduces the reported bug: an ungrouped function sitting right next to a 2-member
        // folder - moving it towards the folder would swap it with the folder's near edge,
        // landing it directly between the folder's two functions and splitting the folder.
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.CreateNewFunction("f3");
        controller.SetFunctionFolder("f2", "Group");
        controller.SetFunctionFolder("f3", "Group");
        controller.CreateNewFunction("f4");
        // Final order: f1(none), f2(Group), f3(Group), f4(none).

        Assert.IsFalse(controller.CanMoveFunctionDown("f1"), "f1 would step down into the Group folder");
        Assert.IsFalse(controller.CanMoveFunctionUp("f4"), "f4 would step up into the Group folder");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task CanMoveFunctionUpDown_False_ForMultiMemberFolderMemberSteppingOut()
    {
        // The other direction of the same bug: a 2+-member folder's own member stepping out via
        // a plain arrow swap doesn't clear its folder field, so it would keep carrying that
        // folder's name to a disconnected spot - splitting the folder just as visibly as an
        // outsider stepping in. Leaving a folder is done via "Move to folder" instead.
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.CreateNewFunction("f3");
        controller.SetFunctionFolder("f2", "Group");
        controller.SetFunctionFolder("f3", "Group");
        controller.CreateNewFunction("f4");
        // Final order: f1(none), f2(Group), f3(Group), f4(none).

        Assert.IsFalse(controller.CanMoveFunctionUp("f2"), "f2 would step out of Group towards f1");
        Assert.IsFalse(controller.CanMoveFunctionDown("f3"), "f3 would step out of Group towards f4");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task CanMoveFunctionDown_True_WhenNeighbourFolderHasOnlyOneMember()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.SetFunctionFolder("f2", "Solo");

        // Nothing to split with only one member in "Solo", so a plain swap is fine.
        Assert.IsTrue(controller.CanMoveFunctionDown("f1"));

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task CanMoveFunctionUpDown_True_WithinSameFolder()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.SetFunctionFolder("f1", "Group");
        controller.SetFunctionFolder("f2", "Group");

        Assert.IsTrue(controller.CanMoveFunctionDown("f1"));
        Assert.IsTrue(controller.CanMoveFunctionUp("f2"));

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task CanMoveFunctionUpDown_False_AtListBoundary()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");

        // The template's built-in library functions precede anything user-created, so the very
        // first function overall is one of those, not "f1" - query it rather than assume.
        var firstFunction = controller.WorldModel.Elements.GetElements(ElementType.Function)
            .OrderBy(e => e.MetaFields[MetaFieldDefinitions.SortIndex])
            .First();

        Assert.IsFalse(controller.CanMoveFunctionUp(firstFunction.Name), "the very first function has nothing above it");
        Assert.IsFalse(controller.CanMoveFunctionDown("f2"), "f2 is already last");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task CanMoveFunctionFolderUpDown_FalseAtBoundaries_TrueOtherwise()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.CreateNewFunction("f3");
        controller.SetFunctionFolder("f2", "Group");
        // Final order: f1(none), f2(Group), f3(none) - Group is the very last thing in the list.

        Assert.IsTrue(controller.CanMoveFunctionFolderUp("Group"), "f1 sits before the folder");
        Assert.IsFalse(controller.CanMoveFunctionFolderDown("Group"), "nothing follows the folder");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task MoveFunctionFolderUp_SwapsWholeBlockPastLoneFunction_PreservingMemberOrder()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.CreateNewFunction("f3");
        controller.SetFunctionFolder("f2", "Group");
        controller.SetFunctionFolder("f3", "Group");
        // Final order: f1(none), f2(Group), f3(Group).

        controller.MoveFunctionFolderUp("Group");

        var order = new[] { "f1", "f2", "f3" }
            .Select(name => GetSortIndex(controller, name))
            .ToArray();
        Assert.IsTrue(order[1] < order[0], "the Group block should now precede f1");
        Assert.IsTrue(order[1] < order[2], "f2 should stay before f3 within the Group block");
        Assert.AreEqual("Group", GetFolder(controller, "f2"));
        Assert.AreEqual("Group", GetFolder(controller, "f3"));

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task MoveFunctionFolderDown_SwapsWholeBlockPastAnotherFolder_PreservingBothBlocksOrder()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("a1");
        controller.CreateNewFunction("a2");
        controller.CreateNewFunction("b1");
        controller.CreateNewFunction("b2");
        controller.SetFunctionFolder("a1", "Alpha");
        controller.SetFunctionFolder("a2", "Alpha");
        controller.SetFunctionFolder("b1", "Beta");
        controller.SetFunctionFolder("b2", "Beta");
        // Final order: a1(Alpha), a2(Alpha), b1(Beta), b2(Beta).

        controller.MoveFunctionFolderDown("Alpha");

        var order = new[] { "a1", "a2", "b1", "b2" }
            .Select(name => GetSortIndex(controller, name))
            .ToArray();
        Assert.IsTrue(order[2] < order[0], "Beta block should now precede Alpha block");
        Assert.IsTrue(order[2] < order[3], "b1 should stay before b2 within Beta");
        Assert.IsTrue(order[0] < order[1], "a1 should stay before a2 within Alpha");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task MoveFunctionFolder_DoesNotTouchLibraryFunctionSortIndexes()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.CreateNewFunction("f3");
        controller.SetFunctionFolder("f2", "Group");
        controller.SetFunctionFolder("f3", "Group");

        var libraryFunctions = controller.WorldModel.Elements.GetElements(ElementType.Function)
            .Where(e => e.MetaFields[MetaFieldDefinitions.Library])
            .ToList();
        var beforeSortIndexes = libraryFunctions.ToDictionary(e => e.Name, e => e.MetaFields[MetaFieldDefinitions.SortIndex]);

        controller.MoveFunctionFolderUp("Group");

        foreach (var function in libraryFunctions)
        {
            Assert.AreEqual(beforeSortIndexes[function.Name], function.MetaFields[MetaFieldDefinitions.SortIndex],
                $"Library function '{function.Name}' should not have been reordered");
        }

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task MoveFunctionFolderUp_UndoRedo_RestoresPosition()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("f1");
        controller.CreateNewFunction("f2");
        controller.SetFunctionFolder("f2", "Group");

        var before = GetSortIndex(controller, "f1") < GetSortIndex(controller, "f2");
        Assert.IsTrue(before, "f1 should start before the Group folder");

        controller.MoveFunctionFolderUp("Group");
        Assert.IsTrue(GetSortIndex(controller, "f2") < GetSortIndex(controller, "f1"), "Group should now precede f1");

        await controller.Undo();
        Assert.IsTrue(GetSortIndex(controller, "f1") < GetSortIndex(controller, "f2"), "Undo should restore f1 before Group");

        controller.Redo();
        Assert.IsTrue(GetSortIndex(controller, "f2") < GetSortIndex(controller, "f1"), "Redo should reapply the move");

        controller.Uninitialise();
    }

    [TestMethod]
    public async Task UpdateTree_ReflectsSortIndexOrder_AfterFolderReassignmentInterleavesUntouchedFunction()
    {
        // Regression test: UpdateTree()'s full-rebuild traversal used to walk elements in raw
        // creation/storage order rather than SortIndex order - fine normally since the two
        // coincide, but SetFunctionFolder (and MoveFunctionFolderUp/Down) explicitly reorders
        // functions by adjusting SortIndex alone, which a full rebuild must reflect or the
        // reordered elements silently revert to their pre-reorder display position. Reproduces
        // the exact pattern that exposed it: an untouched function (Gamma) created between two
        // reassigned ones (Alpha, Beta), then a brand new one (Delta) created and assigned
        // afterwards.
        var controller = await LoadTemplateController("English");
        controller.CreateNewFunction("Alpha");
        controller.CreateNewFunction("Beta");
        controller.CreateNewFunction("Gamma");
        controller.SetFunctionFolder("Alpha", "Math");
        controller.SetFunctionFolder("Beta", "Math");
        controller.CreateNewFunction("Delta");
        controller.SetFunctionFolder("Delta", "Math");

        var addedOrder = new List<string>();
        controller.AddedNode += (_, e) => addedOrder.Add(e.Key);
        controller.UpdateTree();

        var userFunctionOrder = addedOrder.Where(k => k is "Alpha" or "Beta" or "Gamma" or "Delta").ToArray();
        CollectionAssert.AreEqual(new[] { "Gamma", "Alpha", "Beta", "Delta" }, userFunctionOrder);

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
