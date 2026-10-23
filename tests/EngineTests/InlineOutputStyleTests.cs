using QuestViva.Common;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// Output leaves out the inline font, size and colour that match the game's starting values, so
// it inherits them from the UI's tokens, which themes and stylesheets can override
// (docs/ui-v2-contract.md, "Inline output styles"). Anything that differs, such as a colour set
// mid-game, is still written inline.
[TestClass]
public class InlineOutputStyleTests
{
    private static List<string> Output(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.addText")
            .Select(i => (string)((object[])i.Arguments[1])[0])
            .ToList();

    private static string LastLineContaining(GameDriver driver, string text) =>
        Output(driver).Last(html => html.Contains(text));

    private static Dictionary<string, string> Tokens(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setTokens")
            .SelectMany(i => (IDictionary<string, string>)((object[])i.Arguments[1])[0])
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);

    [TestMethod]
    public async Task TextAtTheStartingDefaults_HasNoInlineFontSizeOrColour()
    {
        var driver = await GameDriver.LoadAsync("inlinestyletest.aslx");
        await driver.SendCommandAsync("plain");

        LastLineContaining(driver, "Plain text.").ShouldStartWith("<span style=\"\">");
    }

    [TestMethod]
    public async Task TheStartingDefaults_BecomeTextTokens()
    {
        var driver = await GameDriver.LoadAsync("inlinestyletest.aslx");

        var tokens = Tokens(driver);
        tokens["--qv-text"].ShouldBe("#111111");
        tokens["--qv-link"].ShouldBe("#0000aa");
        tokens["--qv-font-body"].ShouldBe("Georgia, serif");
        tokens["--qv-font-size"].ShouldBe("13pt");
    }

    [TestMethod]
    public async Task ALinkInTheStartingLinkColour_HasNoInlineColour()
    {
        var driver = await GameDriver.LoadAsync("inlinestyletest.aslx");
        await driver.SendCommandAsync("link");

        var line = LastLineContaining(driver, "data-elementid=\"lamp\"");
        line.ShouldContain("class=\"cmdlink elementmenu\"");
        line.ShouldContain("style=\"\"");
    }

    [TestMethod]
    public async Task AColourSetMidGame_IsStillWrittenInline()
    {
        var driver = await GameDriver.LoadAsync("inlinestyletest.aslx");
        await driver.SendCommandAsync("recolour");

        LastLineContaining(driver, "Red text.").ShouldStartWith("<span style=\"color:red;\">");
        LastLineContaining(driver, "Another").ShouldContain("style=\"color:green;\" class=\"cmdlink elementmenu\"");
    }

    [TestMethod]
    public async Task TheCommandBarStyle_KeepsTheFullFormat()
    {
        // A text input doesn't inherit the page's font, so the command bar is styled in full.
        var driver = await GameDriver.LoadAsync("inlinestyletest.aslx");

        var style = driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.setCommandBarStyle")
            .Select(i => (string)((object[])i.Arguments[1])[0])
            .Last();
        style.ShouldContain("font-family:Georgia, serif;");
        style.ShouldContain("color:#111111;");
        style.ShouldContain("font-size:13pt;");
    }

    [TestMethod]
    public async Task AfterLoadingASave_TheStartingDefaultsStillApply()
    {
        // The defaults are recorded once and kept in the save, so text written before a
        // mid-game colour change keeps inheriting the original colour after a restore.
        var driver = await GameDriver.LoadAsync("inlinestyletest.aslx");
        await driver.SendCommandAsync("recolour");

        var tempFilename = Path.GetTempFileName();
        try
        {
            var saveData = driver.Model.Save(SaveMode.SavedGame, html: null);
            await File.WriteAllTextAsync(tempFilename, saveData);

            var restored = await GameDriver.LoadAsync(tempFilename);
            Tokens(restored)["--qv-text"].ShouldBe("#111111");
            await restored.SendCommandAsync("plain");
            LastLineContaining(restored, "Plain text.").ShouldStartWith("<span style=\"color:red;\">");
        }
        finally
        {
            File.Delete(tempFilename);
        }
    }
}
