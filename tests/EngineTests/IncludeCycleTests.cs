using System.Text;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;
using Shouldly;

namespace QuestViva.EngineTests;

// An <include> that leads back to a file already being loaded used to recurse until the stack
// overflowed (#2866). The adjacent files here stand in for a case-insensitive disk, where
// "Deutsch.aslx" finds deutsch.aslx.
[TestClass]
public class IncludeCycleTests
{
    private static string Game(string includes) => $"""
        <asl version="580">
          {includes}
          <include ref="Core.aslx" />
          <game name="test"/>
          <object name="room">
            <object name="player">
              <inherit name="defaultplayer" />
            </object>
          </object>
        </asl>
        """;

    private static string Library(string includes) => $"""
        <library>
          {includes}
        </library>
        """;

    private static async Task<(WorldModel WorldModel, bool Ok)> LoadAsync(
        string filename, string game, Dictionary<string, string> adjacentFiles)
    {
        var gameBytes = Encoding.UTF8.GetBytes(game);
        var adjacent = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, contents) in adjacentFiles)
        {
            adjacent[name] = Encoding.UTF8.GetBytes(contents);
        }

        // Like qv's folder lookup, the game file itself is among the adjacent files.
        adjacent[filename] = gameBytes;

        var worldModel = Helpers.CreateWorldModel(new GameData(gameBytes, filename, adjacent));
        var ok = await worldModel.Initialise(new Mock<IPlayer>().Object);
        return (worldModel, ok);
    }

    [TestMethod]
    public async Task AGameNamedAfterABuiltInLibraryGetsTheBuiltInLibrary()
    {
        var (worldModel, ok) = await LoadAsync(
            "english.aslx", Game("""<include ref="English.aslx" />"""), []);

        ok.ShouldBeTrue(string.Join("; ", worldModel.Errors));
    }

    [TestMethod]
    public async Task AGameIncludingItselfIsALoadError()
    {
        // Spelled differently from the game's own name, as a different file the lookup could
        // still be (on a case-sensitive disk it would be), so only the cycle check catches it.
        var (worldModel, ok) = await LoadAsync(
            "game.aslx",
            Game("""<include ref="lib.aslx" />"""),
            new() { ["lib.aslx"] = Library("""<include ref="lib.aslx" />""") });

        ok.ShouldBeFalse();
        worldModel.Errors.ShouldContain(e => e.Contains("'lib.aslx' includes itself"));
    }

    [TestMethod]
    public async Task LibrariesIncludingEachOtherAreALoadError()
    {
        var (worldModel, ok) = await LoadAsync(
            "game.aslx",
            Game("""<include ref="a.aslx" />"""),
            new()
            {
                ["a.aslx"] = Library("""<include ref="B.aslx" />"""),
                ["b.aslx"] = Library("""<include ref="A.aslx" />""")
            });

        ok.ShouldBeFalse();
        worldModel.Errors.ShouldContain(e => e.Contains("'A.aslx' includes itself"));
    }

    [TestMethod]
    public async Task AnIncludedGameFileIsALoadErrorWithoutLoadingIt()
    {
        // other.aslx is a game, not a library, and includes the game being loaded.
        var (worldModel, ok) = await LoadAsync(
            "game.aslx",
            Game("""<include ref="other.aslx" />"""),
            new() { ["other.aslx"] = Game("""<include ref="game2.aslx" />""") });

        ok.ShouldBeFalse();
        worldModel.Errors.ShouldContain(e => e.Contains("'other.aslx' is not a library"));
        worldModel.Errors.ShouldNotContain(e => e.Contains("game2.aslx"));
    }

    [TestMethod]
    public async Task ALibraryIncludedTwiceFromDifferentPlacesStillLoads()
    {
        // Not a cycle: a and b both include c, which loads (and is merged) twice as before.
        var (worldModel, ok) = await LoadAsync(
            "game.aslx",
            Game("""<include ref="English.aslx" /><include ref="a.aslx" /><include ref="b.aslx" />"""),
            new()
            {
                ["a.aslx"] = Library("""<include ref="c.aslx" />"""),
                ["b.aslx"] = Library("""<include ref="c.aslx" />"""),
                ["c.aslx"] = Library("")
            });

        ok.ShouldBeTrue(string.Join("; ", worldModel.Errors));
    }
}
