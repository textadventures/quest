using Moq;
using QuestViva.Common;
using QuestViva.Engine;
using Shouldly;

namespace QuestViva.EngineTests;

// Ask()/ShowMenu() and the 'ask'/'show menu' script commands used to render through
// IPlayer.ShowQuestion/ShowMenu, which every player implements as a jQuery UI modal. For v600+
// games the engine now draws them as numbered links in the transcript instead, matching the
// callback forms in CoreFunctions.aslx that authors can already reach from the script adder
// (#2287). Older games - including Quest 4 ones, which have no WorldModel version at all - keep
// the dialog, because "allow cancel" and disambiguation menus behave differently there and a
// published game's own inlined Core library can't be updated to match.
[TestClass]
public class InlinePromptTests
{
    [TestMethod]
    public async Task ShowMenuFunction_V600_RendersOptionsInlineWithNoDialog()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        var output = await driver.SendCommandAsync("menufn");

        output.ShouldContain("Pick one");
        output.ShouldContain("1: Red");
        output.ShouldContain("2: Green");
        output.ShouldContain("3: Blue");
        output.ShouldNotContain("menu: Red");

        // The player is still told a menu is pending - a headless IPlayer such as the walkthrough
        // runner needs that - but it is flagged as already drawn.
        driver.PlayerMock.Verify(p => p.ShowMenu(It.Is<MenuData>(m => m.Inline)), Times.Once);
    }

    [TestMethod]
    public async Task ShowMenuFunction_V600_TypedOptionNumberChoosesThatOption()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        await driver.SendCommandAsync("menufn");

        var output = await driver.SendCommandAsync("2");
        output.ShouldContain("menu: Green");
    }

    [TestMethod]
    public async Task ShowMenuFunction_V600_SetMenuResponseStillResolves()
    {
        // Clicking an option sends its number as an ordinary command, but SetMenuResponse remains
        // the programmatic path - it is what the walkthrough runner's "menu:" step calls.
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        await driver.SendCommandAsync("menufn");

        var output = await driver.SetMenuResponseAsync("Blue");
        output.ShouldContain("menu: Blue");
    }

    [TestMethod]
    public async Task ShowMenuFunction_V600_NotCancellable_IgnoresOtherInput()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        await driver.SendCommandAsync("menufn");

        var ignored = await driver.SendCommandAsync("ping");
        ignored.ShouldBeEmpty();

        // Out-of-range numbers are not options either.
        var alsoIgnored = await driver.SendCommandAsync("9");
        alsoIgnored.ShouldBeEmpty();

        // The menu is still waiting.
        var output = await driver.SendCommandAsync("1");
        output.ShouldContain("menu: Red");
    }

    [TestMethod]
    public async Task ShowMenuFunction_V600_Cancellable_OtherInputCancelsAndReturnsEmpty()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        await driver.SendCommandAsync("menufncancel");

        var output = await driver.SendCommandAsync("ping");
        // ShowMenu returned "" - the script's "menu: " line, trimmed by the test driver.
        output.ShouldContain("menu:");
        // The cancelling command itself is discarded rather than run as an ordinary turn.
        output.ShouldNotContain("pong");

        // ...and having been cancelled, the prompt no longer intercepts anything.
        var afterwards = await driver.SendCommandAsync("ping");
        afterwards.ShouldContain("pong");
    }

    [TestMethod]
    public async Task AskFunction_V600_RendersYesNoInlineAndResolvesFromTypedNumber()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        var output = await driver.SendCommandAsync("askfn");

        output.ShouldContain("Some question?");
        output.ShouldContain("1: Yes");
        output.ShouldContain("2: No");

        var answered = await driver.SendCommandAsync("2");
        answered.ShouldContain("ask: no");
    }

    [TestMethod]
    public async Task AskFunction_V600_SetQuestionResponseStillResolves()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        await driver.SendCommandAsync("askfn");

        var output = await driver.SetQuestionResponseAsync(true);
        output.ShouldContain("ask: yes");
    }

    [TestMethod]
    public async Task ShowMenuScriptCommand_V600_RendersOptionsInline()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        var output = await driver.SendCommandAsync("menucmd");

        output.ShouldContain("1: Red");
        driver.PlayerMock.Verify(p => p.ShowMenu(It.Is<MenuData>(m => m.Inline)), Times.Once);

        var chosen = await driver.SendCommandAsync("3");
        chosen.ShouldContain("menu: Blue");
    }

    [TestMethod]
    public async Task AskScriptCommand_V600_RendersYesNoInline()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        var output = await driver.SendCommandAsync("askcmd");

        output.ShouldContain("1: Yes");
        driver.PlayerMock.Verify(p => p.ShowQuestion(It.IsAny<string>(), true), Times.Once);

        var answered = await driver.SendCommandAsync("1");
        answered.ShouldContain("ask: True");
    }

    // The expression forms throw for v540-v580 and pre-v540 output does not route through
    // addText, so the script commands - available at every version - are what these two check
    // the unchanged pre-v600 path with.
    [TestMethod]
    public async Task ShowMenuScriptCommand_BelowV600_StillUsesTheDialog()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        driver.Model.Version = WorldModelVersion.v550;

        var output = await driver.SendCommandAsync("menucmd");
        output.ShouldContain("Pick one");
        output.ShouldNotContain("1: Red");
        driver.PlayerMock.Verify(p => p.ShowMenu(It.Is<MenuData>(m => !m.Inline)), Times.Once);

        // Typed input is not intercepted - there is nothing inline to intercept for.
        var ignored = await driver.SendCommandAsync("1");
        ignored.ShouldNotContain("menu: Red");

        var output2 = await driver.SetMenuResponseAsync("Red");
        output2.ShouldContain("menu: Red");
    }

    [TestMethod]
    public async Task AskScriptCommand_BelowV600_StillUsesTheDialog()
    {
        var driver = await GameDriver.LoadAsync("inlineprompttest.aslx");
        driver.Model.Version = WorldModelVersion.v550;

        var output = await driver.SendCommandAsync("askcmd");
        output.ShouldNotContain("1: Yes");
        driver.PlayerMock.Verify(p => p.ShowQuestion(It.IsAny<string>(), false), Times.Once);
    }
}
