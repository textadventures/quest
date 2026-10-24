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
    // Each status line's text, joined as the status pane shows them.
    private static string StatusText(object items) =>
        string.Join("<br/>", ((System.Collections.IEnumerable)items)
            .Cast<System.Collections.IDictionary>()
            .Select(item => (string)item["text"]!));

    [TestMethod]
    public async Task StatusAttributeSetInStartScriptDoesNotError()
    {
        var gameData = new GameData(File.ReadAllBytes("statusattributetest.aslx"), "statusattributetest.aslx");
        var worldModel = Helpers.CreateWorldModel(gameData);

        var errors = new List<string>();
        worldModel.LogError += ex => errors.Add(ex.Message);

        var status = new List<string>();
        var player = new Mock<IPlayer>();
        player.Setup(p => p.RunScriptAsync("QuestUI.setStatusAttributes", It.IsAny<object[]>()))
            .Callback<string, object[]>((_, parameters) => status.Add(StatusText(parameters[0])))
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

    // Regression coverage for issue #2355. InitStatusAttributes used to do an unconditional
    // 'dictionary add' of the default health/score/money entry, which threw "Error adding
    // key 'health' to dictionary" at game start whenever the author had already given that
    // key its own format (a natural way to customise the built-in Health line) while the
    // matching Features-tab toggle was also on. The author's own format should win instead.
    [TestMethod]
    public async Task CustomFormatForBuiltInStatusAttributeDoesNotErrorAndWins()
    {
        var gameData = new GameData(File.ReadAllBytes("statusattributecustombuiltintest.aslx"), "statusattributecustombuiltintest.aslx");
        var worldModel = Helpers.CreateWorldModel(gameData);

        var errors = new List<string>();
        worldModel.LogError += ex => errors.Add(ex.Message);

        var status = new List<string>();
        var player = new Mock<IPlayer>();
        player.Setup(p => p.RunScriptAsync("QuestUI.setStatusAttributes", It.IsAny<object[]>()))
            .Callback<string, object[]>((_, parameters) => status.Add(StatusText(parameters[0])))
            .Returns(Task.CompletedTask);

        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await worldModel.Begin();

        errors.ShouldBeEmpty();
        status.Last().ShouldBe("HP: 100");
    }

    // 610 Core adds "pinned" and "max" to each status item, from game.pinnedstatusattributes
    // and game.statusattributemaximums: a maximum can be a number or another attribute of the
    // same element, and one that names no numeric attribute is left out.
    [TestMethod]
    public async Task StatusItemsCarryPinnedAndMax()
    {
        var gameData = new GameData(File.ReadAllBytes("statusattributepinmaxtest.aslx"), "statusattributepinmaxtest.aslx");
        var worldModel = Helpers.CreateWorldModel(gameData);

        var errors = new List<string>();
        worldModel.LogError += ex => errors.Add(ex.Message);

        List<System.Collections.IDictionary>? items = null;
        var player = new Mock<IPlayer>();
        player.Setup(p => p.RunScriptAsync("QuestUI.setStatusAttributes", It.IsAny<object[]>()))
            .Callback<string, object[]>((_, parameters) =>
                items = ((System.Collections.IEnumerable)parameters[0]).Cast<System.Collections.IDictionary>().ToList())
            .Returns(Task.CompletedTask);

        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");
        await worldModel.Begin();

        errors.ShouldBeEmpty();
        items.ShouldNotBeNull();
        var byName = items.ToDictionary(item => (string)item["name"]!);

        byName["score"].Contains("pinned").ShouldBeTrue();
        byName["score"].Contains("max").ShouldBeFalse();

        byName["health"]["pinned"].ShouldBe(true);
        byName["health"]["max"].ShouldBe(100);
        byName["health"]["value"].ShouldBe(70);

        byName["mana"].Contains("pinned").ShouldBeFalse();
        byName["mana"]["max"].ShouldBe(8);

        byName["gold"].Contains("max").ShouldBeFalse();
        byName["gold"]["text"].ShouldBe("Gold: 5");
    }
}
