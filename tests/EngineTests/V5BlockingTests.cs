using System.Text.RegularExpressions;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

/// <summary>
/// Drives a v500 WorldModel and captures output one interaction at a time.
/// v500 uses the PrintText event directly (unlike v580+ which routes through JS.addText),
/// so we subscribe to PrintText and strip the output wrapper.
/// </summary>
internal sealed class V5BlockingGameDriver
{
    private readonly WorldModel _worldModel;
    private List<string> _batch = [];

    private static readonly Regex OutputTag =
        new(@"<output[^>]*>(.*?)</output>", RegexOptions.Singleline | RegexOptions.Compiled);

    private V5BlockingGameDriver(WorldModel worldModel)
    {
        _worldModel = worldModel;
        worldModel.PrintText += text =>
        {
            var m = OutputTag.Match(text);
            _batch.Add(m.Success ? m.Groups[1].Value : text);
        };
        worldModel.LogError += ex => throw ex;
    }

    public static async Task<V5BlockingGameDriver> LoadAsync(string filename)
    {
        var data = await new FileGameDataProvider(filename).GetData();
        var model = new WorldModel(data, null);
        var driver = new V5BlockingGameDriver(model);
        var playerMock = new Mock<IPlayer>();
        var success = await model.Initialise(playerMock.Object);
        if (!success)
            throw new Exception($"Game failed to load: {string.Join("; ", model.Errors)}");
        await model.BeginAsync();
        driver._batch.Clear();
        return driver;
    }

    private IReadOnlyList<string> TakeBatch()
    {
        var result = _batch;
        _batch = [];
        return result;
    }

    public async Task<IReadOnlyList<string>> SendCommandAsync(string command)
    {
        _batch = [];
        await _worldModel.SendCommand(command);
        return TakeBatch();
    }

    public async Task<IReadOnlyList<string>> FinishWaitAsync()
    {
        _batch = [];
        await _worldModel.FinishWait();
        return TakeBatch();
    }

    public async Task<IReadOnlyList<string>> SetQuestionResponseAsync(bool response)
    {
        _batch = [];
        await _worldModel.SetQuestionResponse(response);
        return TakeBatch();
    }
}

/// <summary>
/// Tests blocking input/wait/ask behaviour for v500 games.
/// In v500, GetInput/request(Wait)/Ask are synchronous blocking operations: the script
/// genuinely suspends and only continues after the player responds. This is distinct
/// from v580's fire-and-forget callback pattern where the script continues past the block
/// immediately.
/// </summary>
[TestClass]
public class V5BlockingTests
{
    // GetInput() suspends the script until the player responds; "Hello …" must
    // not appear until after the response command.
    [TestMethod]
    public async Task GetInput_OutputAppearsInResponseTurn()
    {
        var driver = await V5BlockingGameDriver.LoadAsync("v500test.aslx");

        var phase1 = await driver.SendCommandAsync("input");
        phase1.ShouldContain("Enter your name...");
        phase1.ShouldNotContain("Hello John");

        var phase2 = await driver.SendCommandAsync("John");
        phase2.ShouldContain("Hello John");
    }

    // request(Wait) suspends the script until FinishWait() is called; "Done!" must
    // not appear until after the key press.
    [TestMethod]
    public async Task Wait_OutputAppearsAfterKeyPress()
    {
        var driver = await V5BlockingGameDriver.LoadAsync("v500test.aslx");

        var phase1 = await driver.SendCommandAsync("testwait");
        phase1.ShouldContain("Press a key...");
        phase1.ShouldNotContain("Done!");

        var phase2 = await driver.FinishWaitAsync();
        phase2.ShouldContain("Done!");
    }

    // Ask() suspends the script: both the conditional result and the statement after
    // the if/else must appear in the response turn, not the ask turn.
    [TestMethod]
    public async Task Ask_ScriptBlocksUntilResponse()
    {
        var driver = await V5BlockingGameDriver.LoadAsync("v500test.aslx");

        var phase1 = await driver.SendCommandAsync("ask");
        phase1.ShouldContain("Asking a question...");
        phase1.ShouldNotContain("You said yes");
        phase1.ShouldNotContain("This is run after asking the question");

        var phase2 = await driver.SetQuestionResponseAsync(true);
        phase2.ShouldContain("You said yes");
        phase2.ShouldContain("This is run after asking the question");
    }
}
