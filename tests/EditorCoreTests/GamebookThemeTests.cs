using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// A gamebook's Appearance tab (GamebookCoreEditor.aslx) has the Theme chooser too (#2656): every
// theme but the retired ones, from CoreThemes.aslx, which GamebookCore.aslx includes. A theme's
// width replaces the gamebook's 700 pixels, and choosing another theme puts it back.
[TestClass]
public class GamebookThemeTests
{
    private static readonly string[] Offered =
    [
        "*", "theme_parchment", "theme_noir", "theme_storybook", "theme_novella", "theme_typewriter",
        "theme_midnight", "theme_terminal"
    ];

    private static async Task<EditorController> LoadController(string templateName)
    {
        var template = EditorController.GetAvailableTemplates().Values.Single(t => t.TemplateName == templateName);
        var bytes = Encoding.UTF8.GetBytes(EditorController.CreateNewGameFile(template.ResourceName, "Test"));
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
        Assert.IsTrue(await controller.Initialise(new GameData(bytes, "test.aslx")));
        controller.UpdateTree();
        return controller;
    }

    private static IEditorControl ThemeControl(EditorController controller) =>
        controller.GetEditorDefinition(controller.GetElementEditorName("game")!).Tabs.Values
            .SelectMany(t => t.Controls)
            .Single(c => c.ControlType == "dropdowntypes" && c.GetBool("previews"));

    [TestMethod]
    [DataRow("Gamebook", "Parchment")]
    [DataRow("Gamebook (Deutsch)", "Pergament")]
    [DataRow("Gamebook (Español)", "Pergamino")]
    public async Task ANewGamebook_IsOfferedEveryCurrentThemeWithItsLook(string templateName, string parchment)
    {
        using var controller = await LoadController(templateName);
        var control = ThemeControl(controller);

        CollectionAssert.AreEqual(Offered, controller.GetOfferedDropdownTypes("game", control));
        Assert.AreEqual(parchment, control.GetDictionary("types")!["theme_parchment"]);
        var previews = controller.GetDropdownTypePreviews(control).ToDictionary(p => p.Type);
        CollectionAssert.AreEquivalent(Offered, previews.Keys.ToArray());
        Assert.AreEqual("#faf3e3", previews["theme_parchment"].Background);
        Assert.AreEqual("#2a2119", previews["theme_parchment"].DarkBackground);
    }

    [TestMethod]
    public async Task ATheme_SetsTheGamebooksLook_AndChoosingStandardPutsItsOwnBack()
    {
        using var controller = await LoadController("Gamebook");
        var control = ThemeControl(controller);
        var game = controller.GetEditorData("game")!;
        game.SetAttribute("customwidth", 800);
        game.SetAttribute("bordercolour", "Green");
        game.SetAttribute("clearlastpage", true);

        var result = controller.SetDropdownType("game", control, "theme_novella");

        Assert.IsTrue(result.Valid);
        CollectionAssert.AreEquivalent(new[] { "customwidth", "bordercolour" }, result.ResetAttributes);
        Assert.AreEqual(650, game.GetAttribute("customwidth"));
        Assert.AreEqual(60, game.GetAttribute("custompaddingtop"));
        // Not part of the look
        Assert.AreEqual(true, game.GetAttribute("clearlastpage"));

        controller.SetDropdownType("game", control, "*");

        Assert.AreEqual(700, game.GetAttribute("customwidth"));
        Assert.AreEqual(false, game.GetAttribute("setcustompadding"));
        Assert.AreEqual("Georgia, serif", game.GetAttribute("defaultfont"));
    }
}
