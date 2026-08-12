using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// Text Adventure "Pages" (dialogue trees) - see src/Engine/Core/CorePages.aslx. The
// mechanism deliberately mirrors GamebookCore's DoPage/HandleCommand rather than
// ShowMenu, so the game is fully idle between choices: each option is a CommandLink
// that runs as an ordinary turn, which is what makes mid-dialogue save/load work.
[TestClass]
public class DialoguePageTests
{
    private static async Task<GameDriver> LoadGameAsync()
    {
        return await GameDriver.LoadAsync("dialoguepagetest.aslx");
    }

    private static async Task AssertTrueAsync(GameDriver driver, string expr)
    {
        (await driver.Model.AssertAsync(expr)).ShouldBeTrue(expr);
    }

    [TestMethod]
    public async Task ShowPage_RendersDescriptionNumberedOptionsAndBrokenLinks_AndSetsVisited()
    {
        var driver = await LoadGameAsync();

        var output = string.Join("\n", await driver.SendCommandAsync("talk"));

        output.ShouldContain("The guard eyes you suspiciously.");
        output.ShouldContain("1: Ask about the weather");
        output.ShouldContain("2: Ask about the castle");
        output.ShouldContain("Ask about nothing (broken link)");
        await AssertTrueAsync(driver, "game.currentpage = guard_intro");
        await AssertTrueAsync(driver, "guard_intro.visited");
        await AssertTrueAsync(driver, "not guard_weather.visited");
    }

    [TestMethod]
    public async Task ChoosingAnOption_ByKeyOrTypedNumber_MovesThroughThePageGraph()
    {
        var driver = await LoadGameAsync();
        await driver.SendCommandAsync("talk");

        // By option key (what the rendered CommandLink sends)
        var output = string.Join("\n", await driver.SendCommandAsync("guard_weather"));
        output.ShouldContain("Ask about the weather"); // choice echoed
        output.ShouldContain("Nice weather today.");
        output.ShouldContain("1: Back");
        await AssertTrueAsync(driver, "game.currentpage = guard_weather");

        // By typed option number (an improvement over gamebook, matching ShowMenu)
        output = string.Join("\n", await driver.SendCommandAsync("1"));
        output.ShouldContain("The guard eyes you suspiciously.");
        await AssertTrueAsync(driver, "game.currentpage = guard_intro");
    }

    [TestMethod]
    public async Task PageWithNoOptions_EndsTheDialogue()
    {
        var driver = await LoadGameAsync();
        await driver.SendCommandAsync("talk");

        var output = string.Join("\n", await driver.SendCommandAsync("2"));
        output.ShouldContain("The castle is closed.");
        await AssertTrueAsync(driver, "game.currentpage = null");

        // and the game is back to normal command handling
        var after = string.Join("\n", await driver.SendCommandAsync("marker"));
        after.ShouldContain("marker ran");
    }

    [TestMethod]
    public async Task NonOptionCommand_WithCancelForbidden_PromptsAndStaysInDialogue()
    {
        var driver = await LoadGameAsync();
        await driver.SendCommandAsync("talk");

        var output = string.Join("\n", await driver.SendCommandAsync("marker"));
        output.ShouldContain("Please choose one of the options above.");
        output.ShouldNotContain("marker ran");
        await AssertTrueAsync(driver, "game.currentpage = guard_intro");

        // out-of-range numbers are not options either
        output = string.Join("\n", await driver.SendCommandAsync("9"));
        output.ShouldContain("Please choose one of the options above.");
        await AssertTrueAsync(driver, "game.currentpage = guard_intro");
    }

    [TestMethod]
    public async Task NonOptionCommand_WithCancelAllowed_EndsDialogueAndRunsTheCommand()
    {
        var driver = await LoadGameAsync();
        await driver.SendCommandAsync("talkcancel");

        var output = string.Join("\n", await driver.SendCommandAsync("marker"));
        output.ShouldContain("marker ran");
        await AssertTrueAsync(driver, "game.currentpage = null");
    }

    [TestMethod]
    public async Task TurnScripts_AreSuppressedDuringDialogue_UnlessOptedIn()
    {
        var driver = await LoadGameAsync();

        // a normal turn ticks
        var output = string.Join("\n", await driver.SendCommandAsync("marker"));
        output.ShouldContain("tick");

        // flag off: no ticks on the launch turn, choices, or the final choice
        output = string.Join("\n", await driver.SendCommandAsync("talk"));
        output.ShouldNotContain("tick");
        output = string.Join("\n", await driver.SendCommandAsync("1"));
        output.ShouldNotContain("tick");
        output = string.Join("\n", await driver.SendCommandAsync("1"));
        output.ShouldNotContain("tick");
        output = string.Join("\n", await driver.SendCommandAsync("2")); // terminal page
        output.ShouldNotContain("tick");
        await AssertTrueAsync(driver, "game.currentpage = null");

        // back to normal turns
        output = string.Join("\n", await driver.SendCommandAsync("marker"));
        output.ShouldContain("tick");

        // flag on: every choice is a full turn
        output = string.Join("\n", await driver.SendCommandAsync("talkturns"));
        output.ShouldContain("tick");
        output = string.Join("\n", await driver.SendCommandAsync("1"));
        output.ShouldContain("tick");
    }

    [TestMethod]
    public async Task ScriptOnlyPage_CanRedirect_OrEndTheDialogue()
    {
        var driver = await LoadGameAsync();

        // script page that redirects via GoToPage
        var output = string.Join("\n", await driver.SendCommandAsync("talkscriptredirect"));
        output.ShouldContain("redirect script ran");
        output.ShouldContain("Nice weather today.");
        await AssertTrueAsync(driver, "game.currentpage = guard_weather");
        await driver.SendCommandAsync("1"); // Back
        await driver.SendCommandAsync("2"); // castle, ends dialogue

        // script page that doesn't chain anywhere ends the dialogue
        output = string.Join("\n", await driver.SendCommandAsync("talkscriptend"));
        output.ShouldContain("end script ran");
        await AssertTrueAsync(driver, "game.currentpage = null");
    }

    [TestMethod]
    public async Task ScriptTextPage_RunsScriptThenRendersTextAndOptions()
    {
        var driver = await LoadGameAsync();

        var output = string.Join("\n", await driver.SendCommandAsync("talkscripttext"));
        output.ShouldContain("scripttext script ran");
        output.ShouldContain("Text after the script.");
        output.ShouldContain("1: Go to the castle page");
        await AssertTrueAsync(driver, "game.currentpage = guard_scripttext");
    }

    [TestMethod]
    public async Task PagesAreInvisibleToWorldScoping()
    {
        var driver = await LoadGameAsync();

        // guard_intro sits inside the player's room, but pages are data, not objects:
        // it must not be reachable by LOOK AT even before/outside any dialogue
        var output = string.Join("\n", await driver.SendCommandAsync("look at guard_intro"));
        output.ShouldNotContain("The guard eyes you suspiciously.");
        await AssertTrueAsync(driver, "game.currentpage = null");
    }

    [TestMethod]
    public async Task PageLinkInObjectDescription_RendersAndLaunchesTheDialogueWhenClicked()
    {
        var driver = await LoadGameAsync();

        // {page:guard_intro:talk to the guard} in signpost's description - not reached
        // via any active dialogue's own rendered options.
        var lookOutput = string.Join("\n", await driver.SendCommandAsync("look at signpost"));
        lookOutput.ShouldContain("talk to the guard");
        await AssertTrueAsync(driver, "game.currentpage = null");

        // Clicking that link sends the target page's raw object name as an ordinary
        // command (same as {command:...}) - previously this fell through to
        // "I don't understand your command" instead of starting the dialogue.
        var clickOutput = string.Join("\n", await driver.SendCommandAsync("guard_intro"));
        clickOutput.ShouldNotContain("I don't understand");
        clickOutput.ShouldContain("The guard eyes you suspiciously.");
        await AssertTrueAsync(driver, "game.currentpage = guard_intro");
    }

    [TestMethod]
    public async Task SaveAndLoadMidDialogue_PreservesDialogueStateAndOptions()
    {
        var driver = await LoadGameAsync();
        await driver.SendCommandAsync("talk");
        await driver.SendCommandAsync("guard_weather");
        await AssertTrueAsync(driver, "game.currentpage = guard_weather");

        var tempFilename = Path.GetTempFileName();
        try
        {
            var saveData = driver.Model.Save(SaveMode.SavedGame, html: null);
            await File.WriteAllTextAsync(tempFilename, saveData);

            var restored = await GameDriver.LoadAsync(tempFilename);
            await AssertTrueAsync(restored, "game.currentpage = guard_weather");
            await AssertTrueAsync(restored, "guard_intro.visited");

            // options are still selectable after the reload...
            var output = string.Join("\n", await restored.SendCommandAsync("1"));
            output.ShouldContain("The guard eyes you suspiciously.");
            await AssertTrueAsync(restored, "game.currentpage = guard_intro");

            // ...and the allowCancel flag survived too (cancel was forbidden)
            output = string.Join("\n", await restored.SendCommandAsync("marker"));
            output.ShouldContain("Please choose one of the options above.");
            output.ShouldNotContain("marker ran");
        }
        finally
        {
            File.Delete(tempFilename);
        }
    }
}
