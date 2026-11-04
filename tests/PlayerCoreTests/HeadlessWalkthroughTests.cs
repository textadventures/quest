using QuestViva.Common;
using QuestViva.PlayerCore;

namespace QuestViva.PlayerCoreTests;

[TestClass]
public class HeadlessWalkthroughTests
{
    private static async Task<WalkthroughResult> RunAsync(string name, List<WalkthroughStep>? steps = null)
    {
        var session = await HeadlessSession.StartAsync(
            new GameData(File.ReadAllBytes("headless.aslx"), "headless.aslx"));
        return await HeadlessWalkthrough.RunAsync(session, name, steps == null ? null : steps.Add);
    }

    [TestMethod]
    public async Task ListsTheGamesWalkthroughs()
    {
        var session = await HeadlessSession.StartAsync(
            new GameData(File.ReadAllBytes("headless.aslx"), "headless.aslx"));
        CollectionAssert.Contains(HeadlessWalkthrough.Names(session).ToList(), "passes");
    }

    [TestMethod]
    public async Task CommandsMenusQuestionsAndAssertsPass()
    {
        var steps = new List<WalkthroughStep>();
        var result = await RunAsync("passes", steps);

        Assert.IsTrue(result.Passed, result.Failure);
        Assert.AreEqual(7, result.StepCount);
        Assert.AreEqual(7, result.StepsRun);
        Assert.IsTrue(steps.Single(s => s.Step.StartsWith("assert:")).AssertPassed);
        StringAssert.Contains(steps.Single(s => s.Step == "menu:g").Response!.Text, "chose g");
        StringAssert.Contains(steps.Single(s => s.Step == "answer:yes").Response!.Text, "answer True");
    }

    [TestMethod]
    public async Task FailedAssertFailsAtThatStep()
    {
        var result = await RunAsync("failsassert");
        Assert.IsFalse(result.Passed);
        Assert.AreEqual(1, result.FailedStep);
        StringAssert.Contains(result.Failure, "lamp.parent = player");
    }

    [TestMethod]
    public async Task AssertThatCantBeEvaluatedFails()
    {
        var result = await RunAsync("badassert");
        Assert.AreEqual(1, result.FailedStep);
        StringAssert.Contains(result.Failure, "couldn't be evaluated");
    }

    [TestMethod]
    public async Task ScriptErrorFails()
    {
        var result = await RunAsync("scripterror");
        Assert.AreEqual(1, result.FailedStep);
        StringAssert.StartsWith(result.Failure, "Script error:");
    }

    [TestMethod]
    public async Task MenuAnswerThatIsntAnOptionFails()
    {
        var result = await RunAsync("wrongmenukey");
        Assert.AreEqual(2, result.FailedStep);
        StringAssert.Contains(result.Failure, "isn't one of the menu's options");
    }

    [TestMethod]
    public async Task CommandWhileAMenuIsPendingFails()
    {
        var result = await RunAsync("unansweredmenu");
        Assert.AreEqual(2, result.FailedStep);
        StringAssert.Contains(result.Failure, "menu:<key>");
    }

    [TestMethod]
    public async Task GameEndingBeforeTheLastStepFails()
    {
        var result = await RunAsync("endsearly");
        Assert.IsFalse(result.Passed);
        Assert.IsNull(result.FailedStep);
        Assert.AreEqual(1, result.StepsRun);
        StringAssert.Contains(result.Failure, "ended after step 1");
    }

    [TestMethod]
    public async Task DelayAdvancesTheClockAfterEachStep()
    {
        var steps = new List<WalkthroughStep>();
        var result = await RunAsync("delayticks", steps);

        Assert.IsTrue(result.Passed, result.Failure);
        // The clock timer's interval is 10 seconds: 5 after the delay step itself, 5 more after look.
        Assert.DoesNotContain("tock", steps[1].Response!.Text);
        StringAssert.Contains(steps[2].Response!.Text, "tock");
    }

    [TestMethod]
    public async Task UnknownWalkthroughThrows()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => RunAsync("nosuchwalkthrough"));
    }
}
