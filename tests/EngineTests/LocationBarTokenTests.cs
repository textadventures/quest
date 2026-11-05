using System.Text;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// The location bar's options (#2719). On v2 there's no classic location style: the bar follows the
// game's colours and takes each location bar option the game sets. The classic interface keeps
// its blue bar until the game turns off classiclocation, as before.
[TestClass]
public class LocationBarTokenTests
{
    private static async Task<Dictionary<string, string>> TokensAsync(string gameAttributes)
    {
        var aslx = $"""
            <asl version="610">
              <include ref="English.aslx"/>
              <include ref="Core.aslx"/>
              <game name="LocationBarTest">{gameAttributes}</game>
              <object name="room">
                <inherit name="editor_room" />
                <object name="player">
                  <inherit name="defaultplayer" />
                </object>
              </object>
            </asl>
            """;
        var driver = await GameDriver.LoadAsync(new GameData(Encoding.UTF8.GetBytes(aslx), "locationbar.aslx"));
        return driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);
    }

    private const string Classic = "<classicinterface type=\"boolean\">true</classicinterface>";
    private const string OwnStyle = "<classiclocation type=\"boolean\">false</classiclocation>";
    private const string Colours =
        "<customlocationcolour>Navy</customlocationcolour><customlocationtextcolour>White</customlocationtextcolour>"
        + "<customlocationbordercolour>Gold</customlocationbordercolour>";

    [TestMethod]
    public async Task OnV2_WithNoOptions_SendsNoLocationTokens()
    {
        var tokens = await TokensAsync("");

        tokens.Keys.ShouldNotContain(key => key.StartsWith("--qv-status-"));
    }

    [TestMethod]
    public async Task OnV2_TheColoursApply_WithTheClassicLocationStyleStillOn()
    {
        var tokens = await TokensAsync(Colours);

        tokens["--qv-status-bg"].ShouldBe("Navy");
        tokens["--qv-status-fg"].ShouldBe("White");
        tokens["--qv-status-border"].ShouldBe("1px solid Gold");
        tokens.ShouldNotContainKey("--qv-status-image");
    }

    [TestMethod]
    public async Task WithTheClassicInterface_TheClassicLocationStyle_KeepsTheBlueBar()
    {
        var tokens = await TokensAsync(Classic + Colours);

        tokens.Keys.ShouldNotContain(key => key.StartsWith("--qv-status-"));
    }

    [TestMethod]
    public async Task WithTheClassicInterface_TurningOffTheClassicStyle_UsesTheColoursOrNone()
    {
        var own = await TokensAsync(Classic + OwnStyle + Colours);
        own["--qv-status-bg"].ShouldBe("Navy");
        own["--qv-status-image"].ShouldBe("none");

        var none = await TokensAsync(Classic + OwnStyle);
        none["--qv-status-bg"].ShouldBe("transparent");
        none["--qv-status-border"].ShouldBe("none");
    }
}
