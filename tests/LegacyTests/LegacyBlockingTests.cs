using QuestViva.Common;
using QuestViva.Legacy;

namespace QuestViva.LegacyTests;

/// <summary>
/// Tests blocking input/wait/ask behaviour for v4.10 games.
/// In v4, enter/wait/ask are synchronous blocking operations: the script genuinely
/// suspends at each point and only continues once the player responds.
/// </summary>
[TestClass]
public class LegacyBlockingTests
{
    private readonly TestPlayer _player = new();
    private IGame _game = null!;

    [TestInitialize]
    public async Task Init()
    {
        var filename = Path.Combine("..", "..", "..", "v410test.asl");
        var gameData = new GameData(File.ReadAllBytes(filename), filename);
        _game = new V4Game(gameData, null);
        _game.PrintText += _player.PrintText;
        await _game.Initialise(_player);
        await _game.Begin();
    }

    // wait blocks the script until FinishWait() is called; "Done" must not
    // appear until after the key press.
    [TestMethod]
    public async Task Wait_OutputAppearsAfterKeyPress()
    {
        _player.ClearBuffer();
        await _game.SendCommand("wait");

        Assert.AreEqual("Press a key...", _player.Buffer(1));
        Assert.IsTrue(_player.IsWaiting);
        Assert.AreEqual(2, _player.BufferLength, "No output expected after wait until key is pressed");

        _player.IsWaiting = false;
        await _game.FinishWait();

        Assert.AreEqual("Done", _player.Buffer(2));
    }

    // enter blocks the script until the player types a response; "Hello …" must
    // not appear until after the response command.
    [TestMethod]
    public async Task Enter_OutputAppearsInResponseTurn()
    {
        _player.ClearBuffer();
        await _game.SendCommand("input");

        Assert.AreEqual("Enter your name...", _player.Buffer(1));
        Assert.AreEqual(2, _player.BufferLength, "No output expected after enter until player responds");

        await _game.SendCommand("John");

        Assert.AreEqual("Hello John", _player.Buffer(2));
    }

    // ask blocks the script: both the conditional result and the statement after
    // the if/else must appear in the response turn, not the ask turn.
    [TestMethod]
    public async Task Ask_ScriptBlocksUntilResponse()
    {
        _player.ClearBuffer();
        await _game.SendCommand("ask");

        Assert.AreEqual(1, _player.BufferLength, "No output expected after ask until player responds");
        Assert.AreEqual("Answer me this", _player.QuestionData);

        await _game.SetQuestionResponse(true);

        Assert.AreEqual("You said yes", _player.Buffer(1));
        Assert.AreEqual("This is run after asking the question", _player.Buffer(2));
    }

    // A save taken while a script is suspended would lose the rest of it, so the player's Save
    // button relies on SetTurnPending(true) at every mid-turn prompt and false once the turn ends.
    [TestMethod]
    public void TurnPending_FalseAfterBegin()
    {
        Assert.IsFalse(_player.TurnPending);
    }

    [TestMethod]
    public async Task TurnPending_Wait()
    {
        await _game.SendCommand("wait");
        Assert.IsTrue(_player.TurnPending);

        await _game.FinishWait();
        Assert.IsFalse(_player.TurnPending);
    }

    [TestMethod]
    public async Task TurnPending_Pause()
    {
        await _game.SendCommand("pause");
        Assert.IsTrue(_player.TurnPending);

        await _game.FinishPause();
        Assert.IsFalse(_player.TurnPending);
    }

    [TestMethod]
    public async Task TurnPending_Ask()
    {
        await _game.SendCommand("ask");
        Assert.IsTrue(_player.TurnPending);

        await _game.SetQuestionResponse(false);
        Assert.IsFalse(_player.TurnPending);
    }

    [TestMethod]
    public async Task TurnPending_Menu()
    {
        await _game.SendCommand("menu");
        Assert.IsTrue(_player.TurnPending);

        await _game.SetMenuResponse(_player.LatestMenu!.Options.First().Key);
        Assert.IsFalse(_player.TurnPending);
    }

    // The answer to the first enter resumes the script straight into the second one, so the
    // turn is still pending after it even though the answering command itself has finished.
    [TestMethod]
    public async Task TurnPending_ConsecutiveEnters()
    {
        await _game.SendCommand("input2");
        Assert.IsTrue(_player.TurnPending);

        await _game.SendCommand("red");
        Assert.IsTrue(_player.TurnPending, "Still pending at the second enter");

        _player.ClearBuffer();
        await _game.SendCommand("blue");
        Assert.IsFalse(_player.TurnPending);
        CollectionAssert.Contains(Enumerable.Range(0, _player.BufferLength).Select(_player.Buffer).ToList(), "red and blue");
    }
}
