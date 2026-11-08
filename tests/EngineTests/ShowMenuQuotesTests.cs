using Shouldly;

namespace QuestViva.EngineTests;

/// <summary>
/// Issue #2667: with game.showmenuresponses on, ShowMenuResponse echoed the chosen option before
/// unescaping its quotes, so the player saw "I@@@singlequote@@@m here" while the callback got
/// "I'm here". Options arrive escaped from a game carrying an older copy of ShowMenu, whose links
/// passed them through EscapeQuotes.
/// </summary>
[TestClass]
public class ShowMenuQuotesTests
{
    [TestMethod]
    [DataRow("I@@@singlequote@@@m here", "I'm here")]
    [DataRow("Say @@@doublequote@@@hi@@@doublequote@@@", "Say \"hi\"")]
    [DataRow("I'm here", "I'm here")]
    public async Task EchoedResponse_HasQuotesUnescaped(string sent, string expected)
    {
        var driver = await GameDriver.LoadAsync("showmenuquotestest.aslx");
        await driver.SendCommandAsync("menu");

        var output = await driver.SendEventAsync("ShowMenuResponse", sent);

        output.ShouldContain(">> " + expected);
        output.ShouldContain("result: " + expected);
        output.ShouldAllBe(line => !line.Contains("@@@"));
    }
}
