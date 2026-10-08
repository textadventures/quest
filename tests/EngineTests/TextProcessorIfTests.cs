using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

// Regression coverage for issue #2351: {if} used to replace every "this" in its condition
// with the object's name, including inside words and values, so with "this" set to teapot,
// {if this.colour=thistle:...} became teapot.colour=teapottle and never matched.
[TestClass]
public class TextProcessorIfTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameData = new GameData(File.ReadAllBytes("textprocessoriftest.aslx"), "textprocessoriftest.aslx");
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
