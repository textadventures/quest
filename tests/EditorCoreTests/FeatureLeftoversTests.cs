using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// A feature checkbox ("Container", "Run an initialisation script", the game's "Ask/Tell") only
// shows or hides the feature's tab. Turning it off leaves whatever the author set there on the
// object, still taking effect in the game (#1586). EditorController.GetFeatureLeftovers finds
// those settings so the editor can offer to remove them, and a feature tab stays visible while
// its element still has any.
[TestClass]
public class FeatureLeftoversTests
{
    private EditorController _controller = null!;

    [TestInitialize]
    public async Task Init()
    {
        _controller = await LoadTemplateController("English");
        _controller.CreateNewObject("thing", null, "Thing");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _controller.Uninitialise();
    }

    [TestMethod]
    public async Task NothingLeftWhenNothingWasSet()
    {
        Data("thing").SetAttribute("feature_container", true);

        Assert.AreEqual(0, _controller.GetFeatureLeftovers("thing", "feature_container").Count);
        Data("thing").SetAttribute("feature_container", false);
        Assert.IsFalse(await FindTab("thing", "feature_container").IsTabVisible(Data("thing")));
    }

    [TestMethod]
    public async Task InitialiseScriptIsALeftover()
    {
        _controller.CreateNewEditableScripts("thing", "_initialise_", "msg", false);

        var leftovers = _controller.GetFeatureLeftovers("thing", "feature_startscript").Single();
        CollectionAssert.AreEqual(new[] {"_initialise_"}, leftovers.Attributes.ToArray());

        var tab = FindTab("thing", "feature_startscript");
        Assert.IsTrue(await tab.IsTabVisible(Data("thing")));
        Assert.IsTrue(await tab.IsTabVisibleOnlyForLeftovers(Data("thing")));

        Data("thing").SetAttribute("feature_startscript", true);
        Assert.IsFalse(await tab.IsTabVisibleOnlyForLeftovers(Data("thing")));
    }

    [TestMethod]
    public async Task ContainerTypeIsALeftover()
    {
        _controller.AddInheritedTypeToElement("thing", "container_open", false);
        Data("thing").SetAttribute("transparent", true);

        var leftovers = _controller.GetFeatureLeftovers("thing", "feature_container").Single();
        CollectionAssert.AreEqual(new[] {"container_open"}, leftovers.Types.ToArray());
        CollectionAssert.AreEqual(new[] {"transparent"}, leftovers.Attributes.ToArray());
        Assert.IsTrue(leftovers.Labels.Contains("Container type: Container"),
            $"Labels should use the editor's captions, got: {string.Join(" | ", leftovers.Labels)}");

        Assert.IsTrue(await FindTab("thing", "feature_container").IsTabVisibleOnlyForLeftovers(Data("thing")));
    }

    [TestMethod]
    public async Task RemovingLeftoversIsOneUndoStep()
    {
        Data("thing").SetAttribute("feature_container", true);
        _controller.AddInheritedTypeToElement("thing", "container_open", false);
        Data("thing").SetAttribute("transparent", true);
        var undoCountBefore = _controller.GetUndoItems().Count();

        _controller.StartTransaction("Turn off feature_container");
        _controller.RemoveFeatureLeftovers(_controller.GetFeatureLeftovers("thing", "feature_container"));
        Data("thing").SetAttribute("feature_container", false);
        _controller.EndTransaction();

        Assert.IsFalse(_controller.DoesElementInheritType("thing", "container_open"));
        Assert.AreEqual(0, _controller.GetFeatureLeftovers("thing", "feature_container").Count);
        Assert.AreEqual(undoCountBefore + 1, _controller.GetUndoItems().Count());

        await _controller.Undo();

        Assert.IsTrue(_controller.DoesElementInheritType("thing", "container_open"));
        Assert.AreEqual(true, Data("thing").GetAttribute("transparent"));
        Assert.AreEqual(true, Data("thing").GetAttribute("feature_container"));
    }

    [TestMethod]
    public void GameWideFeatureCoversEveryObject()
    {
        _controller.CreateNewObject("other", null, "Other");
        _controller.CreateNewEditableScripts("thing", "askdefault", "msg", false);
        _controller.CreateNewEditableScripts("other", "telldefault", "msg", false);

        var leftovers = _controller.GetFeatureLeftovers("game", "feature_asktell");
        CollectionAssert.AreEquivalent(new[] {"thing", "other"}, leftovers.Select(l => l.ElementKey).ToArray());

        var justThing = _controller.GetFeatureLeftovers("thing", "feature_asktell", onlyThisElement: true);
        Assert.AreEqual("thing", justThing.Single().ElementKey);

        // An object-level lookup never reaches a game-wide feature's tabs.
        Assert.AreEqual(0, _controller.GetFeatureLeftovers("thing", "feature_asktell").Count);
    }

    [TestMethod]
    public void EditorOnlyFeatureHasNoLeftovers()
    {
        // Annotations are notes for the author, so leaving them behind is harmless.
        Data("thing").SetAttribute("implementation_notes", "Remember to add a key");

        Assert.AreEqual(0, _controller.GetFeatureLeftovers("game", "feature_annotations").Count);
    }

    [TestMethod]
    public void TypeInheritedThroughAnotherTypeIsNotALeftover()
    {
        // The object is a container because its "chest" type is: that's the type's doing, and
        // removing container_open from the object itself wouldn't change anything.
        _controller.CreateNewType("chest");
        _controller.AddInheritedTypeToElement("chest", "container_open", false);
        _controller.AddInheritedTypeToElement("thing", "chest", false);

        Assert.AreEqual(0, _controller.GetFeatureLeftovers("thing", "feature_container").Count);
    }

    private IEditorData Data(string key)
    {
        return _controller.GetEditorData(key)!;
    }

    private IEditorTab FindTab(string key, string featureAttribute)
    {
        var definition = _controller.GetEditorDefinition(_controller.GetElementEditorName(key)!);
        return definition.Tabs.Values.First(t => t.FeatureAttribute == featureAttribute);
    }

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
