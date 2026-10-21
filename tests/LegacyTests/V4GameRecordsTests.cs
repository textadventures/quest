using QuestViva.Common;
using QuestViva.Legacy;

namespace QuestViva.LegacyTests;

// VB6 Quest's object and room records were value types; these check the behaviour that relied on it
[TestClass]
public class V4GameRecordsTests
{
    private readonly TestPlayer _player = new();
    private IGame _game = null!;

    [TestInitialize]
    public async Task Init()
    {
        var filename = Path.Combine(["..", "..", "..", "records-test.asl"]);
        var gameData = await new FileGameDataProvider(filename).GetData();
        _game = new V4Game(gameData, null);
        _game.PrintText += _player.PrintText;
        await _game.Initialise(_player);
        await _game.Begin();
        _player.ClearBuffer();
    }

    private List<string> Output()
    {
        var output = Enumerable.Range(0, _player.BufferLength).Select(_player.Buffer).ToList();
        CollectionAssert.DoesNotContain(output, "[An internal error occurred]", string.Join(" | ", output));
        return output;
    }

    private static void AssertContains(List<string> output, string line)
    {
        CollectionAssert.Contains(output, line, string.Join(" | ", output));
    }

    [TestMethod]
    public async Task TestRemoveWithoutParent()
    {
        // Reads object id 0 as the parent
        await _game.SendCommand("removeloose");
        AssertContains(Output(), "Removed.");
    }

    [TestMethod]
    public async Task TestPropertyOfMissingObject()
    {
        // Reads object id 0, as "nosuch" doesn't exist
        await _game.SendCommand("missingprop");
        Assert.IsTrue(Output().Any(line => line.StartsWith("Before")), string.Join(" | ", Output()));
    }

    [TestMethod]
    public async Task TestCloneCopiesObject()
    {
        // Cloning used to alias the original, so the first clone renamed it and the second found nothing
        await _game.SendCommand("clonetwice");
        AssertContains(Output(), "Colours: red green red");
    }

    [TestMethod]
    public async Task TestTakeFromContainer()
    {
        await _game.SendCommand("take ball");
        var output = Output();
        AssertContains(output, "(first removing it from box)");
        AssertContains(output, "You pick it up.");
    }
}
