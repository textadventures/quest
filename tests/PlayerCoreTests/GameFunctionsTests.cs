using QuestViva.Common;
using QuestViva.PlayerCore;

namespace QuestViva.PlayerCoreTests;

[TestClass]
public class GameFunctionsTests
{
    private static async Task<FunctionInfo> FunctionAsync(string name)
    {
        var functions = await GameFunctions.LoadAsync(new GameData(File.ReadAllBytes("functions.aslx"), "functions.aslx",
            new Dictionary<string, byte[]> { ["checklib.aslx"] = File.ReadAllBytes("checklib.aslx") }));
        return functions.Single(f => f.Name == name);
    }

    [TestMethod]
    public async Task TheGamesOwnFunction()
    {
        var function = await FunctionAsync("TwoParams");
        CollectionAssert.AreEqual(new[] { "a", "b" }, function.Parameters.ToArray());
        Assert.AreEqual("int", function.ReturnType);
        Assert.IsNull(function.Library);
        Assert.IsNull(function.Overrides);
        StringAssert.Contains(function.Script, "return (a + b)");
    }

    [TestMethod]
    public async Task AnOverrideNamesTheLibraryItReplaces()
    {
        var function = await FunctionAsync("GetDisplayAlias");
        Assert.IsNull(function.Library);
        Assert.AreEqual("CoreDescriptions.aslx", function.Overrides);
        StringAssert.Contains(function.Script, "my own alias");
    }

    [TestMethod]
    public async Task AnIncludedLibrarysFunction()
    {
        var function = await FunctionAsync("LibraryFunction");
        Assert.AreEqual("checklib.aslx", function.Library);
        Assert.IsFalse(function.BuiltIn);
    }

    [TestMethod]
    public async Task ABuiltInLibrarysFunction()
    {
        var function = await FunctionAsync("GetDisplayName");
        Assert.AreEqual("CoreDescriptions.aslx", function.Library);
        Assert.IsTrue(function.BuiltIn);
    }

    [TestMethod]
    public async Task Quest4GamesHaveNoFunctions()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            GameFunctions.LoadAsync(new GameData(File.ReadAllBytes("quest4.asl"), "quest4.asl")));
    }
}
