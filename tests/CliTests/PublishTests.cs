using System.IO.Compression;
using QuestViva.Cli;
using QuestViva.Common;
using QuestViva.PlayerCore;

namespace QuestViva.CliTests;

[TestClass]
public class PublishTests
{
    private const string Game = """
        <asl version="580">
          <include ref="English.aslx" />
          <include ref="Core.aslx" />
          <game name="Publish test">
            GAMEID
            <start type="script">
              msg ("Hello.")
              picture ("map.png")
            </start>
          </game>
          <object name="room">
            <object name="player" />
            <object name="lamp">
              <picture>lamp.png</picture>
            </object>
          </object>
          <walkthrough name="main">
            <steps>look</steps>
          </walkthrough>
        </asl>
        """;

    private DirectoryInfo _folder = null!;

    [TestInitialize]
    public void CreateFolder() =>
        _folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "qv-publish-" + Guid.NewGuid()));

    [TestCleanup]
    public void DeleteFolder() => _folder.Delete(recursive: true);

    private FileInfo Write(string name, string content)
    {
        var path = Path.Combine(_folder.FullName, name);
        File.WriteAllText(path, content);
        return new FileInfo(path);
    }

    private FileInfo WriteGame(string? gameId = "a1b2c3d4-0000-4000-8000-000000000001") =>
        Write("game.aslx", Game.Replace("GAMEID", gameId == null ? "" : $"<gameid>{gameId}</gameid>"));

    private static List<string> Entries(string package)
    {
        using var zip = ZipFile.OpenRead(package);
        return zip.Entries.Select(e => e.FullName).Order(StringComparer.Ordinal).ToList();
    }

    [TestMethod]
    public async Task PackagesTheGameWithItsFolderButNotEarlierOutput()
    {
        var game = WriteGame();
        Write("map.png", "png");
        Write("lamp.png", "png");
        Write("mylib.aslx", "<library />");
        Write("notes.xml", "<x />");
        Write("old.quest", "old");
        Write("game.html", "old");
        Write("game.zip", "old");
        Write("game-html.zip", "old");
        Write(".hidden", "x");
        Write("._map.png", "junk");

        var result = await GamePublisher.PublishAsync(game, null, force: false);

        Assert.AreEqual(Path.Combine(_folder.FullName, "game.quest"), result.Output);
        CollectionAssert.AreEqual(new[] { "lamp.png", "map.png" }, result.Files.Select(f => f.Name).ToList());
        Assert.IsEmpty(result.Warnings);
        CollectionAssert.AreEqual(new[] { "game.aslx", "lamp.png", "map.png", "metadata.iFiction" }, Entries(result.Output!));
    }

    [TestMethod]
    public async Task ThePackagePlays()
    {
        var result = await GamePublisher.PublishAsync(WriteGame(), null, force: false);
        var session = await HeadlessSession.StartAsync(
            new GameData(File.ReadAllBytes(result.Output!), result.Output!));
        StringAssert.Contains(session.Opening.Text, "Hello.");
    }

    [TestMethod]
    public async Task KeepsTheGamesIfid()
    {
        var result = await GamePublisher.PublishAsync(WriteGame(), null, force: false);
        using var zip = ZipFile.OpenRead(result.Output!);
        StringAssert.Contains(zip.Comment, "UUID://A1B2C3D4-0000-4000-8000-000000000001//");
    }

    [TestMethod]
    public async Task WarnsWhenTheGameHasNoGameid()
    {
        Write("map.png", "png");
        Write("lamp.png", "png");
        var result = await GamePublisher.PublishAsync(WriteGame(gameId: null), null, force: false);
        var warning = result.Warnings.Single();
        StringAssert.Contains(warning, "no gameid");

        // The warning gives the id the package was given, so the author can keep it.
        var id = warning.Split("<gameid>")[1].Split("</gameid>")[0];
        using var zip = ZipFile.OpenRead(result.Output!);
        StringAssert.Contains(zip.Comment, $"UUID://{id.ToUpperInvariant()}//");
    }

    [TestMethod]
    public async Task WarnsAboutFilesTheGameRefersToThatArentThere()
    {
        Write("lamp.png", "png");
        var result = await GamePublisher.PublishAsync(WriteGame(), null, force: false);
        Assert.AreEqual("The game refers to 'map.png', which isn't in the game's folder.", result.Warnings.Single());
    }

    [TestMethod]
    public async Task RefusesAGameWithProblemsUnlessForced()
    {
        File.Copy("check.aslx", Path.Combine(_folder.FullName, "check.aslx"));
        File.Copy("checklib.aslx", Path.Combine(_folder.FullName, "checklib.aslx"));
        var game = new FileInfo(Path.Combine(_folder.FullName, "check.aslx"));

        var refused = await GamePublisher.PublishAsync(game, null, force: false);
        Assert.IsNull(refused.Output);
        Assert.IsNotEmpty(refused.Problems!.Problems);
        Assert.IsFalse(File.Exists(Path.Combine(_folder.FullName, "check.quest")));
        StringAssert.Contains(PublishCommand.Format(refused), "pass --force to publish anyway");

        var forced = await GamePublisher.PublishAsync(game, null, force: true);
        Assert.IsTrue(File.Exists(forced.Output));
        StringAssert.StartsWith(forced.Warnings.First(), "Published anyway");
    }

    [TestMethod]
    public async Task WritesWhereItsTold()
    {
        Write("map.png", "png");
        Write("lamp.png", "png");
        var output = new FileInfo(Path.Combine(_folder.FullName, "out", "release.quest"));
        output.Directory!.Create();
        var result = await GamePublisher.PublishAsync(WriteGame(), output, force: false);
        Assert.AreEqual(output.FullName, result.Output);
        Assert.IsTrue(output.Exists);
    }

    [TestMethod]
    public async Task OnlyPublishesAslx()
    {
        var quest = Write("game.quest", "x");
        var ex = await Assert.ThrowsAsync<NotSupportedException>(() => GamePublisher.PublishAsync(quest, null, false));
        StringAssert.Contains(ex.Message, "already published");
    }

    [TestMethod]
    public async Task TheMcpToolPublishesAGameByPath()
    {
        Write("map.png", "png");
        Write("lamp.png", "png");
        var game = WriteGame();
        var result = await new McpGame(null, null).PublishAsync(game.FullName, null, false);
        StringAssert.StartsWith(result, $"Published {Path.Combine(_folder.FullName, "game.quest")}\n");
        StringAssert.Contains(result, "  map.png (3 bytes)\n");
    }
}
