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

    private static Dictionary<string, string> Tokens(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);

    [TestMethod]
    public async Task ATextColourFromTheEditor_GivesTheGameItsOwnColours()
    {
        // A new game follows the player's light or dark mode until it has colours of its own
        var driver = await LoadAsync();
        Tokens(driver).ShouldNotContainKey("--qv-text");
        driver.PlayerMock.Invocations.Clear();

        driver.Model.SetGameAttributeFromEditor("defaultforeground", "DarkRed");
        await driver.Model.ReapplyInterfaceAsync("<p>so far</p>");

        var tokens = Tokens(driver);
        tokens["--qv-text"].ShouldBe("DarkRed");
        tokens["--qv-bg"].ShouldBe("White");
        tokens["--qv-link"].ShouldBe("Blue");
    }

    [TestMethod]
    public async Task ClearingTheTextColourInTheEditor_FollowsThePlayersModeAgain()
    {
        var driver = await LoadAsync("<defaultlinkforeground>Purple</defaultlinkforeground>");
        Tokens(driver)["--qv-link"].ShouldBe("Purple");
        driver.PlayerMock.Invocations.Clear();

        driver.Model.SetGameAttributeFromEditor("defaultlinkforeground", null);
        await driver.Model.ReapplyInterfaceAsync("<p>so far</p>");

        Tokens(driver).ShouldNotContainKey("--qv-link");
        Tokens(driver).ShouldNotContainKey("--qv-text");
    }

    [TestMethod]
    public async Task AFontFromTheEditor_BecomesTheTextToken()
    {
        var driver = await LoadAsync();
        driver.PlayerMock.Invocations.Clear();

        driver.Model.SetGameAttributeFromEditor("defaultfont", "Verdana, sans-serif");
        await driver.Model.ReapplyInterfaceAsync("<p>so far</p>");

        Tokens(driver)["--qv-font-body"].ShouldBe("Verdana, sans-serif");
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
