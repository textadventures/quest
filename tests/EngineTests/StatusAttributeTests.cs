using Moq;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// Regression coverage for issue #2178. StartGame calls UpdateStatusAttributes once before
// running the game's own 'start' script, so a status attribute that is only assigned in
// 'start' is read while still unset. With no format string, FormatStatusAttribute builds
// 'CapFirst(attr) + ": " + value' with no null check, which briefly turned into a script
// error on every affected game's very first turn (fixed engine-side in #2188 - string
// concatenation with an unset attribute is an empty string, as it was in Quest 5).
[TestClass]
public class StatusAttributeTests
{
    [TestMethod]
    public async Task StatusAttributeSetInStartScriptDoesNotError()
    {
        var gameDataProvider = new FileGameDataProvider("statusattributetest.aslx");
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);

        var errors = new List<string>();
        worldModel.LogError += ex => errors.Add(ex.Message);

        var status = new List<string>();
        var player = new Mock<IPlayer>();
        player.Setup(p => p.RunScriptAsync("updateStatus", It.IsAny<object[]>()))
            .Callback<string, object[]>((_, parameters) => status.Add((string)parameters[0]))
            .Returns(Task.CompletedTask);

        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await worldModel.Begin();

        errors.ShouldBeEmpty();

        // The pre-'start' update renders the not-yet-set attributes as empty rather than
        // failing; the post-'start' one picks up the values the start script assigned.
        status.First().ShouldBe("Next interaction: <br/>Chapter: ");
        status.Last().ShouldBe("Next interaction: Della<br/>Chapter: One");
    }
}
