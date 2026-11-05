using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// The game's Interface tab options for the 610 UI contract (#2650 PR 3): the layout presets, the
// author hook settings, the map position and the classic interface, plus the Appearance tab's
// accent colour and interface font, each captioned in every editor language. Options that only one
// interface uses are shown only for it (#2719).
[TestClass]
public class InterfaceTabOptionTests
{
    private static readonly string[] Attributes =
    [
        "layoutpreset", "narrowlayoutpreset", "mapinteractive", "mapposition", "commandplaceholder",
        "autoscroll", "continueonclick", "fadepreviousoutput", "classicinterface"
    ];

    private static readonly string[] AppearanceAttributes =
        ["accentcolour", "interfacefont", "darkbackground", "darkforeground", "darklinkforeground", "darkaccentcolour"];

    // Shown only for v2, and only for the classic interface (docs/ui-v2-contract.md).
    private static readonly string[] V2OnlyAttributes =
        ["layoutpreset", "narrowlayoutpreset", "accentcolour", "interfacefont", "mapposition"];
    private static readonly string[] ClassicOnlyAttributes = ["panestheme"];

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
    [DataRow("English", "Appearance")]
    [DataRow("Deutsch", "Darstellung")]
    [DataRow("Español", "Apariencia")]
    public async Task TheAccentAndInterfaceFont_AreOnTheAppearanceTab(string templateName, string tabCaption)
    {
        using var controller = await LoadController(templateName);
        AssertTranslatedControls(InterfaceTab(controller, tabCaption).Controls.ToList(), AppearanceAttributes);
    }

    [TestMethod]
    public async Task EachOption_ShowsOnlyForTheInterfaceItAffects()
    {
        using var controller = await LoadController("English");
        var game = controller.GetEditorData("game")!;
        // The map position only shows with the map on.
        game.SetAttribute("gridmap", true);
        var definition = controller.GetEditorDefinition(controller.GetElementEditorName("game")!);
        var controls = definition.Tabs.Values.SelectMany(t => t.Controls).ToList();

        async Task AssertVisible(string attribute, bool expected, string ui)
        {
            var control = controls.Single(c => c.Attribute == attribute);
            Assert.AreEqual(expected, await control.IsControlVisible(game), $"{attribute} on {ui}");
        }

        foreach (var attribute in V2OnlyAttributes) await AssertVisible(attribute, true, "v2");
        foreach (var attribute in ClassicOnlyAttributes) await AssertVisible(attribute, false, "v2");

        game.SetAttribute("classicinterface", true);
        foreach (var attribute in V2OnlyAttributes) await AssertVisible(attribute, false, "classic");
        foreach (var attribute in ClassicOnlyAttributes) await AssertVisible(attribute, true, "classic");
    }

    [TestMethod]
    public async Task TheOtherDarkModeColours_ShowOnceThereIsADarkPageColour()
    {
        using var controller = await LoadController("English");
        var game = controller.GetEditorData("game")!;
        var controls = GameControls(controller);
        string[] others = ["darkforeground", "darklinkforeground", "darkaccentcolour"];

        Assert.IsTrue(await controls.Single(c => c.Attribute == "darkbackground").IsControlVisible(game));
        foreach (var attribute in others)
        {
            Assert.IsFalse(await controls.Single(c => c.Attribute == attribute).IsControlVisible(game), $"{attribute} without a dark page colour");
        }

        game.SetAttribute("darkbackground", "#1a1a2e");
        foreach (var attribute in others)
        {
            Assert.IsTrue(await controls.Single(c => c.Attribute == attribute).IsControlVisible(game), $"{attribute} with a dark page colour");
        }

        game.SetAttribute("classicinterface", true);
        foreach (var attribute in others.Append("darkbackground"))
        {
            Assert.IsFalse(await controls.Single(c => c.Attribute == attribute).IsControlVisible(game), $"{attribute} on the classic interface");
        }
    }

    [TestMethod]
    public async Task RetiredOptions_ShowOnlyInAGameThatHasSetThem()
    {
        using var controller = await LoadController("English");
        var game = controller.GetEditorData("game")!;
        var controls = GameControls(controller);
        string[] retired =
        [
            "setbackgroundopacity", "colourblend", "shadowbox",
            "menubackground", "menuforeground", "menuhoverbackground", "menuhoverforeground"
        ];

        foreach (var attribute in retired)
        {
            Assert.IsFalse(await controls.Single(c => c.Attribute == attribute).IsControlVisible(game), $"{attribute} in a new game");
        }

        game.SetAttribute("colourblend", true);
        game.SetAttribute("menubackground", "Black");
        foreach (var attribute in new[] { "colourblend", "menubackground" })
        {
            var control = controls.Single(c => c.Attribute == attribute);
            Assert.IsTrue(await control.IsControlVisible(game), $"{attribute} once set");
            Assert.IsTrue(await control.IsControlVisibleOnlyBecauseSet(game), $"{attribute} is flagged as left over");
        }
    }

    [TestMethod]
    public async Task OnV2_TheLocationBarColoursShow_WithoutTheClassicLocationStyle()
    {
        using var controller = await LoadController("English");
        var game = controller.GetEditorData("game")!;
        var controls = GameControls(controller);

        // classiclocation is on by default, which hid the colours.
        Assert.IsFalse(await controls.Single(c => c.Attribute == "classiclocation").IsControlVisible(game));
        Assert.IsTrue(await controls.Single(c => c.Attribute == "customlocationcolour").IsControlVisible(game));

        game.SetAttribute("classicinterface", true);
        Assert.IsTrue(await controls.Single(c => c.Attribute == "classiclocation").IsControlVisible(game));
        Assert.IsFalse(await controls.Single(c => c.Attribute == "customlocationcolour").IsControlVisible(game));
    }

    [TestMethod]
    public async Task DeactivatingCommandLinks_DoesNotNeedObjectLinks()
    {
        // {command:...} links work whether or not objects and exits are links.
        using var controller = await LoadController("English");
        var game = controller.GetEditorData("game")!;
        game.SetAttribute("enablehyperlinks", false);

        Assert.IsTrue(await GameControls(controller).Single(c => c.Attribute == "deactivatecommandlinks").IsControlVisible(game));
    }

    private static List<IEditorControl> GameControls(EditorController controller) =>
        controller.GetEditorDefinition(controller.GetElementEditorName("game")!).Tabs.Values.SelectMany(t => t.Controls).ToList();

    [TestMethod]
    public async Task TheGame_HasNoDisplayTab()
    {
        using var controller = await LoadController("English");
        var definition = controller.GetEditorDefinition(controller.GetElementEditorName("game")!);
        var captions = definition.Tabs.Values.Select(t => t.Caption).ToList();
        CollectionAssert.Contains(captions, "Appearance");
        CollectionAssert.Contains(captions, "Interface");
        CollectionAssert.DoesNotContain(captions, "Display");
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
