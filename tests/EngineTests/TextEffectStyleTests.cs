using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// 610 Core's text effects pass the game's starting font, colour and size as "", as output leaves
// them out of its inline styles, so the effect's text inherits them from the UI (#2627,
// decision 4). A font, colour or size the game has changed since is passed as before.
[TestClass]
public class TextEffectStyleTests
{
    private static object[] LastTextEffect(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.textEffect")
            .Select(i => (object[])i.Arguments[1])
            .Last();

    [TestMethod]
    public async Task Typewriter_WithTheStartingStyle_PassesItEmpty()
    {
        var driver = await GameDriver.LoadAsync("texteffectstest.aslx");
        await driver.SendCommandAsync("typewriter");

        var args = LastTextEffect(driver);
        args[0].ShouldBe("typewriter");
        args[1].ShouldBe("Typed.");
        args.Skip(3).ShouldBe(["", "", ""]);
    }

    [TestMethod]
    public async Task Unscramble_WithTheStartingStyle_PassesItEmpty()
    {
        var driver = await GameDriver.LoadAsync("texteffectstest.aslx");
        await driver.SendCommandAsync("unscramble");

        var args = LastTextEffect(driver);
        args[0].ShouldBe("unscramble");
        args[1].ShouldBe("Unscrambled.");
        args.Skip(4).ShouldBe(["", "", ""]);
    }

    [TestMethod]
    public async Task AStyleChangedMidGame_IsPassed()
    {
        var driver = await GameDriver.LoadAsync("texteffectstest.aslx");
        await driver.SendCommandAsync("restyle");
        await driver.SendCommandAsync("typewriter");

        LastTextEffect(driver).Skip(3).ShouldBe(["Arial", "red", "20"]);
    }
}
