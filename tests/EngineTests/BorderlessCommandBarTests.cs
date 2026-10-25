using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// The borderless command bar's text colour (--qv-command-fg) is the game's text colour, so 610
// Core sends it only once the game has set its own colours, as with the text colours themselves
// (DefaultColourTokenTests). A game that hasn't gets the UI's light or dark text colour.
[TestClass]
public class BorderlessCommandBarTests
{
    private static Dictionary<string, string> Tokens(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);

    private static bool SetsPreset(GameDriver driver, string name) =>
        driver.PlayerMock.Invocations.Any(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                                               && (string)i.Arguments[0] == "QuestUI.setCommandBarPreset"
                                               && (string)((object[])i.Arguments[1])[0] == name);

    [TestMethod]
    public async Task DefaultColours_SendNoCommandColour()
    {
        var driver = await GameDriver.LoadAsync("borderlessdefaulttest.aslx");

        SetsPreset(driver, "borderless").ShouldBeTrue();
        Tokens(driver).ShouldNotContainKey("--qv-command-fg");
    }

    [TestMethod]
    public async Task TheGamesOwnColours_SendItsTextColour()
    {
        var driver = await GameDriver.LoadAsync("borderlesscolourtest.aslx");

        SetsPreset(driver, "borderless").ShouldBeTrue();
        Tokens(driver)["--qv-command-fg"].ShouldBe("Yellow");
    }
}
