using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

[TestClass]
public class IncludedLibraryTests
{
    // Every engine-shipped library (Core.aslx / GamebookCore.aslx, and the per-language file such
    // as English.aslx) leaves the game unable to load if its <include> is removed — see
    // EditorController.CanDelete / WorldModel.IsBuiltInLibrary. Loads a real game from each shipped
    // template (rather than the shared test.aslx fixture, whose <include> lines are commented out)
    // so the libraries are genuinely present, matching TemplateTests' approach.
    [TestMethod]
    public async Task TestCannotDeleteCoreOrLanguageLibrary()
    {
        // The English template includes both Core.aslx and English.aslx — both are built in and
        // should be protected.
        await AssertBuiltInLibrariesProtected("English", "Core.aslx", "English.aslx");
    }

    [TestMethod]
    public async Task TestCannotDeleteGamebookCoreLibrary()
    {
        await AssertBuiltInLibrariesProtected("Gamebook", "GamebookCore.aslx");
    }

    [TestMethod]
    public async Task TestCanDeleteCustomLibrary()
    {
        // A library the game author added themselves (not shipped with the engine) is unaffected
        // by the built-in-library guard and should remain deletable.
        var controller = await LoadTemplateController("English");
        var key = controller.CreateNewIncludedLibrary("custom.aslx");

        Assert.IsTrue(controller.CanDelete(key), "A custom, non-built-in library should be deletable");

        controller.Uninitialise();
    }

    // Regression test for a bug where adding a custom Included Library, saving, then reloading
    // failed with "Library file not found" — CreateNewIncludedLibrary only records the filename
    // (see IncludeSaver, which writes <include ref="custom.aslx"/>, not the library's content), so
    // reloading has to fetch that content again via GetAdjacentFile. WasmEditorBridge fed
    // ByteArrayGameDataProvider with no adjacent-file lookup at all, so any non-built-in library
    // failed to resolve on every reload after the initial (content-free) creation.
    [TestMethod]
    public async Task TestCustomLibraryFailsToReloadWithoutAdjacentFileBytes()
    {
        var savedXml = await CreateGameWithCustomLibraryAndSave();
        var savedBytes = Encoding.UTF8.GetBytes(savedXml);

        var reloadController = new EditorController();
        string errorMessage = null;
        reloadController.ShowMessage += (_, e) => errorMessage = e.Message;

        var ok = await reloadController.Initialise(new ByteArrayGameDataProvider(savedBytes, "test.aslx"));

        Assert.IsFalse(ok, "Reload should fail when the custom library's bytes aren't supplied");
        StringAssert.Contains(errorMessage, "Library file not found: custom.aslx");
    }

    [TestMethod]
    public async Task TestCustomLibraryReloadsWhenAdjacentFileBytesAreSupplied()
    {
        var savedXml = await CreateGameWithCustomLibraryAndSave();
        var savedBytes = Encoding.UTF8.GetBytes(savedXml);
        var adjacentFiles = new Dictionary<string, byte[]>
        {
            ["custom.aslx"] = Encoding.UTF8.GetBytes("<library></library>")
        };

        var reloadController = new EditorController();
        var ok = await reloadController.Initialise(
            new ByteArrayGameDataProvider(savedBytes, "test.aslx", adjacentFiles));

        Assert.IsTrue(ok, "Reload should succeed once the custom library's bytes are supplied");
        reloadController.Uninitialise();
    }

    private static async Task<string> CreateGameWithCustomLibraryAndSave()
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewIncludedLibrary("custom.aslx");
        var xml = controller.Save();
        controller.Uninitialise();
        return xml;
    }

    private static async Task AssertBuiltInLibrariesProtected(string templateName, params string[] libraryFilenames)
    {
        var keysByText = new Dictionary<string, string>();
        void OnAddedNode(object sender, EditorController.AddedNodeEventArgs e) => keysByText[e.Text] = e.Key;

        var controller = await LoadTemplateController(templateName, c => c.AddedNode += OnAddedNode);

        foreach (var filename in libraryFilenames)
        {
            Assert.IsTrue(keysByText.ContainsKey(filename),
                $"Expected an Included Libraries entry for '{filename}'");
            Assert.IsFalse(controller.CanDelete(keysByText[filename]),
                $"Should not be possible to delete '{filename}'");
        }

        controller.Uninitialise();
    }

    private static async Task<EditorController> LoadTemplateController(string templateName,
        Action<EditorController> attachExtraEvents = null)
    {
        var templates = EditorController.GetAvailableTemplates();
        var template = templates.Values.Single(t => t.TemplateName == templateName);
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
        var bytes = Encoding.UTF8.GetBytes(initialFileText);

        var controller = new EditorController();
        // Several of these events are invoked directly rather than via a null-conditional call
        // (e.g. UpdateTree()'s ClearTree/EndTreeUpdate, or RemovedNode when a new element's
        // sortindex is first assigned), so every one needs a subscriber once the controller is
        // fully initialised — mirrors WasmEditorBridge.AttachControllerEvents.
        controller.ClearTree += (_, _) => { };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, _) => { };
        controller.RemovedNode += (_, _) => { };
        controller.RenamedNode += (_, _) => { };
        controller.RetitledNode += (_, _) => { };
        controller.ElementsUpdated += (_, _) => { };
        controller.Dirty += (_, _) => { };
        attachExtraEvents?.Invoke(controller);

        var ok = await controller.Initialise(new ByteArrayGameDataProvider(bytes, "test.aslx"));
        Assert.IsTrue(ok, $"Initialisation failed for template '{templateName}'");

        // Building the tree at least once (as every real caller does right after a successful
        // Initialise — see WasmEditorBridge.Initialise) is what populates m_elementTreeStructure;
        // without it, later element-creation calls NRE deep in EditorController's tree-update path.
        controller.UpdateTree();

        return controller;
    }
}
