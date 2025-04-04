using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.PlayerCore;

namespace QuestViva.PlayerCoreTests;

[TestClass]
public class GameQueryTests
{
    private class Config : IConfig
    {
        public bool UseNCalc => false;
        public string? HomeFile { get; }
        public bool DevEnabled { get; }
    }
    
    private GameQuery GetGameQuery(string filename)
    {
        var factory = new WorldModelFactory(new Config());
        return new GameQuery(factory, filename);
    }
    
    [TestMethod]
    public async Task TestValidASL()
    {
        var query = GetGameQuery("test1.asl");
        var result = await query.Initialise();
        Assert.IsTrue(result);
        Assert.AreEqual("Test ASL Game", query.GameName);
        Assert.AreEqual(410, query.ASLVersion);
        Assert.AreEqual(null, query.GameId);
        Assert.AreEqual(null, query.Category);
        Assert.AreEqual(null, query.Description);
    }

    [TestMethod]
    public async Task TestInvalidASL()
    {
        var query = GetGameQuery("test2.asl");
        var result = await query.Initialise();
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public async Task TestValidQuest()
    {
        var query = GetGameQuery("test1.quest");
        var result = await query.Initialise();
        Assert.IsTrue(result);
        Assert.AreEqual("Test ASLX Game", query.GameName);
        Assert.AreEqual(520, query.ASLVersion);
        Assert.AreEqual("33cb328d-bf80-42f7-a136-e916e7b45ed8", query.GameId);
        Assert.AreEqual("Test Category", query.Category);
        Assert.AreEqual("Test Description", query.Description);
    }
    
    [TestMethod]
    public async Task TestInvalidQuest()
    {
        var query = GetGameQuery("test2.quest");
        var result = await query.Initialise();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task TestQuestResources()
    {
        var query = GetGameQuery("resources.quest");
        var result = await query.Initialise();
        Assert.IsTrue(result);
        var resources = query.GetResourceNames()?.ToList();
        Assert.IsNotNull(resources);
        Assert.AreEqual(3, resources.Count);
        Assert.IsTrue(resources.Contains("game.aslx"));
        Assert.IsTrue(resources.Contains("aw.jpg"));
        Assert.IsTrue(resources.Contains("ta.png"));
        
        var stream = query.GetResource("aw.jpg");
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        Assert.AreEqual(0xd8, bytes[1]);
    }
}