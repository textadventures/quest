using System.Text;
using QuestViva.Common;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

[TestClass]
public class GameRandomTests
{
    private const string Game = """
        <asl version="610">
          <include ref="English.aslx"/>
          <include ref="Core.aslx"/>
          <game name="Random Test"/>
          <object name="room"><isroom/><object name="player"/></object>
        </asl>
        """;

    private static GameData GameData() => new(Encoding.UTF8.GetBytes(Game), "randomtest.aslx");

    // Draws a mix of samples: one each from a small range and NextDouble, two from a range wider
    // than int.MaxValue.
    private static List<double> Draw(GameRandom random, int count)
    {
        var result = new List<double>();
        for (var i = 0; i < count; i++)
        {
            result.Add(random.Next(1, 1000001));
            result.Add(random.NextDouble());
            result.Add(random.Next(int.MinValue, int.MaxValue));
        }

        return result;
    }

    private static async Task<int> Roll(GameDriver driver) =>
        (int)(await driver.Model.EvaluateAsync("GetRandomInt(1, 1000000)"))!;

    [TestMethod]
    public void ASeed_GivesTheSameNumbersAsSystemRandom()
    {
        // So seeds that already gave a known transcript, such as quest-e2e-tests' goldens, still do.
        var random = new GameRandom(1234);
        var reference = new Random(1234);

        for (var i = 0; i < 5; i++)
        {
            random.Next(1, 1000001).ShouldBe(reference.Next(1, 1000001));
            random.NextDouble().ShouldBe(reference.NextDouble());
            random.Next(int.MinValue, int.MaxValue).ShouldBe(reference.Next(int.MinValue, int.MaxValue));
        }

        random.Seed.ShouldBe(1234);
    }

    [TestMethod]
    public void Restore_CarriesOnFromWhereTheSequenceGotTo()
    {
        var random = new GameRandom(42);
        Draw(random, 10);
        var samples = random.Samples;
        samples.ShouldBe(40);
        var expected = Draw(random, 10);

        var restored = new GameRandom();
        restored.Restore(42, samples);

        restored.Seed.ShouldBe(42);
        restored.Samples.ShouldBe(samples);
        Draw(restored, 10).ShouldBe(expected);
    }

    [TestMethod]
    public void AnUnseededGame_StillHasASeedThatRepeatsItsNumbers()
    {
        var random = new GameRandom();
        var first = Draw(random, 5);

        var replay = new GameRandom(random.Seed);
        Draw(replay, 5).ShouldBe(first);
    }

    [TestMethod]
    public void Reseed_StartsTheSequenceAgain()
    {
        var random = new GameRandom(7);
        var first = Draw(random, 3);

        random.Reseed(7);

        random.Samples.ShouldBe(0);
        Draw(random, 3).ShouldBe(first);
    }

    [TestMethod]
    public async Task ALoadedSave_CarriesOnWithTheSameRandomNumbers()
    {
        var driver = await GameDriver.LoadAsync(GameData());
        driver.Model.SetRandomSeed(99);
        for (var i = 0; i < 3; i++) await Roll(driver);

        var saveData = driver.Model.Save(SaveMode.SavedGame, html: null);
        var expected = new List<int>();
        for (var i = 0; i < 3; i++) expected.Add(await Roll(driver));

        var loaded = await GameDriver.LoadAsync(GameData(), saveData: new MemoryStream(Encoding.UTF8.GetBytes(saveData)));
        loaded.Model.RandomSeed.ShouldBe(99);
        var actual = new List<int>();
        for (var i = 0; i < 3; i++) actual.Add(await Roll(loaded));

        actual.ShouldBe(expected);
    }

    [TestMethod]
    public async Task ASaveWithoutRandomState_StillLoads()
    {
        // Saves from before the random state was stored: the loaded game gets a seed of its own.
        var driver = await GameDriver.LoadAsync(GameData());
        var saveData = driver.Model.Save(SaveMode.SavedGame, html: null);
        saveData.ShouldContain("randomseed=");
        var oldSave = System.Text.RegularExpressions.Regex.Replace(saveData, " random(seed|samples)=\"[^\"]*\"", "");
        oldSave.ShouldNotContain("randomseed=");

        var loaded = await GameDriver.LoadAsync(GameData(), saveData: new MemoryStream(Encoding.UTF8.GetBytes(oldSave)));

        (await Roll(loaded)).ShouldBeInRange(1, 1000000);
    }
}
