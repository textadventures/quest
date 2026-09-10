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
}
