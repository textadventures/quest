using QuestViva.Common;
using QuestViva.Legacy;

namespace QuestViva.LegacyTests;

[TestClass]
public class V4GameTests
{
    private readonly TestPlayer m_player = new();
    private IGame m_game;

    [TestInitialize]
    public async Task Init()
    {
        var filename = Path.Combine(["..", "..", "..", "test1.asl"]);
        var gameDataProvider = new FileGameDataProvider(filename);
        var gameData = await gameDataProvider.GetData();
        m_game = new V4Game(gameData, null);
        m_game.PrintText += m_player.PrintText;
        await m_game.Initialise(m_player);
        m_game.Begin();
    }

    [TestMethod]
    public async Task TestLookAt()
    {
        m_player.ClearBuffer();
        await m_game.SendCommand("look at object");
        Assert.AreEqual("&gt; look at object", m_player.Buffer(0));
        Assert.AreEqual("object look desc", m_player.Buffer(1));
    }

    [TestMethod]
    public async Task TestWait()
    {
        m_player.ClearBuffer();
        await m_game.SendCommand("wait");
        Assert.AreEqual("Start wait", m_player.Buffer(1));
        Assert.AreEqual(true, m_player.IsWaiting);
        Assert.AreEqual(2, m_player.BufferLength, "Expected nothing else in the output buffer after the wait command");
        m_player.IsWaiting = false;
        await m_game.FinishWait();
        Assert.AreEqual("Done wait", m_player.Buffer(2));
    }

    [TestMethod]
    public async Task TestEnter()
    {
        m_player.ClearBuffer();
        await m_game.SendCommand("enter");
        Assert.AreEqual("Enter text", m_player.Buffer(1));
        Assert.AreEqual(2, m_player.BufferLength, "Expected nothing else in the output buffer after the enter command");
        await m_game.SendCommand("response");
        Assert.AreEqual("You entered: response", m_player.Buffer(2));
    }

    [TestMethod]
    public async Task TestMenu()
    {
        m_player.ClearBuffer();
        m_player.LatestMenu = null;
        await m_game.SendCommand("x twin");
        Assert.AreEqual("- <i>Please select which twin you mean:</i>", m_player.Buffer(1));
        Assert.AreEqual(2, m_player.BufferLength, "Expected nothing else in the output buffer after menu displayed");
        Assert.AreNotEqual(null, m_player.LatestMenu);
        Assert.AreEqual(2, m_player.LatestMenu.Options.Count);
        Assert.AreEqual("Twin 1", m_player.LatestMenu.Options.ElementAt(0).Value);
        Assert.AreEqual("Twin 2", m_player.LatestMenu.Options.ElementAt(1).Value);
        await m_game.SetMenuResponse(m_player.LatestMenu.Options.ElementAt(0).Key);
        Assert.AreEqual("It's twin 1", m_player.Buffer(3));
    }

    [TestMethod]
    public async Task TestAsk()
    {
        m_player.ClearBuffer();
        m_player.QuestionData = null;
        await m_game.SendCommand("ask");
        Assert.AreEqual("Some text", m_player.Buffer(1));
        Assert.AreEqual(2, m_player.BufferLength, "Expected nothing else in the output buffer after question is asked");
        Assert.AreEqual("question text", m_player.QuestionData);
        await m_game.SetQuestionResponse(true);
        Assert.AreEqual("response yes", m_player.Buffer(2));
    }

    [TestMethod]
    public async Task TestStatusVariables()
    {
        Assert.AreEqual("Test variable: 0", m_player.StatusText);
        await m_game.SendCommand("setstatus");
        Assert.AreEqual("Test variable: 1", m_player.StatusText);
    }

    [TestMethod]
    public async Task TestLocation()
    {
        Assert.AreEqual("Room", m_player.Location);
        await m_game.SendCommand("south");
        Assert.AreEqual("Room2", m_player.Location);
    }

    [TestMethod]
    public void TestInitialGameProperties()
    {
        Assert.AreEqual("Unit Test 1", m_player.GameName);
        Assert.AreEqual("#000000", m_player.Background);
        Assert.AreEqual("#FFFFFF", m_player.Foreground);
        Assert.AreEqual("TestFont", m_player.FontName);
        Assert.AreEqual("30", m_player.FontSize);
    }
}
