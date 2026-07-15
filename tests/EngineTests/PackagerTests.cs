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
    }
}
