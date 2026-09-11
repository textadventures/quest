using System.IO.Compression;
using System.Text;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class PackagerTests
{
    [TestMethod]
    public async Task CreatePackage_RoundTripsGameAndAssets()
    {
        var gameDataProvider = new FileGameDataProvider("savetest.aslx");
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);
        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        Assert.IsTrue(await worldModel.Initialise(player.Object), "Initialisation failed");

        // savetest.aslx has no gameid; publish requires an IFID for metadata.iFiction.
        worldModel.Game.Fields.Set("gameid", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var assetBytes = "asset contents"u8.ToArray();
        var includeFiles = new[]
        {
            new WorldModel.PackageIncludeFile
            {
                Filename = "asset.txt",
                Content = new MemoryStream(assetBytes)
            }
        };

        using var packageStream = new MemoryStream();
        var success = worldModel.CreatePackage(null, includeWalkthrough: true, out var error, includeFiles, packageStream);
        Assert.IsTrue(success, error);

        var packageBytes = packageStream.ToArray();
        var packageProvider = new ByteArrayGameDataProvider(packageBytes, "game.quest");
        var packageGameData = await packageProvider.GetData();
        var reloadedWorldModel = Helpers.CreateWorldModel(packageGameData);
        reloadedWorldModel.LogError += ex => throw ex;

        Assert.IsTrue(await reloadedWorldModel.Initialise(player.Object), "Reloaded package failed to initialise");

        var game = (IGame) reloadedWorldModel;
        CollectionAssert.Contains(game.GetResourceNames().ToList(), "asset.txt");

        using var reloadedAsset = game.GetResourceStream("asset.txt");
        Assert.IsNotNull(reloadedAsset);
        using var reader = new StreamReader(reloadedAsset);
        Assert.AreEqual("asset contents", await reader.ReadToEndAsync());

        // savetest.aslx's "VerifyCreatedExit" function carries folder="Verification" — asserts
        // the editor-only folder attribute (see FunctionSaver/FunctionLoaderBase) survives a
        // full save/reload round trip, not just the initial parse.
        var reloadedFunction = reloadedWorldModel.Elements.Get("VerifyCreatedExit");
        Assert.AreEqual("Verification", reloadedFunction.Fields[FieldDefinitions.EditorFolder]);
    }

    [TestMethod]
    public async Task CreatePackage_EmbedsIfidAsZipArchiveComment()
    {
        var gameDataProvider = new FileGameDataProvider("savetest.aslx");
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);
        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        Assert.IsTrue(await worldModel.Initialise(player.Object), "Initialisation failed");

        worldModel.Game.Fields.Set("gameid", "1974a053-7db0-4103-93a1-767c1382c0b7");

        using var packageStream = new MemoryStream();
        var success = worldModel.CreatePackage(null, includeWalkthrough: false, out var error,
            Array.Empty<WorldModel.PackageIncludeFile>(), packageStream);
        Assert.IsTrue(success, error);

        var packageBytes = packageStream.ToArray();
        const string expected = "UUID://1974A053-7DB0-4103-93A1-767C1382C0B7//\n";
        StringAssert.Contains(Encoding.ASCII.GetString(packageBytes), expected);

        packageStream.Position = 0;
        using var zip = new ZipArchive(packageStream, ZipArchiveMode.Read, leaveOpen: true);
        Assert.AreEqual(expected, zip.Comment);
    }

    [TestMethod]
    public async Task CreatePackage_WritesEntriesWithoutByteOrderMark()
    {
        var gameDataProvider = new FileGameDataProvider("savetest.aslx");
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);
        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        Assert.IsTrue(await worldModel.Initialise(player.Object), "Initialisation failed");

        worldModel.Game.Fields.Set("gameid", "5d3c2a1b-0f9e-4d8c-b7a6-958473625140");

        using var packageStream = new MemoryStream();
        var success = worldModel.CreatePackage(null, includeWalkthrough: false, out var error,
            Array.Empty<WorldModel.PackageIncludeFile>(), packageStream);
        Assert.IsTrue(success, error);

        packageStream.Position = 0;
        using var zip = new ZipArchive(packageStream, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var name in (string[]) ["game.aslx", "metadata.iFiction"])
        {
            var entry = zip.GetEntry(name);
            Assert.IsNotNull(entry, $"Published .quest should contain {name}");
            using var entryStream = entry.Open();
            using var entryBytes = new MemoryStream();
            await entryStream.CopyToAsync(entryBytes);
            Assert.IsFalse(entryBytes.ToArray().AsSpan().StartsWith(Encoding.UTF8.Preamble),
                $"{name} should not start with a UTF-8 byte order mark");
        }
    }

    [TestMethod]
    public async Task CreatePackage_EmbedsMetadataIFiction()
    {
        var gameDataProvider = new FileGameDataProvider("savetest.aslx");
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);
        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        Assert.IsTrue(await worldModel.Initialise(player.Object), "Initialisation failed");

        worldModel.Game.Fields.Set("gameid", "1652e7ec-2d7d-4b59-95ed-e65d79edf257");
        worldModel.Game.Fields.Set("author", "Test Author");
        worldModel.Game.Fields.Set("subtitle", "An Interactive Test");
        worldModel.Game.Fields.Set("category", "Fantasy");
        worldModel.Game.Fields.Set("firstpublished", "2026");
        worldModel.Game.Fields.Set("version", "1.0");
        worldModel.Game.Fields.Set("versioncode", 3);
        worldModel.Game.Fields.Set("description", "Line one.<br/>Line two.");
        worldModel.Game.Fields.Set("cover", "cover.png");

        // Minimal 1x1 PNG
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");
        var includeFiles = new[]
        {
            new WorldModel.PackageIncludeFile
            {
                Filename = "cover.png",
                Content = new MemoryStream(png)
            }
        };

        using var packageStream = new MemoryStream();
        var success = worldModel.CreatePackage(null, includeWalkthrough: false, out var error, includeFiles, packageStream);
        Assert.IsTrue(success, error);

        packageStream.Position = 0;
        using var zip = new ZipArchive(packageStream, ZipArchiveMode.Read, leaveOpen: true);
        var entry = zip.GetEntry("metadata.iFiction");
        Assert.IsNotNull(entry, "Published .quest should contain metadata.iFiction");

        using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream, Encoding.UTF8);
        var ifiction = await reader.ReadToEndAsync();

        StringAssert.Contains(ifiction, "<ifid>1652E7EC-2D7D-4B59-95ED-E65D79EDF257</ifid>");
        StringAssert.Contains(ifiction, "<format>quest</format>");
        StringAssert.Contains(ifiction, "<title>savetest</title>");
        StringAssert.Contains(ifiction, "<author>Test Author</author>");
        StringAssert.Contains(ifiction, "<headline>An Interactive Test</headline>");
        StringAssert.Contains(ifiction, "<genre>Fantasy</genre>");
        StringAssert.Contains(ifiction, "<firstpublished>2026</firstpublished>");
        StringAssert.Contains(ifiction, "<description>Line one.<br/>Line two.</description>");
        StringAssert.Contains(ifiction, "<language>en</language>");
        StringAssert.Contains(ifiction, "<cover>");
        StringAssert.Contains(ifiction, "<format>png</format>");
        StringAssert.Contains(ifiction, "<height>1</height>");
        StringAssert.Contains(ifiction, "<width>1</width>");
        StringAssert.Contains(ifiction, "<generator>Quest Viva</generator>");
        StringAssert.Contains(ifiction, "<quest>");
        StringAssert.Contains(ifiction, "<style>Text Adventure</style>");
        StringAssert.Contains(ifiction, "<version>1.0</version>");
        StringAssert.Contains(ifiction, "<coverleafname>cover.png</coverleafname>");
        StringAssert.Contains(ifiction, "<releases>");
        StringAssert.Contains(ifiction, "<attached>");
        StringAssert.Contains(ifiction, "<version>3</version>");
        StringAssert.Contains(ifiction, "<compiler>Quest Viva</compiler>");
        StringAssert.Contains(ifiction, $"<compilerversion>{VersionInfo.Version}</compilerversion>");
        StringAssert.Contains(ifiction, "<releasedate>");

        // metadata.iFiction is a bibliographic sidecar, not a playable game resource
        var packageProvider = new ByteArrayGameDataProvider(packageStream.ToArray(), "game.quest");
        var packageGameData = await packageProvider.GetData();
        var reloaded = Helpers.CreateWorldModel(packageGameData);
        Assert.IsTrue(await reloaded.Initialise(player.Object), "Reloaded package failed to initialise");
        CollectionAssert.DoesNotContain(((IGame)reloaded).GetResourceNames().ToList(), "metadata.iFiction");
        CollectionAssert.Contains(((IGame)reloaded).GetResourceNames().ToList(), "cover.png");
    }
}
