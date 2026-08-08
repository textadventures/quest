using QuestViva.Engine;
using Shouldly;

namespace QuestViva.EngineTests;

// The real Quest 5 engine rejected the ExpressionOwner.GetInput()/Ask()/ShowMenu() expression-function
// forms and the request(Wait)/request(Pause) script forms for games declaring WorldModel version 5.4+/
// 5.5+, in favour of their callback-based replacements ('get input'/'ask'/'show menu'/'wait' script
// commands, 'SetTimeout'). The 2026-06-26 asyncification of the engine (converting these from
// Monitor-based thread blocking to TaskCompletionSource-based async blocking) accidentally dropped the
// version guard from the three ExpressionOwner functions ("un-deprecated" per that commit's message),
// while the RequestScript guards for Wait/Pause survived untouched - an inconsistency, not a deliberate
// decision. These tests restore v5 parity for v540-v580 games, and cover WorldModelVersion.v600 as the
// version where all five are deliberately re-enabled: now that they're backed by TCS-based async
// blocking rather than a real blocked thread, they're genuinely more usable than the callback
// replacements they were originally deprecated in favour of.
[TestClass]
public class V600UndeprecationTests
{
    private static readonly WorldModelVersion[] GatedVersions =
        [WorldModelVersion.v540, WorldModelVersion.v550, WorldModelVersion.v580];

    [TestMethod]
    public async Task GetInputFunction_ThrowsForV540ThroughV580_WorksBelowV540AndAtV600()
    {
        foreach (var version in GatedVersions)
        {
            var driver = await GameDriver.LoadAsync("versiongatetest.aslx");
            driver.Model.Version = version;

            var ex = await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("getinputfn"));
            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.Message.ShouldContain(
                "'GetInput' function is not supported for games written for Quest 5.4 or later");
        }

        // v530 (pre-v580) output routes through the PrintText event rather than
        // RunScriptAsync("addText", ...), so V5BlockingGameDriver is needed here instead of GameDriver.
        var belowThresholdDriver = await V5BlockingGameDriver.LoadAsync("versiongatetest.aslx");
        belowThresholdDriver.Model.Version = WorldModelVersion.v530;
        var belowPhase1 = await belowThresholdDriver.SendCommandAsync("getinputfn");
        belowPhase1.ShouldNotContain("got: John");
        var belowPhase2 = await belowThresholdDriver.SendCommandAsync("John");
        belowPhase2.ShouldContain("got: John");

        var v600Driver = await GameDriver.LoadAsync("versiongatetest.aslx"); // defaults to v600
        var phase1 = await v600Driver.SendCommandAsync("getinputfn");
        phase1.ShouldNotContain("got: John");
        var phase2 = await v600Driver.SendCommandAsync("John");
        phase2.ShouldContain("got: John");
    }

    [TestMethod]
    public async Task AskFunction_ThrowsForV540ThroughV580_WorksAtV600()
    {
        foreach (var version in GatedVersions)
        {
            var driver = await GameDriver.LoadAsync("versiongatetest.aslx");
            driver.Model.Version = version;

            var ex = await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("askfn"));
            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.Message.ShouldContain(
                "'Ask' function is not supported for games written for Quest 5.4 or later");
        }

        var v600Driver = await GameDriver.LoadAsync("versiongatetest.aslx");
        var phase1 = await v600Driver.SendCommandAsync("askfn");
        phase1.ShouldNotContain("ask: yes");
        var phase2 = await v600Driver.SetQuestionResponseAsync(true);
        phase2.ShouldContain("ask: yes");
    }

    [TestMethod]
    public async Task ShowMenuFunction_ThrowsForV540ThroughV580_WorksAtV600()
    {
        foreach (var version in GatedVersions)
        {
            var driver = await GameDriver.LoadAsync("versiongatetest.aslx");
            driver.Model.Version = version;

            var ex = await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("menufn"));
            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.Message.ShouldContain(
                "'ShowMenu' function is not supported for games written for Quest 5.4 or later");
        }

        var v600Driver = await GameDriver.LoadAsync("versiongatetest.aslx");
        var phase1 = await v600Driver.SendCommandAsync("menufn");
        phase1.ShouldNotContain("menu: optiona");
        var phase2 = await v600Driver.SetMenuResponseAsync("optiona");
        phase2.ShouldContain("menu: optiona");
    }

    [TestMethod]
    public async Task WaitRequest_ThrowsForV540ThroughV580_WorksAtV600()
    {
        foreach (var version in GatedVersions)
        {
            var driver = await GameDriver.LoadAsync("versiongatetest.aslx");
            driver.Model.Version = version;

            var ex = await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("waitrequest"));
            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.Message.ShouldContain(
                "'Wait' request is not supported for games written for Quest 5.4 or later");
        }

        var v600Driver = await GameDriver.LoadAsync("versiongatetest.aslx");
        var phase1 = await v600Driver.SendCommandAsync("waitrequest");
        phase1.ShouldNotContain("wait done");
        var phase2 = await v600Driver.FinishWaitAsync();
        phase2.ShouldContain("wait done");
    }

    // Pause's threshold is v550 (Wait/GetInput/Ask/ShowMenu are v540), so v540 itself must still work.
    [TestMethod]
    public async Task PauseRequest_ThrowsForV550AndV580_WorksAtV540AndV600()
    {
        foreach (var version in new[] {WorldModelVersion.v550, WorldModelVersion.v580})
        {
            var driver = await GameDriver.LoadAsync("versiongatetest.aslx");
            driver.Model.Version = version;

            var ex = await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("pauserequest"));
            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.Message.ShouldContain(
                "'Pause' request is not supported for games written for Quest 5.5 or later");
        }

        var v540Driver = await GameDriver.LoadAsync("versiongatetest.aslx");
        v540Driver.Model.Version = WorldModelVersion.v540;
        var v540Phase1 = await v540Driver.SendCommandAsync("pauserequest");
        v540Phase1.ShouldNotContain("pause done");
        var v540Phase2 = await v540Driver.FinishPauseAsync();
        v540Phase2.ShouldContain("pause done");

        var v600Driver = await GameDriver.LoadAsync("versiongatetest.aslx");
        var phase1 = await v600Driver.SendCommandAsync("pauserequest");
        phase1.ShouldNotContain("pause done");
        var phase2 = await v600Driver.FinishPauseAsync();
        phase2.ShouldContain("pause done");
    }

    // Regression test: the Pause() Core.aslx function used to unconditionally call
    // error("The Pause function is obsolete as of Quest 5.5") regardless of WorldModel
    // version. It's been restored to just delegate to request (Pause, ...), so it must be
    // gated identically to the raw request form - not hardcoded broken for every version.
    [TestMethod]
    public async Task PauseFunction_ThrowsForV550AndV580_WorksAtV540AndV600()
    {
        foreach (var version in new[] {WorldModelVersion.v550, WorldModelVersion.v580})
        {
            var driver = await GameDriver.LoadAsync("versiongatetest.aslx");
            driver.Model.Version = version;

            var ex = await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("pausefunction"));
            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.Message.ShouldContain(
                "'Pause' request is not supported for games written for Quest 5.5 or later");
        }

        var v540Driver = await GameDriver.LoadAsync("versiongatetest.aslx");
        v540Driver.Model.Version = WorldModelVersion.v540;
        var v540Phase1 = await v540Driver.SendCommandAsync("pausefunction");
        v540Phase1.ShouldNotContain("pause function done");
        var v540Phase2 = await v540Driver.FinishPauseAsync();
        v540Phase2.ShouldContain("pause function done");

        var v600Driver = await GameDriver.LoadAsync("versiongatetest.aslx");
        var phase1 = await v600Driver.SendCommandAsync("pausefunction");
        phase1.ShouldNotContain("pause function done");
        var phase2 = await v600Driver.FinishPauseAsync();
        phase2.ShouldContain("pause function done");
    }

    // Regression test: WaitForKeyPress() was a Core.aslx function until it was deleted outright
    // in 2018 (unlike Pause(), which survived but was hardcoded broken). Restored to delegate to
    // request (Wait, ""), so it must be gated identically to the raw request form.
    [TestMethod]
    public async Task WaitForKeyPressFunction_ThrowsForV540ThroughV580_WorksAtV600()
    {
        foreach (var version in GatedVersions)
        {
            var driver = await GameDriver.LoadAsync("versiongatetest.aslx");
            driver.Model.Version = version;

            var ex = await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("waitforkeypressfunction"));
            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.Message.ShouldContain(
                "'Wait' request is not supported for games written for Quest 5.4 or later");
        }

        var v600Driver = await GameDriver.LoadAsync("versiongatetest.aslx");
        var phase1 = await v600Driver.SendCommandAsync("waitforkeypressfunction");
        phase1.ShouldNotContain("wait for key press function done");
        var phase2 = await v600Driver.FinishWaitAsync();
        phase2.ShouldContain("wait for key press function done");
    }
}
