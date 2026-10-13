using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

// Regression coverage for issue #2543: a pronoun ("it") referring to the previous
// command's object only matched objects in the command's own scope, so GET IT for an
// object already held failed with "I can't see that", while GET STICK fell through
// to the visible-objects secondary scope and said "You are already carrying it".
// Pronouns now get the same secondary-scope fallback as names, but still only for
// objects that are actually visible (discussion #1669).
[TestClass]
public class PronounScopeTests
{
    [TestMethod]
    public async Task RunWalkthrough()
    {
        var gameData = new GameData(File.ReadAllBytes("pronounscopetest.aslx"), "pronounscopetest.aslx");
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
