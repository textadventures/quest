using System.Text;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// InitInterface passes game.layoutpreset and game.narrowlayoutpreset to QuestUI.setLayoutPreset.
// The editor's dropdowns store "" for their default ("Beside the game", "Same as above"), which
// counts as unset, so a game left on the defaults sends nothing.
[TestClass]
public class LayoutPresetTests
{
    private static async Task<GameDriver> LoadAsync(string gameAttributes)
    {
        var aslx = $"""
            <asl version="610">
              <include ref="English.aslx"/>
              <include ref="Core.aslx"/>
              <game name="LayoutPresetTest">{gameAttributes}</game>
              <object name="room">
                <inherit name="editor_room" />
                <object name="player">
                  <inherit name="defaultplayer" />
                </object>
              </object>
            </asl>
            """;
        return await GameDriver.LoadAsync(new GameData(Encoding.UTF8.GetBytes(aslx), "layoutpresettest.aslx"));
    }

    private static List<string> Presets(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setLayoutPreset")
            .Select(i => string.Join(", ", (object[])i.Arguments[1]))
            .ToList();

    [TestMethod]
    public async Task NoPresets_SendsNothing()
    {
        Presets(await LoadAsync("")).ShouldBeEmpty();
    }

    [TestMethod]
    public async Task TheEditorsDefaults_SendNothing()
    {
        Presets(await LoadAsync("<layoutpreset></layoutpreset><narrowlayoutpreset></narrowlayoutpreset>")).ShouldBeEmpty();
    }

    [TestMethod]
    public async Task AWidePresetOnly_IsUsedAtEveryWidth()
    {
        Presets(await LoadAsync("<layoutpreset>bottombar</layoutpreset><narrowlayoutpreset></narrowlayoutpreset>"))
            .ShouldBe(["bottombar, bottombar"]);
    }

    [TestMethod]
    public async Task ANarrowPresetOnly_KeepsSidePanesWhenWide()
    {
        Presets(await LoadAsync("<layoutpreset></layoutpreset><narrowlayoutpreset>nopanes</narrowlayoutpreset>"))
            .ShouldBe(["sidepanes, nopanes"]);
    }
}
