using Shouldly;

namespace QuestViva.EngineTests;

// LOOK IN / LOOK INSIDE <object> is a Core verb (property "lookin"). Containers get a default
// script from container_base that lists their contents; anything else gets DefaultLookIn.
[TestClass]
public class LookInTests
{
    private static async Task<string> Run(GameDriver driver, string command)
    {
        return string.Join("\n", await driver.SendCommandAsync(command));
    }

    [TestMethod]
    public async Task OpenContainerListsContentsAndRevealsHiddenChildren()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        driver.Model.Elements.Get("box").Fields.GetAsType<bool>("hidechildren").ShouldBeTrue();

        var output = await Run(driver, "look in box");

        output.ShouldContain("It contains an apple and a pear.");
        driver.Model.Elements.Get("box").Fields.GetAsType<bool>("hidechildren").ShouldBeFalse();
    }

    [TestMethod]
    public async Task LookInsideIsASynonym()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look inside box")).ShouldContain("It contains an apple and a pear.");
    }

    [TestMethod]
    public async Task ClosedOpaqueContainer()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        var output = await Run(driver, "look in chest");
        output.ShouldContain("It is closed.");
        output.ShouldNotContain("gold");
    }

    [TestMethod]
    public async Task ClosedTransparentContainer()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in jar")).ShouldContain("It contains a marble.");
    }

    [TestMethod]
    public async Task EmptyContainer()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in bucket")).ShouldContain("It is empty.");
    }

    [TestMethod]
    public async Task NonContainer()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in rock")).ShouldContain("You can't see inside it.");
    }

    [TestMethod]
    public async Task PerObjectStringOverride()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in mirror")).ShouldContain("You see your own reflection.");
    }

    [TestMethod]
    public async Task PerObjectScriptOverridesContainerDefault()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        var output = await Run(driver, "look in cupboard");
        output.ShouldContain("A moth flies out.");
        output.ShouldNotContain("contains");
    }

    [TestMethod]
    public async Task DarkRoom()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        await Run(driver, "in");

        // A weak light source can be seen, and looked inside, without lighting the room, but only
        // contents that give off light themselves are listed. Nothing visible isn't "empty".
        (await Run(driver, "look in lantern")).ShouldContain("It contains a candle.");
        (await Run(driver, "look in tin")).ShouldContain("It is too dark to make anything out.");
        (await Run(driver, "look in crate")).ShouldNotContain("bottle");
        var output = await Run(driver, "look in pouch");
        output.ShouldContain("It is too dark to make anything out.");
        output.ShouldNotContain("coin");
    }

    [TestMethod]
    public async Task BareLookInStillReachesTheInLookExit()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in")).ShouldContain("Steps lead down.");
    }

    [TestMethod]
    public async Task GameCommandBeatsCoreVerb()
    {
        var driver = await GameDriver.LoadAsync("lookinoverridetest.aslx");
        (await Run(driver, "look in box")).ShouldContain("Game command looks in box.");
    }

    [TestMethod]
    public async Task GameVerbBeatsCoreVerb()
    {
        var driver = await GameDriver.LoadAsync("lookinoverridetest.aslx");
        (await Run(driver, "look inside rock")).ShouldContain("Game verb: nothing inside rock.");
    }
}
