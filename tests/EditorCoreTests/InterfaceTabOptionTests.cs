using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// The game's Interface tab options for the 610 UI contract (#2650 PR 3): the layout presets, the
// author hook settings, the map position and the classic interface, plus the Display tab's accent
// colour and interface font, each captioned in every editor language.
[TestClass]
public class InterfaceTabOptionTests
{
    private static readonly string[] Attributes =
    [
        "layoutpreset", "narrowlayoutpreset", "mapinteractive", "mapposition", "commandplaceholder",
        "autoscroll", "continueonclick", "fadepreviousoutput", "classicinterface"
    ];

    private static readonly string[] DisplayAttributes = ["accentcolour", "interfacefont"];

    private static async Task<EditorController> LoadController(string templateName)
    {
        var template = EditorController.GetAvailableTemplates().Values.Single(t => t.TemplateName == templateName);
        var bytes = Encoding.UTF8.GetBytes(EditorController.CreateNewGameFile(template.ResourceName, "Test"));

        var controller = new EditorController();
        controller.ClearTree += (_, _) => { };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { };
        controller.AddedNode += (_, _) => { };
        Assert.IsTrue(await controller.Initialise(new GameData(bytes, "test.aslx")), $"Initialisation failed for '{templateName}'");
        return controller;
    }

    private static IEditorTab InterfaceTab(EditorController controller, string caption)
    {
        var definition = controller.GetEditorDefinition(controller.GetElementEditorName("game")!);
        return definition.Tabs.Values.Single(t => t.Caption == caption);
    }

    private static void AssertTranslatedControls(List<IEditorControl> controls, IEnumerable<string> attributes)
    {
        foreach (var attribute in attributes)
        {
            var control = controls.SingleOrDefault(c => c.Attribute == attribute);
            Assert.IsNotNull(control, $"no control for {attribute}");
            Assert.IsFalse(string.IsNullOrEmpty(control.Caption), attribute);
            StringAssert.DoesNotMatch(control.Caption, new System.Text.RegularExpressions.Regex(@"\["),
                $"{attribute}'s caption isn't translated");
        }
    }

    [TestMethod]
    [DataRow("English", "Interface", "Beside the game (in a drawer on a phone)")]
    [DataRow("Deutsch", "Interface", "Neben dem Spiel (auf dem Handy in einer Schublade)")]
    [DataRow("Español", "Interfaz", "Junto al juego (en un cajón en el móvil)")]
    public async Task EachOption_IsOnTheInterfaceTab(string templateName, string tabCaption, string defaultLayout)
    {
        using var controller = await LoadController(templateName);
        var controls = InterfaceTab(controller, tabCaption).Controls.ToList();

        AssertTranslatedControls(controls, Attributes);

        var layout = controls.Single(c => c.Attribute == "layoutpreset").GetDictionary("validvalues")!;
        CollectionAssert.AreEquivalent(new[] { "", "bottombar", "nopanes" }, layout.Keys.ToArray());
        Assert.AreEqual(defaultLayout, layout[""]);
        var narrow = controls.Single(c => c.Attribute == "narrowlayoutpreset").GetDictionary("validvalues")!;
        CollectionAssert.AreEquivalent(new[] { "", "sidepanes", "bottombar", "nopanes" }, narrow.Keys.ToArray());
        var mapPosition = controls.Single(c => c.Attribute == "mapposition").GetDictionary("validvalues")!;
        CollectionAssert.AreEquivalent(new[] { "", "panes" }, mapPosition.Keys.ToArray());
    }

    [TestMethod]
    [DataRow("English", "Display")]
    [DataRow("Deutsch", "Oberfläche")]
    [DataRow("Español", "Pantalla")]
    public async Task TheAccentAndInterfaceFont_AreOnTheDisplayTab(string templateName, string tabCaption)
    {
        using var controller = await LoadController(templateName);
        AssertTranslatedControls(InterfaceTab(controller, tabCaption).Controls.ToList(), DisplayAttributes);
    }

    [TestMethod]
    public async Task ANewGame_ScrollsAndLetsTheMapMove()
    {
        using var controller = await LoadController("English");
        var game = controller.GetEditorData("game")!;

        Assert.AreEqual(true, game.GetAttribute("autoscroll"));
        Assert.AreEqual(true, game.GetAttribute("mapinteractive"));
        Assert.AreEqual(false, game.GetAttribute("continueonclick"));
    }
}
