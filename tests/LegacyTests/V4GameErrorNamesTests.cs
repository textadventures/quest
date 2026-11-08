using QuestViva.Common;
using QuestViva.Legacy;

namespace QuestViva.LegacyTests;

// The game block's "error <name; message>" lines
[TestClass]
public class V4GameErrorNamesTests
{
    private readonly TestPlayer _player = new();
    private IGame _game = null!;

    [TestInitialize]
    public async Task Init()
    {
        var filename = Path.Combine(["..", "..", "..", "error-names-test.asl"]);
        var gameData = new GameData(File.ReadAllBytes(filename), filename);
        _game = new V4Game(gameData, null);
        _game.PrintText += _player.PrintText;
        await _game.Initialise(_player);
        await _game.Begin();
        _player.ClearBuffer();
    }

    private List<string> Output()
    {
        return Enumerable.Range(0, _player.BufferLength).Select(_player.Buffer).ToList();
    }

    private static void AssertContains(List<string> output, string line)
    {
        CollectionAssert.Contains(output, line, string.Join(" | ", output));
    }

    [TestMethod]
    public async Task TestQdkDefaultTakeSpelling()
    {
        // QDK writes "defaulttake"; Quest only knew "defaultake"
        await _game.SendCommand("take ball");
        AssertContains(Output(), "Got it, thanks.");
    }

    [TestMethod]
    public async Task TestUnknownNamesKeepBadCommand()
    {
        // An unrecognised name used to overwrite the badcommand message
        await _game.SendCommand("frobnicate");
        AssertContains(Output(), "Eh? Never heard of that.");
    }
}
