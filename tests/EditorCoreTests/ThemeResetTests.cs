using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Choosing a theme on the game's Appearance tab (a dropdowntypes control with <resetoverrides/>)
// clears the game's own value for anything a theme sets, so the new theme isn't hidden by options
// the author chose under the old one. It's one undo step, and other type dropdowns don't do it.
[TestClass]
public class ThemeResetTests
{
    private EditorController _controller = null!;

    [TestInitialize]
    public async Task Init()
    {
        var template = EditorController.GetAvailableTemplates().Values.Single(t => t.TemplateName == "English");
        var bytes = Encoding.UTF8.GetBytes(EditorController.CreateNewGameFile(template.ResourceName, "Test"));
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
        Assert.IsTrue(await _controller.Initialise(new GameData(bytes, "test.aslx")));
        _controller.UpdateTree();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _controller.Uninitialise();
    }

    [TestMethod]
    public void ChoosingATheme_ClearsWhatAnyThemeSets()
    {
        var game = Data("game");
        game.SetAttribute("defaultbackground", "Pink");
        game.SetAttribute("defaultfont", "Arial");
        // Set by Novella and Typewriter, not Retro: still cleared, so Retro's look is the whole of it
        game.SetAttribute("customwidth", 800);
        // Not set by any theme
        game.SetAttribute("accentcolour", "Purple");

        var result = _controller.SetDropdownType("game", ThemeControl(), "theme_retro");

        Assert.IsTrue(result.Valid);
        CollectionAssert.AreEquivalent(new[] { "defaultbackground", "defaultfont", "customwidth" }, result.ResetAttributes);
        Assert.IsTrue(_controller.DoesElementInheritType("game", "theme_retro"));
        Assert.AreEqual("Black", Data("game").GetAttribute("defaultbackground"));
        Assert.AreEqual("'Lucida Console', Monaco, monospace", Data("game").GetAttribute("defaultfont"));
        Assert.AreEqual(950, Data("game").GetAttribute("customwidth"));
        Assert.AreEqual("Purple", Data("game").GetAttribute("accentcolour"));
    }

    [TestMethod]
    public void ChoosingStandard_AlsoClears()
    {
        _controller.SetDropdownType("game", ThemeControl(), "theme_retro");
        Data("game").SetAttribute("defaultbackground", "Navy");

        var result = _controller.SetDropdownType("game", ThemeControl(), "*");

        CollectionAssert.AreEquivalent(new[] { "defaultbackground" }, result.ResetAttributes);
        Assert.IsFalse(_controller.DoesElementInheritType("game", "theme_retro"));
        Assert.IsNull(Data("game").GetAttribute("defaultbackground"));
    }

    [TestMethod]
    public async Task OneUndo_BringsBackTheOldThemeAndSettings()
    {
        _controller.SetDropdownType("game", ThemeControl(), "theme_novella");
        Data("game").SetAttribute("defaultbackground", "Pink");
        Data("game").SetAttribute("customwidth", 800);

        _controller.SetDropdownType("game", ThemeControl(), "theme_retro");
        await _controller.Undo();

        Assert.IsTrue(_controller.DoesElementInheritType("game", "theme_novella"));
        Assert.IsFalse(_controller.DoesElementInheritType("game", "theme_retro"));
        Assert.AreEqual("Pink", Data("game").GetAttribute("defaultbackground"));
        Assert.AreEqual(800, Data("game").GetAttribute("customwidth"));
    }

    [TestMethod]
    public void OnANewGame_NothingIsCleared()
    {
        var result = _controller.SetDropdownType("game", ThemeControl(), "theme_retro");

        Assert.AreEqual(0, result.ResetAttributes.Count, string.Join(",", result.ResetAttributes));
    }

    [TestMethod]
    public void OtherTypeDropdowns_KeepTheElementsOwnValues()
    {
        _controller.CreateNewObject("thing", null, "Thing");
        Data("thing").SetAttribute("gender", "it");
        var setupType = _controller.GetEditorDefinition(_controller.GetElementEditorName("thing")!).Tabs.Values
            .SelectMany(t => t.Controls)
            .Single(c => c.ControlType == "dropdowntypes" && c.GetDictionary("types")!.ContainsKey("male"));

        var result = _controller.SetDropdownType("thing", setupType, "male");

        Assert.IsTrue(result.Valid);
        Assert.AreEqual(0, result.ResetAttributes.Count);
        Assert.AreEqual("it", Data("thing").GetAttribute("gender"));
    }

    [TestMethod]
    public void AnUnknownType_IsRefused()
    {
        var result = _controller.SetDropdownType("game", ThemeControl(), "theme_nonesuch");

        Assert.IsFalse(result.Valid);
    }

    private IEditorControl ThemeControl()
    {
        return _controller.GetEditorDefinition(_controller.GetElementEditorName("game")!).Tabs.Values
            .SelectMany(t => t.Controls)
            .Single(c => c.ControlType == "dropdowntypes" && c.GetBool("resetoverrides"));
    }

    private IEditorData Data(string key)
    {
        return _controller.GetEditorData(key)!;
    }
}
