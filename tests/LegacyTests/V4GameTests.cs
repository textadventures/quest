using QuestViva.Common;
using QuestViva.Legacy;

namespace QuestViva.LegacyTests;

[TestClass]
public class V4GameTests
{
    private readonly TestPlayer _player = new();
    private IGame _game;

    [TestInitialize]
    public async Task Init()
    {
        var filename = Path.Combine(["..", "..", "..", "test1.asl"]);
        var gameDataProvider = new FileGameDataProvider(filename);
        var gameData = await gameDataProvider.GetData();
        _game = new V4Game(gameData, null);
        _game.PrintText += _player.PrintText;
        await _game.Initialise(_player);
        await _game.Begin();
    }

    [TestMethod]
    public async Task TestLookAt()
    {
        _player.ClearBuffer();
        await _game.SendCommand("look at object");
        Assert.AreEqual("&gt; look at object", _player.Buffer(0));
        Assert.AreEqual("object look desc", _player.Buffer(1));
    }

    [TestMethod]
    public async Task TestWait()
    {
        _player.ClearBuffer();
        await _game.SendCommand("wait");
        Assert.AreEqual("Start wait", _player.Buffer(1));
        Assert.AreEqual(true, _player.IsWaiting);
        Assert.AreEqual(2, _player.BufferLength, "Expected nothing else in the output buffer after the wait command");
        _player.IsWaiting = false;
        await _game.FinishWait();
        Assert.AreEqual("Done wait", _player.Buffer(2));
    }

    [TestMethod]
    public async Task TestEnter()
    {
        _player.ClearBuffer();
        await _game.SendCommand("enter");
        Assert.AreEqual("Enter text", _player.Buffer(1));
        Assert.AreEqual(2, _player.BufferLength, "Expected nothing else in the output buffer after the enter command");
        await _game.SendCommand("response");
        Assert.AreEqual("You entered: response", _player.Buffer(2));
    }

    [TestMethod]
    public async Task TestMenu()
    {
        _player.ClearBuffer();
        _player.LatestMenu = null;
        await _game.SendCommand("x twin");
        Assert.AreEqual("- <i>Please select which twin you mean:</i>", _player.Buffer(1));
        Assert.AreEqual(2, _player.BufferLength, "Expected nothing else in the output buffer after menu displayed");
        Assert.AreNotEqual(null, _player.LatestMenu);
        Assert.AreEqual(2, _player.LatestMenu.Options.Count);
        Assert.AreEqual("Twin 1", _player.LatestMenu.Options.ElementAt(0).Value);
        Assert.AreEqual("Twin 2", _player.LatestMenu.Options.ElementAt(1).Value);
        await _game.SetMenuResponse(_player.LatestMenu.Options.ElementAt(0).Key);
        Assert.AreEqual("It's twin 1", _player.Buffer(3));
    }

    [TestMethod]
    public async Task TestAsk()
    {
        _player.ClearBuffer();
        _player.QuestionData = null;
        await _game.SendCommand("ask");
        Assert.AreEqual("Some text", _player.Buffer(1));
        Assert.AreEqual(2, _player.BufferLength, "Expected nothing else in the output buffer after question is asked");
        Assert.AreEqual("question text", _player.QuestionData);
        await _game.SetQuestionResponse(true);
        Assert.AreEqual("response yes", _player.Buffer(2));
    }

    [TestMethod]
    public async Task TestStatusVariables()
    {
        Assert.AreEqual("Test variable: 0", _player.StatusText);
        await _game.SendCommand("setstatus");
        Assert.AreEqual("Test variable: 1", _player.StatusText);
    }

    [TestMethod]
    public async Task TestLocation()
    {
        Assert.AreEqual("Room", _player.Location);
        await _game.SendCommand("south");
        Assert.AreEqual("Room2", _player.Location);
    }

    [TestMethod]
    public void TestInitialGameProperties()
    {
        Assert.AreEqual("Unit Test 1", _player.GameName);
        Assert.AreEqual("#000000", _player.Background);
        Assert.AreEqual("#FFFFFF", _player.Foreground);
        Assert.AreEqual("TestFont", _player.FontName);
        Assert.AreEqual("30", _player.FontSize);
    }
}
