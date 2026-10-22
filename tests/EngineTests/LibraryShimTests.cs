using System.Text;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// Issue #2189's fix has to reach games that were published before it, which carry their own
// inlined Grid_CalculateMapCoordinates. LibraryShims wraps that inlined body with the fix when a
// game is loaded to play. These tests use the stock bodies that shipped in each release, frozen
// in GridShim/, slotted into a game that otherwise uses the live Core library (the helpers they
// call haven't changed since Quest 5.5).
[TestClass]
public class LibraryShimTests
{
    private static readonly string[] StockBodies =
    [
        "quest-5.5-beta",
        "quest-5.5.1-to-5.9",
        "quest-5.10-to-6.0-rc",
        "quest-viva-6.0.0",
    ];

    public static IEnumerable<object[]> StockBodyCases => StockBodies.Select(b => new object[] { b });

    private static GameData BuildGame(string stockBody, string version = "600", Func<string, string>? editBody = null)
    {
        var body = File.ReadAllText(Path.Combine("GridShim", stockBody + ".txt"));
        if (editBody != null)
        {
            body = editBody(body);
        }

        var aslx = File.ReadAllText("gridshimtest.aslx")
            .Replace("{{VERSION}}", version)
            .Replace("{{BODY}}", body);
        return new GameData(Encoding.UTF8.GetBytes(aslx), "gridshimtest.aslx");
    }

    private static async Task<double> CoordinateAsync(GameDriver driver, string room, string coordinate)
    {
        return (double)(await driver.Model.EvaluateAsync(
            $"Grid_GetGridCoordinateForPlayer(player, {room}, \"{coordinate}\")"))!;
    }

    private static string FunctionScript(WorldModel worldModel)
    {
        var function = worldModel.Elements.Get(ElementType.Function, "Grid_CalculateMapCoordinates");
        return function.Fields[FieldDefinitions.Script]!.Save();
    }

    [TestMethod]
    [DynamicData(nameof(StockBodyCases))]
    public void StockBody_IsRecognised(string stockBody)
    {
        // Guards against a fixture and its hash drifting apart, which would otherwise show up
        // only as the behavioural tests below failing for no obvious reason
        var body = File.ReadAllText(Path.Combine("GridShim", stockBody + ".txt"));
        LibraryShims.HashBody(body).ShouldBeOneOf(
            "0748afc537fbdf0379598b6b805adc92a905209da5279723ecbefc90226878b8",
            "f3396d4dd03e7ec1e5bee7c9718f4c050ba9b6481904f4102c46080a9130cd20",
            "c1c7650d335c3b5cdf9cbcfb9761b5a9e40dac40d3f886893cec7a5e4e66440f",
            "a452fdbb0f4e80be6f42cadab415cc32a4aee17994ef0601365906ccfd34c9b5");
    }

    [TestMethod]
    [DynamicData(nameof(StockBodyCases))]
    public async Task MovePlayer_ToRoomNoExitLeadsTo_PlacesItOnANewLevel(string stockBody)
    {
        var driver = await GameDriver.LoadAsync(BuildGame(stockBody));

        await Should.NotThrowAsync(() => driver.SendCommandAsync("teleport RoomB"));

        (await driver.Model.AssertAsync("game.pov.parent = RoomB")).ShouldBeTrue();
        (await CoordinateAsync(driver, "RoomB", "z")).ShouldBe(1);
    }

    [TestMethod]
    [DynamicData(nameof(StockBodyCases))]
    public async Task EnteringRoomThroughExit_MadeVisibleAfterLastPass_PlacesItNextToThePreviousRoom(string stockBody)
    {
        // The Quest 5.10 body skips invisible exits entirely (#1710, fixed for new games by #2300),
        // so this only works there because the shim re-runs the pass from the previous room
        var driver = await GameDriver.LoadAsync(BuildGame(stockBody));

        await driver.SendCommandAsync("reveal");
        await Should.NotThrowAsync(() => driver.SendCommandAsync("east"));

        (await driver.Model.AssertAsync("game.pov.parent = RoomC")).ShouldBeTrue();
        (await CoordinateAsync(driver, "RoomC", "x")).ShouldBe(3);
        (await CoordinateAsync(driver, "RoomC", "z")).ShouldBe(0);
    }

    [TestMethod]
    public async Task BodyChangedByAuthor_IsLeftAlone()
    {
        var driver = await GameDriver.LoadAsync(BuildGame("quest-viva-6.0.0",
            editBody: body => body.Replace("room.parent.grid_render = true", "room.parent.grid_render = false")));

        FunctionScript(driver.Model).ShouldNotContain("grid_lastroom");
        await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("teleport RoomB"));
    }

    [TestMethod]
    public async Task GameSavedAfterTheFix_IsLeftAlone()
    {
        // Can't happen for real (a v610 game carries the fixed library), but shows the version gate works
        var driver = await GameDriver.LoadAsync(BuildGame("quest-viva-6.0.0", version: "610"));

        FunctionScript(driver.Model).ShouldNotContain("grid_lastroom");
    }

    [TestMethod]
    public async Task LoadedForEditing_IsLeftAlone()
    {
        var worldModel = new WorldModel(BuildGame("quest-viva-6.0.0"), null);
        (await worldModel.InitialiseEdit()).ShouldBeTrue();

        FunctionScript(worldModel).ShouldNotContain("grid_lastroom");
    }

    [TestMethod]
    [DynamicData(nameof(StockBodyCases))]
    public async Task PackagedByQuestViva_IsStillRecognised(string stockBody)
    {
        // A package holds each script as re-serialised by the saver, not the original text
        var worldModel = new WorldModel(BuildGame(stockBody), null);
        (await worldModel.InitialiseEdit()).ShouldBeTrue();
        worldModel.Game.Fields.Set("gameid", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        using var packageStream = new MemoryStream();
        worldModel.CreatePackage(null, out var error, [], packageStream).ShouldBeTrue(error);

        var driver = await GameDriver.LoadAsync(new GameData(packageStream.ToArray(), "game.quest"));

        driver.Model.Version.ShouldBe(WorldModelVersion.v600);
        await Should.NotThrowAsync(() => driver.SendCommandAsync("teleport RoomB"));
    }

    [TestMethod]
    public async Task SavedGame_KeepsTheShimWithoutApplyingItTwice()
    {
        var gameData = BuildGame("quest-viva-6.0.0");
        var driver = await GameDriver.LoadAsync(gameData);
        await driver.SendCommandAsync("teleport RoomB");
        var saveData = driver.Model.Save(SaveMode.SavedGame);

        var restored = await GameDriver.LoadAsync(BuildGame("quest-viva-6.0.0"),
            saveData: new MemoryStream(Encoding.UTF8.GetBytes(saveData)));

        var script = FunctionScript(restored.Model);
        (script.Split("grid_lastroom = room").Length - 1).ShouldBe(1);
        (await CoordinateAsync(restored, "RoomB", "z")).ShouldBe(1);
        // The previous room survives restore, so arriving through a revealed exit still works
        await restored.SendCommandAsync("teleport RoomA");
        await restored.SendCommandAsync("reveal");
        await Should.NotThrowAsync(() => restored.SendCommandAsync("east"));
        (await CoordinateAsync(restored, "RoomC", "x")).ShouldBe(3);
    }
}
