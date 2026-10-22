using QuestViva.Common;
using QuestViva.Legacy;

namespace QuestViva.LegacyTests;

// A room name with an array index, e.g. cell[n] with n = 2, refers to the room cell2
[TestClass]
public class V4GameRoomArrayTests
{
    private readonly TestPlayer _player = new();
    private IGame _game = null!;

    [TestInitialize]
    public async Task Init()
    {
        var filename = Path.Combine(["..", "..", "..", "room-array-test.asl"]);
        var gameData = new GameData(File.ReadAllBytes(filename), filename);
        _game = new V4Game(gameData, null);
        _game.PrintText += _player.PrintText;
        await _game.Initialise(_player);
        await _game.Begin();
        _player.ClearBuffer();
    }

    private string Output()
    {
        return string.Join(" | ", Enumerable.Range(0, _player.BufferLength).Select(_player.Buffer));
    }

    [TestMethod]
    public async Task TestGotoArrayIndexedRoom()
    {
        await _game.SendCommand("gocell");
        Assert.AreEqual("Cell2", _player.Location, Output());
    }

    [TestMethod]
    public async Task TestMoveToArrayIndexedRoom()
    {
        await _game.SendCommand("movecoin");
        Assert.AreEqual("Cell2", _player.Location, Output());
        StringAssert.Contains(Output(), "<b>coin</b>");
    }
}
