using System.Text;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// A game (usually through its theme) with a darkbackground has colours for v2's dark scheme too
// (HasDarkColours). 610 Core then sends each scheme's colours under its own names
// (--qv-text-light, --qv-text-dark, ...), so the UI can follow the player's light or dark choice,
// instead of the one set for both (--qv-text, ...) that keeps the game always light.
[TestClass]
public class DarkColourTokenTests
{
    private static async Task<GameDriver> LoadAsync(string gameAttributes)
    {
        var aslx = $$"""
            <asl version="610">
              <include ref="English.aslx"/>
              <include ref="Core.aslx"/>
              <game name="DarkColourTest">{{gameAttributes}}</game>
              <object name="room">
                <inherit name="editor_room" />
                <object name="player">
                  <inherit name="defaultplayer" />
                </object>
              </object>
              <command name="background">
                <pattern>background</pattern>
                <script>SetBackgroundColour ("Navy")</script>
              </command>
            </asl>
            """;
        return await GameDriver.LoadAsync(new GameData(Encoding.UTF8.GetBytes(aslx), "darkcolourtest.aslx"));
    }

    private static Dictionary<string, string> Tokens(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);

    private static string LastCommandBarStyle(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setCommandBarStyle")
            .Select(i => (string)((object[])i.Arguments[1])[0])
            .Last();

    private const string Parchment = """
        <defaultbackground>#faf3e3</defaultbackground>
        <defaultforeground>#3b2f22</defaultforeground>
        <defaultlinkforeground>#8a3b12</defaultlinkforeground>
        <darkbackground>#2a2119</darkbackground>
        <darkforeground>#efe4cf</darkforeground>
        """;

    [TestMethod]
    public async Task LightAndDarkColours_GoUnderEachSchemesNames()
    {
        var driver = await LoadAsync(Parchment);

        var tokens = Tokens(driver);
        tokens["--qv-bg-light"].ShouldBe("#faf3e3");
        tokens["--qv-text-light"].ShouldBe("#3b2f22");
        tokens["--qv-link-light"].ShouldBe("#8a3b12");
        tokens["--qv-bg-dark"].ShouldBe("#2a2119");
        tokens["--qv-text-dark"].ShouldBe("#efe4cf");
        tokens.ShouldNotContainKey("--qv-link-dark");
        tokens.ShouldNotContainKey("--qv-bg");
        tokens.ShouldNotContainKey("--qv-text");
        tokens.ShouldNotContainKey("--qv-link");
        // No dark accent, so the light link colour is the accent for both, which the UI lightens
        tokens["--qv-accent"].ShouldBe("#8a3b12");
    }

    [TestMethod]
    public async Task ADarkAccent_GoesWithTheLightOne()
    {
        var driver = await LoadAsync(Parchment + "<accentcolour>#8a3b12</accentcolour><darkaccentcolour>#e0a066</darkaccentcolour>");

        var tokens = Tokens(driver);
        tokens["--qv-accent-light"].ShouldBe("#8a3b12");
        tokens["--qv-accent-dark"].ShouldBe("#e0a066");
        tokens.ShouldNotContainKey("--qv-accent");
    }

    [TestMethod]
    public async Task ADarkLinkColour_IsTheDarkAccentWithoutOne()
    {
        var driver = await LoadAsync(Parchment + "<darklinkforeground>#f0b080</darklinkforeground>");

        var tokens = Tokens(driver);
        tokens["--qv-link-dark"].ShouldBe("#f0b080");
        tokens["--qv-accent-dark"].ShouldBe("#f0b080");
        tokens["--qv-accent-light"].ShouldBe("#8a3b12");
    }

    [TestMethod]
    public async Task OnlyDarkColours_LeaveTheLightSchemeToTheUI()
    {
        var driver = await LoadAsync("<darkbackground>#101828</darkbackground>");

        var tokens = Tokens(driver);
        tokens["--qv-bg-dark"].ShouldBe("#101828");
        foreach (var name in new[] { "--qv-bg", "--qv-text", "--qv-link", "--qv-bg-light", "--qv-text-light", "--qv-link-light", "--qv-accent" })
        {
            tokens.ShouldNotContainKey(name);
        }
    }

    [TestMethod]
    [DataRow("<marginscolour>Tan</marginscolour>")]
    [DataRow("<customlocationcolour>Tan</customlocationcolour>")]
    [DataRow("<colourblend /><colourblendtop>Tan</colourblendtop><colourblendbottom>White</colourblendbottom>")]
    [DataRow("<classicinterface />")]
    public async Task AColourWithNoDarkVersion_KeepsTheGameLight(string extra)
    {
        var driver = await LoadAsync(Parchment + extra);

        var tokens = Tokens(driver);
        tokens["--qv-bg"].ShouldBe("#faf3e3");
        tokens["--qv-text"].ShouldBe("#3b2f22");
        tokens.ShouldNotContainKey("--qv-bg-dark");
        tokens.ShouldNotContainKey("--qv-bg-light");
    }

    [TestMethod]
    [DataRow("<marginscolour>Tan</marginscolour><darkmarginscolour>#3a2e22</darkmarginscolour>", "--qv-page-bg", "Tan", "#3a2e22")]
    [DataRow("<customlocationcolour>Tan</customlocationcolour><darkcustomlocationcolour>#3a2e22</darkcustomlocationcolour>", "--qv-status-bg", "Tan", "#3a2e22")]
    [DataRow("<customlocationtextcolour>Maroon</customlocationtextcolour><darkcustomlocationtextcolour>Pink</darkcustomlocationtextcolour>", "--qv-status-fg", "Maroon", "Pink")]
    public async Task AColourWithADarkVersion_GoesUnderEachSchemesNames(string extra, string token, string light, string dark)
    {
        var driver = await LoadAsync(Parchment + extra);

        var tokens = Tokens(driver);
        tokens[token + "-light"].ShouldBe(light);
        tokens[token + "-dark"].ShouldBe(dark);
        tokens.ShouldNotContainKey(token);
        // The game still follows light and dark mode
        tokens["--qv-bg-dark"].ShouldBe("#2a2119");
        tokens.ShouldNotContainKey("--qv-bg");
    }

    [TestMethod]
    public async Task OneLightOnlyColour_StillKeepsTheGameLight()
    {
        var driver = await LoadAsync(Parchment
            + "<marginscolour>Tan</marginscolour><darkmarginscolour>#3a2e22</darkmarginscolour><customlocationcolour>Tan</customlocationcolour>");

        var tokens = Tokens(driver);
        tokens["--qv-bg"].ShouldBe("#faf3e3");
        tokens["--qv-page-bg"].ShouldBe("Tan");
        tokens["--qv-status-bg"].ShouldBe("Tan");
        tokens.ShouldNotContainKey("--qv-bg-dark");
        tokens.ShouldNotContainKey("--qv-page-bg-dark");
    }

    [TestMethod]
    public async Task ADarkMarginWithoutDarkColours_IsLight()
    {
        // A dark margin only counts once the game has a dark page colour too
        var driver = await LoadAsync("<defaultbackground>#faf3e3</defaultbackground><marginscolour>Tan</marginscolour><darkmarginscolour>#3a2e22</darkmarginscolour>");

        var tokens = Tokens(driver);
        tokens["--qv-bg"].ShouldBe("#faf3e3");
        tokens["--qv-page-bg"].ShouldBe("Tan");
        tokens.ShouldNotContainKey("--qv-page-bg-dark");
    }

    [TestMethod]
    public async Task BorderColours_KeepTheLightOneInDarkModeWithoutADarkVersion()
    {
        var driver = await LoadAsync(Parchment
            + "<bordercolour>Tan</bordercolour><customlocationbordercolour>Maroon</customlocationbordercolour><darkcustomlocationbordercolour>Pink</darkcustomlocationbordercolour>");

        var tokens = Tokens(driver);
        tokens["--qv-border-colour-light"].ShouldBe("Tan");
        tokens["--qv-border-colour-dark"].ShouldBe("Tan");
        tokens["--qv-status-border-light"].ShouldBe("1px solid Maroon");
        tokens["--qv-status-border-dark"].ShouldBe("1px solid Pink");
        // Border colours have no say in whether the game follows dark mode
        tokens["--qv-bg-dark"].ShouldBe("#2a2119");
    }

    [TestMethod]
    public async Task ABackgroundSetMidGame_MakesTheMarginOneSchemeToo()
    {
        var driver = await LoadAsync(Parchment + "<marginscolour>Tan</marginscolour><darkmarginscolour>#3a2e22</darkmarginscolour>");
        await driver.SendCommandAsync("background");

        Tokens(driver)["--qv-page-bg"].ShouldBe("Tan");
    }

    [TestMethod]
    public async Task TheCommandBar_TakesTheUIsColours()
    {
        var driver = await LoadAsync(Parchment);

        var style = LastCommandBarStyle(driver);
        style.ShouldNotContain("color:");
        style.ShouldNotContain("background");
    }

    [TestMethod]
    public async Task ABackgroundSetMidGame_IsForBothSchemes()
    {
        var driver = await LoadAsync(Parchment);
        await driver.SendCommandAsync("background");

        var tokens = Tokens(driver);
        tokens["--qv-bg"].ShouldBe("Navy");
        tokens["--qv-text"].ShouldBe("#3b2f22");
        tokens["--qv-accent"].ShouldBe("#8a3b12");
        driver.Model.Game.Fields.GetAsType<bool>("midgamebackground").ShouldBeTrue();
    }
}
