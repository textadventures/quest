using QuestViva.Common;
using QuestViva.PlayerCore;

namespace QuestViva.PlayerCoreTests;

[TestClass]
public class HeadlessSessionTests
{
    private static Task<HeadlessSession> StartAsync(string filename = "headless.aslx") =>
        HeadlessSession.StartAsync(new GameData(File.ReadAllBytes(filename), filename));

    [TestMethod]
    public async Task OpeningHasStartText()
    {
        var session = await StartAsync();
        StringAssert.Contains(session.Opening.Text, "Welcome.");
        StringAssert.Contains(session.Opening.Text, "A plain room.");
        Assert.IsFalse(session.Opening.Finished);
    }

    [TestMethod]
    public async Task CommandReturnsOnlyThatTurnsText()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testtimeout");
        Assert.DoesNotContain("Welcome.", step.Text);
        StringAssert.Contains(step.Text, "set");
    }

    [TestMethod]
    public async Task WaitIsAdvancedAutomatically()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testwait");
        Assert.AreEqual("> testwait\nbefore wait\nafter wait\n", step.Text);
    }

    [TestMethod]
    public async Task SynchronousSoundIsAdvancedAutomatically()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testsound");
        StringAssert.Contains(step.Text, "after sound");
    }

    [TestMethod]
    public async Task SetTimeoutIsDrainedWithTheStep()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testtimeout");
        StringAssert.Contains(step.Text, "timeout fired");
    }

    [TestMethod]
    public async Task OtherTimersOnlyRunOnTick()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testclock");
        Assert.DoesNotContain("tock", step.Text);

        step = await session.TickAsync(10);
        StringAssert.Contains(step.Text, "tock");
    }

    [TestMethod]
    [DataRow("g")]
    [DataRow("green")]
    [DataRow("2")]
    public async Task MenuAnswerByKeyTextOrNumber(string answer)
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testmenu");
        Assert.IsNotNull(step.PendingMenu);
        Assert.AreEqual("Pick one", step.PendingMenu.Caption);

        step = await session.SendAsync(answer);
        Assert.IsNull(step.PendingMenu);
        StringAssert.Contains(step.Text, "chose g");
    }

    [TestMethod]
    public async Task UnmatchedAnswerCancelsACancellableMenu()
    {
        var session = await StartAsync();
        await session.SendAsync("testmenu");
        var step = await session.SendAsync("blue");
        Assert.IsNull(step.PendingMenu);
        Assert.DoesNotContain("chose r", step.Text);
        Assert.DoesNotContain("chose g", step.Text);
    }

    [TestMethod]
    public async Task UnmatchedAnswerLeavesAnUncancellableMenuPending()
    {
        var session = await StartAsync();
        await session.SendAsync("testmenunocancel");
        var step = await session.SendAsync("blue");
        Assert.IsNotNull(step.PendingMenu);
        Assert.IsEmpty(step.Output);

        step = await session.SendAsync("1");
        Assert.IsNull(step.PendingMenu);
        StringAssert.Contains(step.Text, "chose r");
    }

    [TestMethod]
    public async Task QuestionAnswer()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testask");
        Assert.AreEqual("Sure?", step.PendingQuestion);

        step = await session.SendAsync("y");
        Assert.IsNull(step.PendingQuestion);
        StringAssert.Contains(step.Text, "answer True");
    }

    [TestMethod]
    public async Task ScriptErrorsAreReportedInOrder()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testerror");
        var kinds = step.Output.Select(o => o.Kind).ToList();
        var beforeError = step.Output.ToList().FindIndex(o => o.Text.Contains("before error"));
        Assert.IsGreaterThanOrEqualTo(0, beforeError);
        Assert.AreEqual(beforeError + 1, kinds.IndexOf(HeadlessOutputKind.Error));
        Assert.AreEqual(1, step.Errors.Count());
    }

    [TestMethod]
    public async Task FinishEndsTheSession()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testend");
        StringAssert.Contains(step.Text, "The end.");
        Assert.IsTrue(step.Finished);
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.SendAsync("look"));
    }

    [TestMethod]
    public async Task SameSeedGivesSameResults()
    {
        var a = await StartAsync();
        var b = await StartAsync();
        Assert.AreEqual(await a.EvaluateAsync("GetRandomInt(1, 1000000)"), await b.EvaluateAsync("GetRandomInt(1, 1000000)"));
    }

    [TestMethod]
    public async Task OutputWrittenSeesEverything()
    {
        var seen = new List<HeadlessOutput>();
        var session = await HeadlessSession.StartAsync(
            new GameData(File.ReadAllBytes("headless.aslx"), "headless.aslx"), onOutput: seen.Add);
        await session.SendAsync("testwait");
        CollectionAssert.IsSubsetOf(session.Opening.Output.ToList(), seen);
        Assert.AreEqual("after wait", seen[^1].Text.Trim());
    }

    // examples/test.asl, a Quest 4 game with one of each kind of prompt.
    [TestMethod]
    public async Task Quest4GamePlays()
    {
        var session = await StartAsync("quest4.asl");
        StringAssert.Contains(session.Opening.Text, "You are in room.");
        CollectionAssert.AreEqual(new[] { "east" }, session.Opening.Exits.ToList());

        var step = await session.SendAsync("take red book");
        StringAssert.Contains(step.Text, "You pick it up.");
        Assert.HasCount(1, step.Inventory);
        Assert.IsTrue(await session.AssertAsync("got <book>"));

        step = await session.SendAsync("quit");
        Assert.IsTrue(step.Finished);
    }

    [TestMethod]
    public async Task Quest4PromptsAreHandled()
    {
        var session = await StartAsync("quest4.asl");
        Assert.AreEqual("> test2\nPress a key please...\nDone!\nNow press a key again...\nFinished!\n",
            (await session.SendAsync("test2")).Text);

        var step = await session.SendAsync("ask");
        Assert.AreEqual("test question", step.PendingQuestion);
        step = await session.SendAsync("yes");
        StringAssert.Contains(step.Text, "You answered YES.");
        Assert.AreEqual("another question", step.PendingQuestion);
        await session.SendAsync("no");

        step = await session.SendAsync("menu");
        Assert.IsNotNull(step.PendingMenu);
        step = await session.SendAsync("one");
        StringAssert.Contains(step.Text, "Option 1");

        await session.SendAsync("test");
        StringAssert.Contains((await session.SendAsync("hello")).Text, "You typed: hello");
    }

    [TestMethod]
    public async Task Quest4TimersFireOnTick()
    {
        var session = await StartAsync("quest4.asl");
        await session.SendAsync("tmron");
        var step = await session.TickAsync(12);
        Assert.AreEqual("Tick\nTick\n", step.Text);
    }

    [TestMethod]
    public async Task Quest4GamesCantEvaluate()
    {
        var session = await StartAsync("quest4.asl");
        await Assert.ThrowsAsync<NotSupportedException>(() => session.EvaluateAsync("1"));
    }

    [TestMethod]
    public async Task ReportsWhatThePanesShow()
    {
        var session = await StartAsync();
        CollectionAssert.AreEqual(new[] { "south" }, session.Opening.Exits.ToList());
        CollectionAssert.Contains(session.Opening.Objects.ToList(), "lamp");
        CollectionAssert.DoesNotContain(session.Opening.Objects.ToList(), "south");
        Assert.IsEmpty(session.Opening.Inventory);
        Assert.IsTrue(session.Opening.PanesVisible);
        Assert.IsTrue(session.Opening.CompassVisible);

        var step = await session.SendAsync("take lamp");
        CollectionAssert.AreEqual(new[] { "lamp" }, step.Inventory.ToList());
        CollectionAssert.DoesNotContain(step.Objects.ToList(), "lamp");
    }

    [TestMethod]
    public async Task ReportsWhenTheGameHidesPanesOrCompass()
    {
        var session = await StartAsync();
        var step = await session.SendAsync("testnocompass");
        Assert.IsTrue(step.PanesVisible);
        Assert.IsFalse(step.CompassVisible);

        step = await session.SendAsync("testnopanes");
        Assert.IsFalse(step.PanesVisible);
        Assert.IsFalse(step.CompassVisible);
    }

    [TestMethod]
    public async Task ReportsTheStatusPane()
    {
        var session = await StartAsync();
        CollectionAssert.AreEqual(new[] { "Score: 0" }, session.Opening.Status.ToList());

        var step = await session.SendAsync("testscore");
        CollectionAssert.AreEqual(new[] { "Score: 5" }, step.Status.ToList());
    }
}
