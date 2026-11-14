using System.Text;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// A walkthrough's optional seed (#2673): its own, or else its parent's, since a sub-walkthrough runs
// its parent's steps first.
[TestClass]
public class WalkthroughSeedTests
{
    private const string Game = """
        <asl version="610">
          <include ref="English.aslx"/>
          <include ref="Core.aslx"/>
          <game name="Walkthrough seed test"/>
          <object name="room"><isroom/><object name="player"/></object>
          <walkthrough name="parent">
            <seed type="int">99</seed>
            <steps>look</steps>
            <walkthrough name="inherits">
              <steps>look</steps>
            </walkthrough>
            <walkthrough name="overrides">
              <seed type="int">7</seed>
              <steps>look</steps>
            </walkthrough>
          </walkthrough>
          <walkthrough name="none">
            <steps>look</steps>
          </walkthrough>
        </asl>
        """;

    private static GameData GameData(string xml) => new(Encoding.UTF8.GetBytes(xml), "walkthroughseedtest.aslx");

    private static IDictionary<string, IWalkthrough> Walkthroughs(GameDriver driver) =>
        driver.Model.Walkthroughs.Walkthroughs;

    [TestMethod]
    public async Task AWalkthroughUsesItsOwnSeedOrItsParents()
    {
        var walkthroughs = Walkthroughs(await GameDriver.LoadAsync(GameData(Game)));

        walkthroughs["parent"].Seed.ShouldBe(99);
        walkthroughs["inherits"].Seed.ShouldBe(99);
        walkthroughs["overrides"].Seed.ShouldBe(7);
        walkthroughs["none"].Seed.ShouldBeNull();
    }

    [TestMethod]
    public async Task TheSeedSurvivesAnEditorSave()
    {
        var model = Helpers.CreateWorldModel(GameData(Game));
        (await model.InitialiseEdit()).ShouldBeTrue();
        var saved = model.Save(SaveMode.Editor);

        var walkthroughs = Walkthroughs(await GameDriver.LoadAsync(GameData(saved)));
        walkthroughs["parent"].Seed.ShouldBe(99);
        walkthroughs["overrides"].Seed.ShouldBe(7);
        walkthroughs["none"].Seed.ShouldBeNull();
    }
}
