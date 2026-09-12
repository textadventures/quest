using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Regression coverage for issue #2258: the attributes editor's "add type" list was offering
// Core's built-in default types (defaultobject, defaultexit, defaultcommand, defaultverb,
// defaultgame), which the engine already applies implicitly to every element of the matching
// kind - adding one explicitly is meaningless. Quest 5's desktop editor filtered these out with
// EditorController.IsDefaultTypeName; WasmEditorBridge.GetTypeNames (browser-wasm only, not
// unit-testable directly) now calls EditorController.GetAddableTypeNames, which is covered here.
[TestClass]
public class AttributesEditorTypeListTests
{
    [TestMethod]
    public async Task TestGetAddableTypeNamesExcludesCoreDefaultTypes()
    {
        var controller = await LoadTemplateController("English");

        controller.CreateNewType("MyCustomType");

        var addableTypeNames = controller.GetAddableTypeNames().ToArray();

        CollectionAssert.DoesNotContain(addableTypeNames, "defaultobject");
        CollectionAssert.DoesNotContain(addableTypeNames, "defaultexit");
        CollectionAssert.DoesNotContain(addableTypeNames, "defaultcommand");
        CollectionAssert.DoesNotContain(addableTypeNames, "defaultverb");
        CollectionAssert.DoesNotContain(addableTypeNames, "defaultgame");
        CollectionAssert.Contains(addableTypeNames, "MyCustomType");

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
