using System.Text.RegularExpressions;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

/// <summary>
/// Drives a v580+ WorldModel and captures output one interaction step at a time, so tests can
/// assert that specific text appeared before vs. after a wait / get-input / etc.
/// For v580+ games, text reaches the player via IPlayer.RunScriptAsync("addText", [html]) rather than
/// the PrintText event, so we intercept the mock player to capture it.
/// </summary>
internal sealed class GameDriver
{
    private readonly WorldModel _worldModel;
    private List<string> _batch = [];
    private Exception _scriptError;
    public List<int> RequestedTimerTicks { get; } = [];

    private static readonly Regex StripTags = new(@"<[^>]+>", RegexOptions.Compiled);

    private GameDriver(WorldModel worldModel, Mock<IPlayer> playerMock)
    {
        _worldModel = worldModel;
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

    public async Task<IReadOnlyList<string>> FinishWaitAsync()
    {
        _batch = [];
        _scriptError = null;
        RequestedTimerTicks.Clear();
        await _worldModel.FinishWait();
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
}
