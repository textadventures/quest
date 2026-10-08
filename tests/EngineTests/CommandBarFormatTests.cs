using Moq;
using QuestViva.Common;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// Regression coverage for issue #2380: ResetCommandBarFormat only calls setCommandBarStyle when
// the format differs from game.commandbarformat. That cached value is saved with the game, so
// after loading a save it already matched, and the restored page's command bar kept the player's
// default font and colour.
[TestClass]
public class CommandBarFormatTests
{
    private static List<string> CommandBarStyles(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "setCommandBarStyle")
            .Select(i => (string)((object[])i.Arguments[1])[0])
            .ToList();

    [TestMethod]
    public async Task LoadingASave_ReappliesTheCommandBarStyle()
    {
        var driver = await GameDriver.LoadAsync("commandbarformattest.aslx");
        var style = CommandBarStyles(driver).ShouldHaveSingleItem();
        style.ShouldContain("Georgia");

        var tempFilename = Path.GetTempFileName();
        try
        {
            var saveData = driver.Model.Save(SaveMode.SavedGame, html: null);
            await File.WriteAllTextAsync(tempFilename, saveData);

            var restored = await GameDriver.LoadAsync(tempFilename);
            CommandBarStyles(restored).ShouldContain(style);
        }
        finally
        {
            File.Delete(tempFilename);
        }
    }
}
