using System.Text;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// A gamebook's walkthrough steps, written the way a text adventure's are: GamebookCore.aslx implies
// simplestringlist for them as Core.aslx does, so a <steps> element with no type attribute is a
// list of steps, not one string that ran as no steps at all.
[TestClass]
public class GamebookWalkthroughTests
{
    private static string Game(string steps) => $"""
        <asl version="610" templatetype="gamebook">
          <include ref="GamebookCore.aslx"/>
          <game name="Gamebook walkthrough test"/>
          <object name="Page1">
            <object name="player"><inherit name="defaultplayer"/></object>
            <description>Start.</description>
            <options type="simplestringdictionary">Page2 = Go on</options>
          </object>
          <object name="Page2">
            <description>The end.</description>
          </object>
          <walkthrough name="test">
            {steps}
          </walkthrough>
        </asl>
        """;

    private const string UntypedSteps = """
        <steps>
              Page2
              assert:false
            </steps>
        """;

    private static GameData GameData(string xml) => new(Encoding.UTF8.GetBytes(xml), "gamebookwalkthroughtest.aslx");

    private static string[] Steps(IWalkthroughs walkthroughs) =>
        walkthroughs.Walkthroughs["test"].Steps.Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();

    [TestMethod]
    public async Task UntypedStepsAreAList()
    {
        var driver = await GameDriver.LoadAsync(GameData(Game(UntypedSteps)));

        Steps(driver.Model.Walkthroughs).ShouldBe(["Page2", "assert:false"]);
    }

    [TestMethod]
    public async Task UntypedStepsSurviveAnEditorSave()
    {
        // The walkthrough saver only writes steps that are a list, so before the implied type an
        // editor save of a gamebook like this one lost its steps.
        var model = Helpers.CreateWorldModel(GameData(Game(UntypedSteps)));
        (await model.InitialiseEdit()).ShouldBeTrue();
        var saved = model.Save(SaveMode.Editor);

        var driver = await GameDriver.LoadAsync(GameData(saved));
        Steps(driver.Model.Walkthroughs).ShouldBe(["Page2", "assert:false"]);
    }

    [TestMethod]
    public async Task StepsThatArentAListAreAnError()
    {
        // As a gamebook saved before the implied type loads: its steps are a single string.
        var driver = await GameDriver.LoadAsync(GameData(Game("<steps type=\"string\">Page2</steps>")));

        var ex = Should.Throw<InvalidOperationException>(() => driver.Model.Walkthroughs.Walkthroughs["test"].Steps);
        ex.Message.ShouldContain("'test'");
        ex.Message.ShouldContain("simplestringlist");
    }
}
