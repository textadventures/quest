using System.Text.RegularExpressions;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

/// <summary>
/// Drives a v600 WorldModel and captures output one interaction step at a time, so tests can
/// assert that specific text appeared before vs. after a wait / get-input / etc. v600 is used
/// (rather than v580) because this fixture also exercises the GetInput()/Ask()/ShowMenu()
/// expression-function forms, which are gated off for v540-v580 games (see V540UndeprecationTests).
/// For v580+ games, text reaches the player via IPlayer.RunScriptAsync("addText", [html]) rather than
/// the PrintText event, so we intercept the mock player to capture it.
/// </summary>
internal sealed class GameDriver
{
    private readonly WorldModel _worldModel;
    private List<string> _batch = [];
    private Exception _scriptError;
    public List<int> RequestedTimerTicks { get; } = [];
    public GameState State => _worldModel.State;
    public WorldModel Model => _worldModel;
    public Mock<IPlayer> PlayerMock { get; private set; }

    private static readonly Regex StripTags = new(@"<[^>]+>", RegexOptions.Compiled);

    private GameDriver(WorldModel worldModel, Mock<IPlayer> playerMock)
    {
        _worldModel = worldModel;
        PlayerMock = playerMock;
        playerMock
            .Setup(p => p.RunScriptAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Callback<string, object[]>((fn, args) =>
            {
                if (fn == "addText" && args?.Length > 0 && args[0] is string html)
                {
                    var text = StripTags.Replace(html, "").Trim();
                    if (!string.IsNullOrEmpty(text))
                        _batch.Add(text);
                }
            })
            .Returns(Task.CompletedTask);
        worldModel.LogError += ex => _scriptError = ex;
        worldModel.RequestNextTimerTick += seconds => RequestedTimerTicks.Add(seconds);
    }

    public static async Task<GameDriver> LoadAsync(string filename)
    {
        var data = await new FileGameDataProvider(filename).GetData();
        var model = new WorldModel(data, null);
        var playerMock = new Mock<IPlayer>();
        var driver = new GameDriver(model, playerMock);
        var success = await model.Initialise(playerMock.Object);
        if (!success)
            throw new Exception($"Game failed to load: {string.Join("; ", model.Errors)}");
        await model.BeginAsync();
        driver._batch.Clear();
        driver._scriptError = null;
        return driver;
    }

    private IReadOnlyList<string> TakeBatch()
    {
        var result = _batch;
        _batch = [];
        if (_scriptError != null)
        {
            var err = _scriptError;
            _scriptError = null;
            throw new Exception("Script error", err);
        }
        return result;
    }

    public async Task<IReadOnlyList<string>> SendCommandAsync(string command)
    {
        _batch = [];
        _scriptError = null;
        RequestedTimerTicks.Clear();
        await _worldModel.SendCommand(command);
        return TakeBatch();
    }

    public async Task<IReadOnlyList<string>> SendCommandAsync(string command, int elapsedTime)
    {
        _batch = [];
        _scriptError = null;
        RequestedTimerTicks.Clear();
        await _worldModel.SendCommand(command, elapsedTime, new Dictionary<string, string>());
        return TakeBatch();
    }

    public async Task<IReadOnlyList<string>> FinishWaitAsync()
    {
        _batch = [];
        _scriptError = null;
        RequestedTimerTicks.Clear();
        await _worldModel.FinishWait();
        return TakeBatch();
    }

    public async Task<IReadOnlyList<string>> FinishPauseAsync()
    {
        _batch = [];
        _scriptError = null;
        RequestedTimerTicks.Clear();
        await _worldModel.FinishPause();
        return TakeBatch();
    }

    public async Task<IReadOnlyList<string>> SetQuestionResponseAsync(bool response)
    {
        _batch = [];
        _scriptError = null;
        RequestedTimerTicks.Clear();
        await _worldModel.SetQuestionResponse(response);
        return TakeBatch();
    }

    public async Task<IReadOnlyList<string>> SetMenuResponseAsync(string response)
    {
        _batch = [];
        _scriptError = null;
        RequestedTimerTicks.Clear();
        await _worldModel.SetMenuResponse(response);
        return TakeBatch();
    }

    public async Task<IReadOnlyList<string>> TickAsync(int elapsedTime)
    {
        _batch = [];
        _scriptError = null;
        RequestedTimerTicks.Clear();
        await _worldModel.Tick(elapsedTime);
        return TakeBatch();
    }
}

[TestClass]
public class CallbackTests
{
    // GetInput() suspends the script until the user responds, so callback output ("got: …")
    // must not appear until after the response command.
    [TestMethod]
    public async Task GetInputFunction_OutputAppearsInResponseTurn()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        var phase1 = await driver.SendCommandAsync("getinput");
        phase1.ShouldContain("before input");
        phase1.ShouldNotContain("got: John");

        var phase2 = await driver.SendCommandAsync("John");
        phase2.ShouldContain("got: John");
    }

    // get input { callback } is fire-and-forget: the script continues past the block immediately,
    // and the callback runs after the response. So "after input block" appears in phase 1.
    [TestMethod]
    public async Task GetInputScript_ScriptContinuesPastBlock()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        var phase1 = await driver.SendCommandAsync("getinputscript");
        phase1.ShouldContain("before input");
        phase1.ShouldContain("after input block");
        phase1.ShouldNotContain("got: John");

        var phase2 = await driver.SendCommandAsync("John");
        phase2.ShouldContain("got: John");
    }

    // wait { callback } is fire-and-forget: the script continues past the block and on ready is
    // queued. The callback and on ready only run after the player presses a key.
    [TestMethod]
    public async Task Wait_ScriptContinuesPastBlock_CallbackRunsOnKeyPress()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        var phase1 = await driver.SendCommandAsync("testwait");
        phase1.ShouldContain("before wait");
        phase1.ShouldContain("after wait block");
        phase1.ShouldNotContain("wait done");
        phase1.ShouldNotContain("on ready");

        var phase2 = await driver.FinishWaitAsync();
        phase2.ShouldContain("wait done");
        phase2.ShouldContain("on ready");
    }

    // Regression test for #2176: a wait{} callback that chains two MoveObjects back to back
    // (room_a then room_b) must let room_a's changedparent -> OnEnterRoom -> 'on ready' cascade
    // - including its 'enter' script - run to completion (as if it were fully synchronous, like
    // Quest 5) before the second MoveObject fires. Before the fix, both MoveObjects (and both
    // top-level 'on ready' registrations) ran back to back before either room's cascade drained,
    // so room_a's cascade actually executed once game.pov.parent already pointed at room_b, and
    // room_b's cascade could run before room_a's - exactly the interleaving that left the real
    // reported game's Inescapable Cage without map coordinates when it was teleported into via
    // the same wait{ MoveObject; MoveObject } pattern.
    [TestMethod]
    public async Task Wait_ChainedMoveObjectsInSameCallback_EachRoomsCascadeCompletesBeforeTheNextMove()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        await driver.SendCommandAsync("trap");
        await driver.FinishWaitAsync();

        var game = driver.Model.Object("game");
        game.Fields.GetString("trap_a_seen_parent").ShouldBe("room_a");
        game.Fields.GetString("trap_b_seen_parent").ShouldBe("room_b");
        game.Fields.GetAsType<bool>("trap_b_saw_a_entered").ShouldBeTrue();
    }

    // A timer created inside a wait callback (e.g. via SetTimeout) must cause the host
    // to be told when to next call Tick(), otherwise the timer never fires. Regression
    // test for a bug where FinishWait didn't re-request the next timer tick after running
    // its callback, so a timer created there would silently never run on WebPlayer/WasmPlayer
    // (which rely on the RequestNextTimerTick event, unlike a desktop player polling a clock).
    [TestMethod]
    public async Task Wait_CallbackCreatesTimer_RequestsNextTimerTick()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        await driver.SendCommandAsync("testwaittimer");

        var phase2 = await driver.FinishWaitAsync();
        phase2.ShouldContain("wait done");
        driver.RequestedTimerTicks.ShouldContain(5);
    }

    // SendCommand(elapsedTime > 0) must run the elapsed-time-triggered timer script to
    // completion before running the command script, not launch both concurrently. Regression
    // test for a bug where the two ran as a fire-and-forget race, both reading/mutating the
    // same Elements/Fields collections — on a real thread pool (WebPlayer) this could corrupt
    // a collection ("Operations that change non-concurrent collections must have exclusive
    // access"), permanently breaking that game session until the process restarted.
    [TestMethod]
    public async Task SendCommand_ElapsedTimeTriggersTimer_TimerRunsBeforeCommand()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        var output = await driver.SendCommandAsync("testtimerrace", 5);

        output.ShouldContain("timer ran");
        output.ShouldContain("command ran");
        var list = output.ToList();
        list.IndexOf("timer ran").ShouldBeLessThan(list.IndexOf("command ran"));
    }

    // An 'on ready' encountered inside another 'on ready' callback should be queued
    // and run after the outer callback completes — not nested inside it.
    // Without the AddOnReady fix, the old code ran inner callbacks immediately (nested),
    // producing "outer before" → "inner" → "outer after". The fix makes it sequential:
    // "outer before" → "outer after" → "inner". Unbounded nesting without the fix also
    // risks a stack overflow.
    [TestMethod]
    public async Task OnReady_NestedCallback_RunsAfterOuterCompletes()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        var output = await driver.SendCommandAsync("nestedonready");
        output.ShouldContain("outer before");
        output.ShouldContain("outer after");
        output.ShouldContain("inner");
        output.ShouldContain("after block");
        // Key ordering assertion: inner on ready must run after the outer callback
        // finishes ("outer after"), not nested inside it.
        var list = output.ToList();
        list.IndexOf("outer after").ShouldBeLessThan(list.IndexOf("inner"));
    }

    // Regression test for a bug (reported against Giantkiller Too's Indoor Market partition
    // puzzle) where an on-ready queued mid-loop never ran: the loop's "get input" reopens the
    // next prompt from inside the current callback, before that callback's own finally block
    // decrements _pendingCallbackCount, so the count never returns to 0 while the loop is
    // running. The old drain condition (_pendingCallbackCount == 0) meant anything queued during
    // any round was stuck forever. Each round's on-ready must run in that same round.
    [TestMethod]
    public async Task OnReady_QueuedInsideRecursiveGetInputLoop_RunsEachRound()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        var phase1 = await driver.SendCommandAsync("puzzleloop");
        phase1.ShouldContain("prompt");

        var phase2 = await driver.SendCommandAsync("first");
        phase2.ShouldContain("deferred: first");
        phase2.ShouldContain("prompt");

        var phase3 = await driver.SendCommandAsync("second");
        phase3.ShouldContain("deferred: second");
        phase3.ShouldContain("prompt");

        var phase4 = await driver.SendCommandAsync("stop");
        phase4.ShouldContain("deferred: stop");
        phase4.ShouldContain("stopped");
    }

    // Tick() used to call SendNextTimerRequest() right after *starting* (not awaiting)
    // TickAsyncInternal, unlike every other entry point (FinishWait, FinishPause,
    // SetQuestionResponse, SetMenuResponse, HandleCommandAsyncInternal, SendEventCore,
    // BeginInternalAsync), which all correctly request the next tick only after their
    // work fully completes. On a real player, a timer's own script can genuinely suspend
    // mid-execution (msg()/JS.* calls go through interop that can yield) - so if
    // chaintimer1 disables itself, then yields, then enables chaintimer2, the premature
    // request captures a stale snapshot with chaintimer1 already disabled and
    // chaintimer2 not yet enabled: GetTimeUntilNextTimerRuns() sees no enabled timers and
    // requests 0, and nothing ever re-requests afterwards - chaintimer2 silently never
    // fires again. This is exactly the WasmPlayer-only "game just stops" bug reported for
    // Timer/SetTimeout chains: WasmPlayer's RunScriptAsync genuinely yields via a
    // throttled JS interop call; WebPlayer's happens not to hit the same race in practice.
    [TestMethod]
    public async Task Tick_TimerScriptYieldsBeforeEnablingNextTimer_RequestsCorrectNextTick()
    {
        var driver = await GameDriver.LoadAsync("callbacktest.aslx");

        // chaintimer1 starts disabled and is enabled here (rather than from game start)
        // so it can't collide with other tests' timers (e.g. racetimer's interval=5).
        await driver.SendCommandAsync("enablechaintimer");
        driver.RequestedTimerTicks.Clear();

        // Simulate a real player's interop call (msg() -> JS.addText) genuinely suspending
        // mid-script - a plain TaskCompletionSource rather than Task.Yield(), since
        // Task.Yield()'s continuation runs on the CLR thread pool and can race the calling
        // thread; this instead matches WasmPlayer's single-threaded JS event loop exactly,
        // where nothing else can possibly run until this is explicitly completed below.
        var addTextTcs = new TaskCompletionSource();
        driver.PlayerMock
            .Setup(p => p.RunScriptAsync("addText", It.IsAny<object[]>()))
            .Returns(addTextTcs.Task);

        var tickTask = driver.Model.Tick(2);

        // chaintimer1 has run DisableTimer and is now suspended mid-msg(), before reaching
        // EnableTimer(chaintimer2). The buggy Tick() calls SendNextTimerRequest() right here
        // - before the timer's script has actually finished - so nothing should have been
        // requested yet.
        driver.RequestedTimerTicks.ShouldBeEmpty();

        addTextTcs.TrySetResult();
        await tickTask;

        driver.RequestedTimerTicks.ShouldNotContain(0);
        driver.RequestedTimerTicks.ShouldContain(3);
    }
}
