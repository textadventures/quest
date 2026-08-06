using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

[TestClass]
public class IncludedLibraryTests
{
    // Base libraries (Core.aslx / GamebookCore.aslx) provide every game's verbs, object types and
    // templates — deleting the <include> for one leaves the game unable to load (see
    // EditorController.CanDelete). Loads a real game from each shipped template (rather than the
    // shared test.aslx fixture, whose <include> lines are commented out) so the base library is
    // genuinely present, matching TemplateTests' approach.
    [TestMethod]
    public async Task TestCannotDeleteCoreLibrary()
    {
        // The English template includes both English.aslx and Core.aslx — only the latter is
        // protected, confirming the guard is scoped to base libraries rather than blocking
        // deletion of included libraries generally.
        await AssertBaseLibraryProtected("English", "Core.aslx", "English.aslx");
    }

    [TestMethod]
    public async Task TestCannotDeleteGamebookCoreLibrary()
    {
        await AssertBaseLibraryProtected("Gamebook", "GamebookCore.aslx", null);
    }

    private static async Task AssertBaseLibraryProtected(string templateName, string baseLibraryFilename,
        string otherLibraryFilename)
    {
        var templates = EditorController.GetAvailableTemplates();
        var template = templates[templateName];
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
        var bytes = Encoding.UTF8.GetBytes(initialFileText);

        var controller = new EditorController();
        // UpdateTree() no-ops unless something is listening to BeginTreeUpdate, and invokes
        // ClearTree/EndTreeUpdate directly (not via null-conditional), so all three need a
        // subscriber — mirrors WasmEditorBridge.AttachControllerEvents.
        controller.BeginTreeUpdate += (_, _) => { };
        controller.ClearTree += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        var keysByText = new Dictionary<string, string>();
        void OnAddedNode(object sender, EditorController.AddedNodeEventArgs e) => keysByText[e.Text] = e.Key;
        controller.AddedNode += OnAddedNode;

        var ok = await controller.Initialise(new ByteArrayGameDataProvider(bytes, "test.aslx"));
        Assert.IsTrue(ok, $"Initialisation failed for template '{templateName}'");
        controller.UpdateTree();

        Assert.IsTrue(keysByText.ContainsKey(baseLibraryFilename),
            $"Expected an Included Libraries entry for '{baseLibraryFilename}'");
        Assert.IsFalse(controller.CanDelete(keysByText[baseLibraryFilename]),
            $"Should not be possible to delete '{baseLibraryFilename}'");

        if (otherLibraryFilename != null)
        {
            Assert.IsTrue(keysByText.ContainsKey(otherLibraryFilename),
                $"Expected an Included Libraries entry for '{otherLibraryFilename}'");
            Assert.IsTrue(controller.CanDelete(keysByText[otherLibraryFilename]),
                $"'{otherLibraryFilename}' is not a base library and should still be deletable");
        }

        controller.Uninitialise();
    }
}
