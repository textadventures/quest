using Moq;
using QuestViva.Common;
using Shouldly;

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

    private static async Task<string> Run(GameDriver driver, string command)
    {
        return string.Join("\n", await driver.SendCommandAsync(command));
    }

    // Issue #2698: a command that names no object (LOOK, N, S) used to replace
    // game.lastobjects with an empty list, so "it" stopped working after it
    [TestMethod]
    public async Task PronounSurvivesCommandsWithNoObject()
    {
        var driver = await GameDriver.LoadAsync("pronounscopetest.aslx");
        await Run(driver, "x stick");
        await Run(driver, "look");
        await Run(driver, "n");
        await Run(driver, "s");

        (await Run(driver, "x it")).ShouldContain("A short stick.");
    }

    [TestMethod]
    public async Task PronounOutOfSightStillUnresolved()
    {
        var driver = await GameDriver.LoadAsync("pronounscopetest.aslx");
        await Run(driver, "x stick");
        await Run(driver, "n");

        var output = await Run(driver, "x it");
        output.ShouldContain("I can't see that.");
        output.ShouldNotContain("(the stick)");
        (await driver.Model.AssertAsync("ListContains(game.lastobjects, stick)")).ShouldBeTrue();
    }

    [TestMethod]
    public async Task PronounEchoesTheObjectItMeans()
    {
        var driver = await GameDriver.LoadAsync("pronounscopetest.aslx");
        (await Run(driver, "x stick")).ShouldNotContain("(the stick)");

        var output = await driver.SendCommandAsync("x it");
        output.ShouldContain("(the stick)");
        string.Join("\n", output).ShouldContain("A short stick.");

        (await Run(driver, "get it")).ShouldContain("(the stick)");
    }
}
