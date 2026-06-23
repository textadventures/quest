using Moq;
using QuestViva.Common;

namespace QuestViva.EngineTests;

public abstract class WalkthroughBase
{
    protected abstract bool UseNCalc { get; }

    protected async Task RunWalkthroughInternal()
    {
        var gameDataProvider = new FileGameDataProvider(Path.Combine("..", "..", "..", "walkthrough.aslx"));
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData, UseNCalc);

        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        await worldModel.Initialise(player.Object);
        worldModel.Begin();

        foreach (var cmd in worldModel.Walkthroughs.Walkthroughs["debug"].Steps)
        {
            if (cmd.StartsWith("assert:"))
            {
                var expr = cmd.Substring(7);
                Assert.IsTrue(worldModel.Assert(expr), expr);
            }
            else
            {
                worldModel.SendCommand(cmd);
            }
        }
    }
}

[TestClass]
public class Walkthrough : WalkthroughBase
{
    protected override bool UseNCalc => false;

    [TestMethod]
    public async Task RunWalkthrough() => await RunWalkthroughInternal();
}

[TestClass]
public class NCalcWalkthrough : WalkthroughBase
{
    protected override bool UseNCalc => true;

    [TestMethod]
    public async Task RunWalkthrough() => await RunWalkthroughInternal();
}