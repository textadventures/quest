using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// A new 610 game leaves its text, link and page colours and its verb menu font unset, so v2 uses
// the player's own light or dark colours and its interface font (#2725). Games that store
// Quest 5's defaults explicitly behave the same, and a colour that text sits on gives the game
// its own colours, so the dark scheme's text can't end up on it.
[TestClass]
public class PlayersOwnDefaultsTests
{
    private static Dictionary<string, string> Tokens(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);

    private static string LastLineContaining(GameDriver driver, string text) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.addText")
            .Select(i => (string)((object[])i.Arguments[1])[0])
            .Last(html => html.Contains(text));

    private static readonly string[] ColourTokens =
    [
        "--qv-text", "--qv-bg", "--qv-link",
        "--qv-menu-bg", "--qv-menu-fg", "--qv-menu-hover-bg", "--qv-menu-hover-fg",
    ];

    [TestMethod]
    public async Task ANewGame_LeavesItsColoursAndMenuFontUnset()
    {
        var driver = await GameDriver.LoadAsync("defaultcolourstest.aslx");

        var game = driver.Model.Game;
        foreach (var attribute in new[] { "defaultforeground", "defaultlinkforeground", "defaultbackground", "menufont", "menufontsize" })
        {
            game.Fields.Get(attribute).ShouldBeNull(attribute);
        }

        var tokens = Tokens(driver);
        foreach (var name in ColourTokens) tokens.ShouldNotContainKey(name);
        tokens.ShouldNotContainKey("--qv-menu-font");
        tokens.ShouldNotContainKey("--qv-menu-font-size");
    }

    [TestMethod]
    public async Task Quest5sExplicitDefaults_CountAsUnset()
    {
        var driver = await GameDriver.LoadAsync("quest5defaultstest.aslx");

        var tokens = Tokens(driver);
        foreach (var name in ColourTokens) tokens.ShouldNotContainKey(name);
        tokens.ShouldNotContainKey("--qv-menu-font");
        tokens.ShouldNotContainKey("--qv-menu-font-size");
    }

    [TestMethod]
    public async Task AMenuFontOfTheGamesOwn_IsSent()
    {
        var driver = await GameDriver.LoadAsync("ownmenufonttest.aslx");

        var tokens = Tokens(driver);
        tokens["--qv-menu-font"].ShouldBe("Courier New");
        tokens["--qv-menu-font-size"].ShouldBe("12pt");
        // The menu colours go together, with Quest's classic ones for those the game leaves unset
        tokens["--qv-menu-bg"].ShouldBe("Black");
        tokens["--qv-menu-fg"].ShouldBe("Black");
        tokens["--qv-menu-hover-bg"].ShouldBe("LightGrey");
        tokens["--qv-menu-hover-fg"].ShouldBe("Black");
    }

    [TestMethod]
    public async Task TheClassicInterface_HasNoDarkScheme()
    {
        var driver = await GameDriver.LoadAsync("classicmarginstest.aslx");

        // so a margin colour doesn't bring the text colours with it. Legacy's own menu font is
        // Arial at 9pt.
        var tokens = Tokens(driver);
        tokens.ShouldNotContainKey("--qv-menu-font");
        tokens.ShouldNotContainKey("--qv-menu-font-size");
        tokens["--qv-page-bg"].ShouldBe("LightGrey");
        tokens.ShouldNotContainKey("--qv-bg");
        tokens.ShouldNotContainKey("--qv-text");
    }

    [TestMethod]
    public async Task AMarginColour_GivesTheGameItsOwnColours()
    {
        var driver = await GameDriver.LoadAsync("marginsonlytest.aslx");

        var tokens = Tokens(driver);
        tokens["--qv-page-bg"].ShouldBe("LightGrey");
        tokens["--qv-bg"].ShouldBe("White");
        tokens["--qv-text"].ShouldBe("Black");
        tokens["--qv-link"].ShouldBe("Blue");
        tokens["--qv-accent"].ShouldBe("Blue");
    }

    [TestMethod]
    public async Task ALocationBarTextColour_GivesTheGameItsOwnColours()
    {
        var driver = await GameDriver.LoadAsync("locationtextonlytest.aslx");

        var tokens = Tokens(driver);
        tokens["--qv-status-fg"].ShouldBe("Navy");
        tokens["--qv-bg"].ShouldBe("White");
        tokens["--qv-text"].ShouldBe("Black");
    }

    [TestMethod]
    public async Task APopup_TakesTheUIsColours()
    {
        var driver = await GameDriver.LoadAsync("defaultcolourstest.aslx");
        await driver.SendCommandAsync("popup");

        var html = LastLineContaining(driver, "Its meaning.");
        html.ShouldContain("data-qv-popup=");
        html.ShouldNotContain("color:");
    }
}
