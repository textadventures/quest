using QuestViva.Cli;

namespace QuestViva.CliTests;

// `qv walkthrough` uses a walkthrough's own seed, or its parent's for a sub-walkthrough, or else the
// default; --seed overrides them all (#2673).
[TestClass]
public class WalkthroughSeedTests
{
    // The first number a game seeded with `seed` draws: no random numbers are drawn before the
    // walkthrough's first step, which is what makes a seed reproduce a player's game.
    private static int FirstRoll(int seed) => new Random(seed).Next(1, 1000001);

    private static string Game() => $$"""
        <asl version="610">
          <include ref="English.aslx" />
          <include ref="Core.aslx" />
          <game name="Walkthrough seed test" />
          <object name="room">
            <isroom />
            <object name="player" />
          </object>
          <walkthrough name="seeded">
            <seed type="int">99</seed>
            <steps>assert:GetRandomInt(1, 1000000) = {{FirstRoll(99)}}</steps>
            <walkthrough name="seededchild">
              <steps>look</steps>
            </walkthrough>
          </walkthrough>
          <walkthrough name="unseeded">
            <steps>assert:GetRandomInt(1, 1000000) = {{FirstRoll(1234)}}</steps>
          </walkthrough>
        </asl>
        """;

    private FileInfo _game = null!;

    [TestInitialize]
    public void WriteGame()
    {
        _game = new FileInfo(Path.Combine(Path.GetTempPath(), $"qv-walkthrough-seed-{Guid.NewGuid()}.aslx"));
        File.WriteAllText(_game.FullName, Game());
    }

    [TestCleanup]
    public void DeleteGame() => _game.Delete();

    [TestMethod]
    public async Task EachWalkthroughUsesItsOwnSeedOrItsParentsOrTheDefault()
    {
        // All three in one run: the command has to start a new session for each seed.
        Assert.AreEqual(0, await WalkthroughCommand.RunAsync(_game, null, null, quiet: true));
    }

    [TestMethod]
    public async Task ASubWalkthroughUsesItsParentsSeed()
    {
        Assert.AreEqual(0, await WalkthroughCommand.RunAsync(_game, "seededchild", null, quiet: true));
    }

    [TestMethod]
    public async Task SeedOptionOverridesTheWalkthroughsSeed()
    {
        Assert.AreEqual(1, await WalkthroughCommand.RunAsync(_game, "seeded", 1234, quiet: true));
        Assert.AreEqual(0, await WalkthroughCommand.RunAsync(_game, "unseeded", 1234, quiet: true));
    }
}
