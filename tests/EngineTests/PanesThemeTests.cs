using System.Text;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// The pane colour schemes (panestheme) are the classic interface's (#2719). 610 Core sends them as
// --qv-pane-* tokens only to a game with classicinterface; on v2 the panes follow the game's own
// colours, and the game stylesheet can still set the tokens.
[TestClass]
public class PanesThemeTests
{
    private static async Task<Dictionary<string, string>> TokensAsync(string gameAttributes)
    {
        var aslx = $"""
            <asl version="610">
              <include ref="English.aslx"/>
              <include ref="Core.aslx"/>
              <game name="PanesThemeTest">{gameAttributes}</game>
              <object name="room">
                <inherit name="editor_room" />
                <object name="player">
                  <inherit name="defaultplayer" />
                </object>
              </object>
            </asl>
            """;
        var driver = await GameDriver.LoadAsync(new GameData(Encoding.UTF8.GetBytes(aslx), "panestheme.aslx"));
        return driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);
    }

    [TestMethod]
    public async Task OnV2_APanesTheme_SendsNoPaneTokens()
    {
        var tokens = await TokensAsync("<panestheme>Midnight</panestheme>");

        tokens.Keys.ShouldNotContain(key => key.StartsWith("--qv-pane-"));
    }

    [TestMethod]
    public async Task WithTheClassicInterface_APanesTheme_SendsItsColours()
    {
        var tokens = await TokensAsync(
            "<panestheme>Midnight</panestheme><classicinterface type=\"boolean\">true</classicinterface>");

        tokens["--qv-pane-fg"].ShouldBe("white");
        tokens["--qv-pane-bg"].ShouldBe("midnightblue");
    }
}
