using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Regression coverage for issue #1734: "Make editable copy" on an inherited script attribute took
// two Undo clicks to revert if the script contained a function call (e.g. defaultobject's
// changedparent, which calls OnEnterRoom / MergePOVCoordinates), while an otherwise identical
// script without one (changedisopen, changedlocked) reverted in a single click.
//
// Cause: FunctionCallParameters hooked its parameter QuestList up to the undo logger *before*
// populating it, so cloning such a script inside an open transaction (which is exactly what
// "Make editable copy" does) logged a spurious UndoListAdd per parameter alongside the real
// attribute-set action. Undoing then removed those parameters again, and QuestList's Removed
// handler in FunctionCallScript raised FunctionCallParametersUpdated with no subscriber attached
// (only a script open in the editor subscribes), throwing a NullReferenceException part-way
// through the undo. The model had already reverted by then, but the exception propagated out of
// WasmEditorBridge.Undo, so the AppShell never ran its post-undo refresh and the UI carried on
// showing the attribute as an owned copy until the next Undo click repainted it.
[TestClass]
public class MakeScriptEditableTests
{
    // changedparent is the reported case (its body calls OnEnterRoom/MergePOVCoordinates); the
    // other two are inherited from the same type and contain no function-call statements, so they
    // were never affected and act as the control.
    [DataTestMethod]
    [DataRow("changedparent")]
    [DataRow("changedisopen")]
    [DataRow("changedlocked")]
    public async Task TestMakeInheritedScriptEditableTakesOneUndo(string attribute)
    {
        var controller = await LoadTemplateController("English");
        controller.CreateNewRoom("aRoom", "game", "A Room");

        var undoCountBefore = controller.GetUndoItems().Count();
        var scripts = (IEditableScripts) controller.GetEditorData("aRoom").GetAttribute(attribute);
        Assert.AreEqual("defaultobject", scripts.Owner, $"'{attribute}' should start out inherited");

        controller.StartTransaction($"Copy {attribute} script to aRoom");
        scripts.Clone("aRoom", attribute);
        controller.EndTransaction();

        Assert.AreEqual(undoCountBefore + 1, controller.GetUndoItems().Count(),
            "Making the script editable should add exactly one undo entry");
        Assert.AreEqual("aRoom", GetScriptOwner(controller, attribute),
            "The script should now be owned by the room");

        await controller.Undo();

        Assert.AreEqual(undoCountBefore, controller.GetUndoItems().Count());
        Assert.AreEqual("defaultobject", GetScriptOwner(controller, attribute),
            "A single Undo should put the script back to its inherited state");

        controller.Uninitialise();
    }

    private static string GetScriptOwner(EditorController controller, string attribute)
    {
        return ((IEditableScripts) controller.GetEditorData("aRoom").GetAttribute(attribute)).Owner;
    }

    // Mirrors IncludedLibraryTests / EditorControllerRoomsAndPagesTests - the shared test.aslx
    // fixture has its <include> lines commented out, so a real template is needed to get the Core
    // library's defaultobject type (and its changed* scripts) into the game.
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
