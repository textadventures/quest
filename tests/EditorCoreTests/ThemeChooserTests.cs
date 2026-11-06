using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// The game's Theme chooser (#2656): every built-in theme, captioned in each editor language; the
// retired Retro and Hot Dog Stand offered only to a game that still has one (<hiddentypes>); and
// each theme's look for ThemePicker.svelte to draw (<previews/>).
[TestClass]
public class ThemeChooserTests
{
    private static readonly string[] Offered =
    [
        "*", "theme_parchment", "theme_noir", "theme_storybook", "theme_novella", "theme_typewriter",
        "theme_midnight", "theme_terminal"
    ];

    private static readonly string[] Retired = ["theme_retro", "theme_hotdogstand"];

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
    [DataRow("English", "Parchment", "Midnight")]
    [DataRow("Deutsch", "Pergament", "Mitternacht")]
    [DataRow("Español", "Pergamino", "Medianoche")]
    public async Task ANewGame_IsOfferedEveryThemeButTheRetiredOnes(string templateName, string parchment, string midnight)
    {
        using var controller = await LoadController(templateName);
        var control = ThemeControl(controller);

        CollectionAssert.AreEqual(Offered, controller.GetOfferedDropdownTypes("game", control));
        var captions = control.GetDictionary("types")!;
        Assert.AreEqual(parchment, captions["theme_parchment"]);
        Assert.AreEqual(midnight, captions["theme_midnight"]);
        foreach (var type in Offered)
        {
            StringAssert.DoesNotMatch(captions[type], new System.Text.RegularExpressions.Regex(@"\["), $"{type}'s caption isn't translated");
        }
    }

    [TestMethod]
    [DataRow("theme_retro")]
    [DataRow("theme_hotdogstand")]
    public async Task AGameWithARetiredTheme_IsStillOfferedIt(string theme)
    {
        using var controller = await LoadController("English");
        controller.AddInheritedTypeToElement("game", theme, false);

        var offered = controller.GetOfferedDropdownTypes("game", ThemeControl(controller));

        CollectionAssert.Contains(offered, theme);
        Assert.AreEqual(theme, controller.GetSelectedDropDownType(ThemeControl(controller), "game"));
    }

    [TestMethod]
    public async Task EachTheme_IsPreviewedInItsOwnColours()
    {
        using var controller = await LoadController("English");
        var previews = controller.GetDropdownTypePreviews(ThemeControl(controller)).ToDictionary(p => p.Type);

        // Every theme, including the retired ones (shown only to a game that has one)
        CollectionAssert.AreEquivalent(Offered.Concat(Retired).ToArray(), previews.Keys.ToArray());

        // The default light and dark colours
        var standard = previews["*"];
        Assert.IsNull(standard.Background);
        Assert.IsNull(standard.DarkBackground);

        // Light and dark colours of its own
        var parchment = previews["theme_parchment"];
        Assert.AreEqual("#faf3e3", parchment.Background);
        Assert.AreEqual("#3b2f22", parchment.Text);
        Assert.AreEqual("#8a3b12", parchment.Link);
        Assert.AreEqual("#2a2119", parchment.DarkBackground);
        Assert.AreEqual("#efe4cf", parchment.DarkText);
        Assert.AreEqual("#e8a873", parchment.DarkLink);
        Assert.AreEqual("Literata", parchment.WebFont);

        // One scheme only
        var terminal = previews["theme_terminal"];
        Assert.AreEqual("#0b0f0b", terminal.Background);
        Assert.AreEqual("#4ff07a", terminal.Text);
        Assert.IsNull(terminal.DarkBackground);

        // A layout theme leaves the colours to the player, but has a font
        var typewriter = previews["theme_typewriter"];
        Assert.IsNull(typewriter.Background);
        Assert.AreEqual("'Courier New', Courier, monospace", typewriter.Font);
    }
}
