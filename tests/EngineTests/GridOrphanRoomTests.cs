using Shouldly;

namespace QuestViva.EngineTests;

// Issue #2189 (https://github.com/textadventures/quest/issues/2189): Grid_CalculateMapCoordinates
// only places rooms it reaches through an exit from a room that's already placed. A room the
// player arrives in some other way had no coordinates, so drawing it threw "key 'x' was not
// present" errors. Now the pass first retries from the room the player came from (which finds
// exits created since its last pass), and failing that puts the room on a new level of its own.
[TestClass]
public class GridOrphanRoomTests
{
    private static async Task<double> CoordinateAsync(GameDriver driver, string room, string coordinate)
    {
        return (double)(await driver.Model.EvaluateAsync(
            $"Grid_GetGridCoordinateForPlayer(player, {room}, \"{coordinate}\")"))!;
    }

    [TestMethod]
    public async Task MovePlayer_ToRoomNoExitLeadsTo_PlacesItOnANewLevel()
    {
        var driver = await GameDriver.LoadAsync("gridorphanroomtest.aslx");

        await Should.NotThrowAsync(() => driver.SendCommandAsync("teleport RoomB"));

        (await driver.Model.AssertAsync("game.pov.parent = RoomB")).ShouldBeTrue();
        (await CoordinateAsync(driver, "RoomB", "x")).ShouldBe(0);
        (await CoordinateAsync(driver, "RoomB", "y")).ShouldBe(0);
        // RoomA is level 0 and Cellar is level -1, so the next free level is 1
        (await CoordinateAsync(driver, "RoomB", "z")).ShouldBe(1);
    }

    [TestMethod]
    public async Task MovePlayer_ToSecondOrphanRoom_PlacesItAboveTheFirst()
    {
        var driver = await GameDriver.LoadAsync("gridorphanroomtest.aslx");

        await driver.SendCommandAsync("teleport RoomB");
        await Should.NotThrowAsync(() => driver.SendCommandAsync("teleport RoomD"));

        (await CoordinateAsync(driver, "RoomD", "z")).ShouldBe(2);
        // Placing RoomD leaves RoomB where it was
        (await CoordinateAsync(driver, "RoomB", "z")).ShouldBe(1);
    }

    [TestMethod]
    public async Task EnteringRoomThroughExit_CreatedAfterLastPass_PlacesItNextToThePreviousRoom()
    {
        var driver = await GameDriver.LoadAsync("gridorphanroomtest.aslx");

        await driver.SendCommandAsync("dig");
        await Should.NotThrowAsync(() => driver.SendCommandAsync("east"));

        (await driver.Model.AssertAsync("game.pov.parent = RoomC")).ShouldBeTrue();
        // RoomA is 2 wide at x=0, and the exit is 1 long
        (await CoordinateAsync(driver, "RoomC", "x")).ShouldBe(3);
        (await CoordinateAsync(driver, "RoomC", "z")).ShouldBe(0);
    }
}
