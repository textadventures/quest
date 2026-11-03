using System.Text;
using QuestViva.Common;
using QuestViva.Engine;
using Shouldly;

namespace QuestViva.EngineTests;

// The editor's live Preview (#2650 part 5): Interface tab changes are copied onto the running game
// (SetGameAttributeFromEditor), then a fresh UI is set up as a restore would (ReapplyInterfaceAsync),
// without reloading the game, so it carries on where it was.
[TestClass]
public class ReapplyInterfaceTests
{
    private static async Task<GameDriver> LoadAsync(string gameAttributes = "")
    {
        var aslx = $$"""
            <asl version="610">
              <include ref="English.aslx"/>
              <include ref="Core.aslx"/>
              <game name="ReapplyInterfaceTest">{{gameAttributes}}</game>
              <object name="room">
                <inherit name="editor_room" />
                <object name="player">
                  <inherit name="defaultplayer" />
                </object>
                <object name="lamp">
                  <take />
                </object>
              </object>
              <command name="waitcmd">
                <pattern>pause</pattern>
                <script>
                  wait {
                    msg ("resumed")
                  }
                </script>
              </command>
            </asl>
            """;
        return await GameDriver.LoadAsync(new GameData(Encoding.UTF8.GetBytes(aslx), "reapplyinterfacetest.aslx"));
    }

    private static List<string> Calls(GameDriver driver, string function) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync) && (string)i.Arguments[0] == function)
            .Select(i => string.Join(", ", ((object?[]?)i.Arguments[1] ?? []).Select(a => a?.ToString())))
            .ToList();

    [TestMethod]
    public async Task ReRunsInitInterface_WithTheNewAttributes()
    {
        var driver = await LoadAsync("<layoutpreset>bottombar</layoutpreset>");
        driver.PlayerMock.Invocations.Clear();

        driver.Model.SetGameAttributeFromEditor("layoutpreset", "nopanes");
        await driver.Model.ReapplyInterfaceAsync("<p>so far</p>");

        Calls(driver, "QuestUI.setLayoutPreset").ShouldBe(["nopanes, nopanes"]);
    }

    [TestMethod]
    public async Task PutsBackTheOutput_AndSendsTheLists()
    {
        var driver = await LoadAsync();
        driver.PlayerMock.Invocations.Clear();

        await driver.Model.ReapplyInterfaceAsync("<p>so far</p>");

        Calls(driver, "loadHtml").ShouldBe(["<p>so far</p>"]);
        Calls(driver, "QuestUI.setStatusAttributes").Count.ShouldBe(1);
        driver.PlayerMock.Invocations.ShouldContain(i => i.Method.Name == nameof(IPlayer.SetTurnPending)
                                                         && (bool)i.Arguments[0] == false);
    }

    [TestMethod]
    public async Task KeepsTheGameWhereItWas()
    {
        var driver = await LoadAsync();
        await driver.SendCommandAsync("take lamp");

        await driver.Model.ReapplyInterfaceAsync("");

        driver.Model.Elements.Get("lamp").Parent!.Name.ShouldBe("player");
        (await driver.SendCommandAsync("drop lamp")).ShouldNotBeEmpty();
    }

    [TestMethod]
    public async Task DoesNothing_WhileATurnIsHeld()
    {
        var driver = await LoadAsync();
        await driver.SendCommandAsync("pause");
        driver.PlayerMock.Invocations.Clear();

        await driver.Model.ReapplyInterfaceAsync("<p>so far</p>");

        Calls(driver, "loadHtml").ShouldBeEmpty();
        (await driver.FinishWaitAsync()).ShouldContain("resumed");
    }

    [TestMethod]
    public async Task SetGameAttributeFromEditor_KeepsADoubleADouble()
    {
        var model = (await LoadAsync("<backgroundopacity type=\"double\">0.5</backgroundopacity>")).Model;

        model.SetGameAttributeFromEditor("backgroundopacity", 1);

        model.Game.Fields.Get("backgroundopacity").ShouldBe(1.0);
    }

    [TestMethod]
    public async Task SetGameAttributeFromEditor_NullInheritsAgain()
    {
        var driver = await LoadAsync("<accentcolour>red</accentcolour>");

        driver.Model.SetGameAttributeFromEditor("accentcolour", null);
        driver.Model.SetGameAttributeFromEditor("notset", null);

        driver.Model.Game.Fields.GetAttributeNames(false).ShouldNotContain("accentcolour");
    }
}
