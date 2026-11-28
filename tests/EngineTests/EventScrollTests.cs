using System.Text;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// An event ends a turn as far as the output goes, so the player scrolls to what it printed: a
// ShowMenu option's response used to be left below the window, since with 610 Core text is added
// without scrolling and only the end of a turn scrolls.
[TestClass]
public class EventScrollTests
{
    private static async Task<GameDriver> LoadAsync()
    {
        const string aslx = """
            <asl version="610">
              <include ref="English.aslx"/>
              <include ref="Core.aslx"/>
              <game name="EventScrollTest"/>
              <object name="room">
                <inherit name="editor_room" />
                <object name="player">
                  <inherit name="defaultplayer" />
                </object>
              </object>
              <command name="talk">
                <pattern>talk</pattern>
                <script>
                  ShowMenu ("What do you say?", Split("a;b", ";"), false) {
                    msg ("You said " + result + ".")
                  }
                </script>
              </command>
              <function name="Silent" parameters="param">
              </function>
            </asl>
            """;
        return await GameDriver.LoadAsync(new GameData(Encoding.UTF8.GetBytes(aslx), "eventscrolltest.aslx"));
    }

    private static List<string> ScrollCalls(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync))
            .Select(i => (string)i.Arguments[0])
            .Where(fn => fn is "scrollToEnd" or "markScrollPosition")
            .ToList();

    [TestMethod]
    public async Task ShowMenuResponse_ScrollsToItsOutput_ThenMarksTheNextTurn()
    {
        var driver = await LoadAsync();
        await driver.SendCommandAsync("talk");
        driver.PlayerMock.Invocations.Clear();

        var output = await driver.SendEventAsync("ShowMenuResponse", "a");

        output.ShouldContain("You said a.");
        ScrollCalls(driver).ShouldBe(["scrollToEnd", "markScrollPosition"]);
    }

    [TestMethod]
    public async Task AnyOtherEvent_ScrollsOnce()
    {
        var driver = await LoadAsync();
        driver.PlayerMock.Invocations.Clear();

        await driver.SendEventAsync("Silent", "");

        ScrollCalls(driver).ShouldBe(["scrollToEnd", "markScrollPosition"]);
    }
}
