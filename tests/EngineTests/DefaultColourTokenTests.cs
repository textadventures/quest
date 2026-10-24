using QuestViva.Common;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// 610 Core sends the colour tokens only once a game has changed a colour from the editor's
// defaults, so a game that hasn't gets the UI's own light or dark colours (#2613, decision 5).
// The text, link and background colours go together, as do the menu colours.
[TestClass]
public class DefaultColourTokenTests
{
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
    public async Task DefaultColours_SendNoColourTokens()
    {
        var driver = await GameDriver.LoadAsync("defaultcolourstest.aslx");

        var tokens = Tokens(driver);
        foreach (var name in ColourTokens) tokens.ShouldNotContainKey(name);
        tokens["--qv-font-body"].ShouldBe("Georgia, serif");
    }

    [TestMethod]
    public async Task DefaultColours_LeaveColoursOutOfTheCommandBarStyle()
    {
        var driver = await GameDriver.LoadAsync("defaultcolourstest.aslx");

        var style = LastCommandBarStyle(driver);
        style.ShouldContain("font-family:Georgia, serif;");
        style.ShouldNotContain("color:");
        style.ShouldNotContain("background");
    }

    [TestMethod]
    public async Task AColourSetMidGame_IsWrittenInlineAndColoursTheCommandBar()
    {
        var driver = await GameDriver.LoadAsync("defaultcolourstest.aslx");
        await driver.SendCommandAsync("recolour");

        LastLineContaining(driver, "Red text.").ShouldStartWith("<span style=\"color:red;\">");
        LastCommandBarStyle(driver).ShouldContain("color:red;");
        Tokens(driver).ShouldNotContainKey("--qv-text");
    }

    [TestMethod]
    public async Task ABackgroundSetMidGame_SendsTheTextColoursWithIt()
    {
        var driver = await GameDriver.LoadAsync("defaultcolourstest.aslx");
        await driver.SendCommandAsync("background");

        var tokens = Tokens(driver);
        tokens["--qv-bg"].ShouldBe("black");
        tokens["--qv-text"].ShouldBe("Black");
        tokens["--qv-link"].ShouldBe("Blue");
    }

    [TestMethod]
    public async Task OneChangedColour_SendsItsWholeGroup()
    {
        var driver = await GameDriver.LoadAsync("backgroundcolourtest.aslx");

        var tokens = Tokens(driver);
        tokens["--qv-bg"].ShouldBe("Black");
        tokens["--qv-text"].ShouldBe("Black");
        tokens["--qv-link"].ShouldBe("Blue");
        tokens["--qv-menu-bg"].ShouldBe("White");
        tokens["--qv-menu-hover-bg"].ShouldBe("Yellow");
        LastCommandBarStyle(driver).ShouldContain("background:Black");
    }

    [TestMethod]
    public async Task AfterLoadingASave_DefaultColoursStillSendNoTokens()
    {
        var driver = await GameDriver.LoadAsync("defaultcolourstest.aslx");
        await driver.SendCommandAsync("recolour");

        var tempFilename = Path.GetTempFileName();
        try
        {
            var saveData = driver.Model.Save(SaveMode.SavedGame, html: null);
            await File.WriteAllTextAsync(tempFilename, saveData);

            var restored = await GameDriver.LoadAsync(tempFilename);
            var tokens = Tokens(restored);
            foreach (var name in ColourTokens) tokens.ShouldNotContainKey(name);
        }
        finally
        {
            File.Delete(tempFilename);
        }
    }
}
