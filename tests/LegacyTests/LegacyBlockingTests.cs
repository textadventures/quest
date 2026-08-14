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
    private IGame _game;

    [TestInitialize]
    public async Task Init()
    {
        var filename = Path.Combine("..", "..", "..", "v410test.asl");
        var gameDataProvider = new FileGameDataProvider(filename);
        var gameData = await gameDataProvider.GetData();
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
}
