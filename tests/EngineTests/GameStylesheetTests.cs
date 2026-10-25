using Shouldly;

namespace QuestViva.EngineTests;

// From 610, game.stylesheet is the game's own CSS, which the player applies over its UI's styles
// (docs/ui-v2-contract.md, "Game stylesheet"). An older game may use that attribute name for
// something of its own, so it's left alone.
[TestClass]
public class GameStylesheetTests
{
    [TestMethod]
    public async Task A610Game_HasItsStylesheet()
    {
        var driver = await GameDriver.LoadAsync("gamestylesheet610test.aslx");

        driver.Model.GetGameStylesheet().ShouldBe("[data-qv-part=\"output\"] > p { color: teal; }");
    }

    [TestMethod]
    public async Task AnOlderGame_HasNone()
    {
        var driver = await GameDriver.LoadAsync("gamestylesheet600test.aslx");

        driver.Model.GetGameStylesheet().ShouldBeNull();
    }
}
