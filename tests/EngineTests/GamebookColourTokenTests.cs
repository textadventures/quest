using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// 610 GamebookCore sends the colour tokens as Core does (DefaultColourTokenTests): none while the
// gamebook keeps the editor's default colours, so it follows the UI's light or dark ones, and
// all three together once it has set its own. It used to send its background on its own, before
// its text colours were recorded, so a default gamebook in dark mode got a white page under
// light text.
[TestClass]
public class GamebookColourTokenTests
{
    private static Dictionary<string, string> Tokens(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);

    [TestMethod]
    public async Task DefaultColours_SendNoColourTokens()
    {
        var driver = await GameDriver.LoadAsync("gamebookdefaultcolourstest.aslx");

        var tokens = Tokens(driver);
        tokens.ShouldNotContainKey("--qv-bg");
        tokens.ShouldNotContainKey("--qv-text");
        tokens.ShouldNotContainKey("--qv-link");
    }

    [TestMethod]
    public async Task TheGamebooksOwnColours_AreSentTogether()
    {
        var driver = await GameDriver.LoadAsync("gamebookowncolourstest.aslx");

        var tokens = Tokens(driver);
        tokens["--qv-bg"].ShouldBe("Black");
        tokens["--qv-text"].ShouldBe("White");
        tokens["--qv-link"].ShouldBe("Blue");
    }
}
