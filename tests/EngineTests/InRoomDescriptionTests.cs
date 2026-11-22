using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

// Issue #2884: with the "In-room descriptions" feature on, an object with an
// in-room description was both written into the room description and listed under
// "You can see". It is now left out of the list whenever its in-room description
// is printed, which is only when the room's description is text: a script
// description gets nothing appended (it can call GetInRoomDescriptions itself), so
// its objects stay listed rather than vanishing.
[TestClass]
public class InRoomDescriptionTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameData = new GameData(File.ReadAllBytes("inroomdescriptiontest.aslx"), "inroomdescriptiontest.aslx");
        var worldModel = Helpers.CreateWorldModel(gameData);

        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await worldModel.Begin();

        foreach (var cmd in worldModel.Walkthroughs.Walkthroughs["verify"].Steps)
        {
            if (cmd.StartsWith("assert:"))
            {
                var expr = cmd.Substring(7);
                Assert.IsTrue(await worldModel.AssertAsync(expr), expr);
            }
            else
            {
                await worldModel.SendCommand(cmd);
            }
        }
    }
}
