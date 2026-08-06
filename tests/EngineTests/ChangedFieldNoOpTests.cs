using QuestViva.Engine;
using Shouldly;

namespace QuestViva.EngineTests;

// Regression test for https://github.com/alexwarren/quest/issues/1988: Element.SetFieldAsync
// used to dispatch changed<field> unconditionally after every write, even when the new value
// equalled the old one. Classic Quest 5 only raises AttributeChanged - and so only fires
// changed<field> - when the value actually changes. A same-value write (e.g. an "enter" script
// re-running MoveObject into the room the player is already in) would otherwise recurse forever.
[TestClass]
public class ChangedFieldNoOpTests
{
    [TestMethod]
    public async Task SameValueWrite_DoesNotFireChangedScript()
    {
        var driver = await GameDriver.LoadAsync("recursiontest.aslx");

        await driver.SendCommandAsync("triggernoop");

        var watcher = driver.Model.Elements.Get("watcher");
        watcher.Fields.Get("hits").ShouldBe(0);
    }

    [TestMethod]
    public async Task DifferentValueWrite_StillFiresChangedScript()
    {
        var driver = await GameDriver.LoadAsync("recursiontest.aslx");

        await driver.SendCommandAsync("triggerrealchange");

        var watcher = driver.Model.Elements.Get("watcher");
        watcher.Fields.Get("hits").ShouldBe(1);
    }
}
