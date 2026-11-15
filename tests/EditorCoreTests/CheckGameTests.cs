using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// The Editor's Check game (#2706) runs what `qv check` runs on the game the Editor has open, so it
// sees edits that haven't been saved.
[TestClass]
public class CheckGameTests
{
    private EditorController _controller = null!;

    [TestInitialize]
    public async Task Init()
    {
        var templates = EditorController.GetAvailableTemplates();
        var template = templates.Values.Single(t => t.TemplateName == "English");
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");

        _controller = new EditorController();
        _controller.ClearTree += (_, _) => { };
        _controller.BeginTreeUpdate += (_, _) => { };
        _controller.EndTreeUpdate += (_, _) => { };
        _controller.AddedNode += (_, _) => { };
        _controller.RemovedNode += (_, _) => { };
        _controller.RenamedNode += (_, _) => { };
        _controller.RetitledNode += (_, _) => { };
        _controller.ElementsUpdated += (_, _) => { };
        _controller.Dirty += (_, _) => { };
        Assert.IsTrue(await _controller.Initialise(new GameData(Encoding.UTF8.GetBytes(initialFileText), "test.aslx")));
        _controller.UpdateTree();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _controller.Uninitialise();
    }

    [TestMethod]
    public void NewGameHasNoProblems()
    {
        Assert.AreEqual(0, _controller.CheckGame().Count);
    }

    [TestMethod]
    public void FindsAnUnsavedEditAndSaysWhereItIs()
    {
        _controller.CreateNewFunction("MyFunction");
        SetScript("MyFunction", "script", "NoSuchFunction (1)");

        var problem = _controller.CheckGame().Single();
        Assert.AreEqual("function", problem.Kind);
        Assert.AreEqual("MyFunction", problem.Element);
        Assert.AreEqual("script", problem.Attribute);
        Assert.IsNull(problem.Filename);
        StringAssert.Contains(problem.Message, "NoSuchFunction");

        // Fixing it clears it, without a save in between.
        SetScript("MyFunction", "script", "msg (\"Hello\")");
        Assert.AreEqual(0, _controller.CheckGame().Count);
    }

    [TestMethod]
    public void LeavesTheGameEditable()
    {
        _controller.CreateNewFunction("MyFunction");
        SetScript("MyFunction", "script", "msg (1 +)");
        Assert.AreEqual(1, _controller.CheckGame().Count);

        // Checking parses scripts as playing the game would, and must leave the world in edit
        // mode: the script stays editable, and the game still saves as it was.
        SetScript("MyFunction", "script", "msg (2)");
        Assert.AreEqual("msg (2)", Scripts("MyFunction", "script").Code.Trim());
        StringAssert.Contains(_controller.Save(), "msg (2)");
    }

    private void SetScript(string element, string attribute, string code)
    {
        var scripts = _controller.GetEditorData(element)!.GetAttribute(attribute) as IEditableScripts
            ?? _controller.CreateNewEditableScripts(element, attribute, null, false);
        scripts.Code = code;
    }

    private IEditableScripts Scripts(string element, string attribute) =>
        (IEditableScripts)_controller.GetEditorData(element)!.GetAttribute(attribute)!;
}
