using System.Text;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// The game options for v2's look: accentcolour (--qv-accent), interfacefont (--qv-font-ui) and
// mapposition (QuestUI.setMapPosition). An accent is sent on its own, so a game that only sets one
// keeps the UI's light and dark colours; a game with its own colours but no accent gets its link
// colour as the accent.
[TestClass]
public class AccentAndMapPositionTests
{
    private static async Task<GameDriver> LoadAsync(string gameAttributes)
    {
        var aslx = $"""
            <asl version="610">
              <include ref="English.aslx"/>
              <include ref="Core.aslx"/>
              <game name="AccentTest">{gameAttributes}</game>
              <object name="room">
                <inherit name="editor_room" />
                <object name="player">
                  <inherit name="defaultplayer" />
                </object>
              </object>
            </asl>
            """;
        return await GameDriver.LoadAsync(new GameData(Encoding.UTF8.GetBytes(aslx), "accenttest.aslx"));
    }

    private static Dictionary<string, string> Tokens(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);

    private static List<string> MapPositions(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setMapPosition")
            .Select(i => (string)((object[])i.Arguments[1])[0])
            .ToList();

    [TestMethod]
    public async Task DefaultColours_SendNoAccent()
    {
        var tokens = Tokens(await LoadAsync(""));

        tokens.ShouldNotContainKey("--qv-accent");
        tokens.ShouldNotContainKey("--qv-font-ui");
    }

    [TestMethod]
    public async Task AnAccentOnItsOwn_LeavesTheColoursToTheUI()
    {
        var tokens = Tokens(await LoadAsync("<accentcolour>#7a3e6e</accentcolour>"));

        tokens["--qv-accent"].ShouldBe("#7a3e6e");
        tokens.ShouldNotContainKey("--qv-bg");
        tokens.ShouldNotContainKey("--qv-text");
        tokens.ShouldNotContainKey("--qv-link");
    }

    [TestMethod]
    public async Task OwnColoursWithNoAccent_UseTheLinkColour()
    {
        var tokens = Tokens(await LoadAsync("<defaultlinkforeground>Green</defaultlinkforeground>"));

        tokens["--qv-link"].ShouldBe("Green");
        tokens["--qv-accent"].ShouldBe("Green");
    }

    [TestMethod]
    public async Task OwnColoursAndAnAccent_KeepThemApart()
    {
        var tokens = Tokens(await LoadAsync("<defaultlinkforeground>Green</defaultlinkforeground><accentcolour>Purple</accentcolour>"));

        tokens["--qv-link"].ShouldBe("Green");
        tokens["--qv-accent"].ShouldBe("Purple");
    }

    [TestMethod]
    public async Task AnInterfaceFont_IsTheUIFont()
    {
        Tokens(await LoadAsync("<interfacefont>Verdana, sans-serif</interfacefont>"))["--qv-font-ui"]
            .ShouldBe("Verdana, sans-serif");
    }

    [TestMethod]
    public async Task AMapInThePanes_IsSent()
    {
        MapPositions(await LoadAsync("<gridmap type=\"boolean\">true</gridmap><mapposition>panes</mapposition>"))
            .ShouldBe(["panes"]);
    }

    [TestMethod]
    public async Task TheEditorsDefaultMapPosition_SendsNothing()
    {
        MapPositions(await LoadAsync("<gridmap type=\"boolean\">true</gridmap><mapposition></mapposition>"))
            .ShouldBeEmpty();
    }

    [TestMethod]
    public async Task AMapPositionWithNoMap_SendsNothing()
    {
        MapPositions(await LoadAsync("<mapposition>panes</mapposition>")).ShouldBeEmpty();
    }
}
