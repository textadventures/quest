using Shouldly;

namespace QuestViva.EngineTests;

// Regression coverage for the "poster hides an exit" report against issue #2189
// (https://github.com/textadventures/quest/issues/2189). Grid_CalculateMapCoordinates
// runs on room entry and walks every exit from the current room to pre-compute map
// coordinates for the room on the other side, so they're ready whenever the player
// actually gets there. PR #1710 (first shipped in v5.10.0) added an "and exit.visible"
// guard around that whole block to stop invisible exits being drawn on the map - but
// that also skipped the coordinate calculation itself for any currently-invisible exit,
// not just its rendering. So a room only reachable through an exit that starts hidden
// (e.g. behind a poster) never gets coordinates assigned. Making the exit visible later
// and walking through it then hits a room with no map position. The fix keeps coordinate
// calculation unconditional and moves the visibility check to only the grid_render flags,
// which is what actually controls drawing.
[TestClass]
public class GridExitVisibilityTests
{
    [TestMethod]
    public async Task EnteringRoomThroughExit_MadeVisibleAfterCoordinatesWereCalculated_DoesNotError()
    {
        var driver = await GameDriver.LoadAsync("gridexitvisibilitytest.aslx");

        // The east exit out of RoomA is invisible at this point, and RoomA's own
        // Grid_CalculateMapCoordinates pass has already run once on game start.
        (await driver.Model.AssertAsync("game.pov.parent = RoomA")).ShouldBeTrue();

        await driver.SendCommandAsync("reveal");

        await Should.NotThrowAsync(() => driver.SendCommandAsync("east"));

        (await driver.Model.AssertAsync("game.pov.parent = RoomB")).ShouldBeTrue();
    }
}
