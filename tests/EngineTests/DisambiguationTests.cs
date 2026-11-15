using Shouldly;

namespace QuestViva.EngineTests;

/// <summary>
/// Issue #2842: the "Do you mean...?" menu listed partial matches alongside two or more full
/// matches, although with exactly one full match the partial matches are ignored. Partial
/// matches are now offered only when nothing matches the whole name.
/// </summary>
[TestClass]
public class DisambiguationTests
{
    private static async Task<string[]> MenuFor(string command)
    {
        var driver = await GameDriver.LoadAsync("disambiguationtest.aslx");
        await driver.SendCommandAsync(command);
        (await driver.Model.AssertAsync("game.disambiguating")).ShouldBeTrue();

        var names = new List<string>();
        foreach (var name in new[] { "red_key", "blue_key", "key_ring", "box_lid", "boxing_glove" })
        {
            if (await driver.Model.AssertAsync($"ListContains(game.menuoptions, {name})"))
            {
                names.Add(name);
            }
        }
        return names.ToArray();
    }

    [TestMethod]
    public async Task TwoFullMatches_MenuLeavesOutPartialMatches()
    {
        (await MenuFor("x key")).ShouldBe(["red_key", "blue_key"], ignoreOrder: true);
    }

    [TestMethod]
    public async Task NoFullMatches_MenuListsPartialMatches()
    {
        (await MenuFor("x box")).ShouldBe(["box_lid", "boxing_glove"], ignoreOrder: true);
    }
}
