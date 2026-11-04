using System.Text.Json.Nodes;
using QuestViva.Cli;

namespace QuestViva.CliTests;

[TestClass]
public class ServeProtocolTests
{
    private static async Task<(ServeProtocol, JsonObject)> StartAsync()
    {
        var protocol = new ServeProtocol();
        var response = await protocol.HandleAsync("""{"id":1,"op":"start","game":"headless.aslx"}""");
        return (protocol, response);
    }

    [TestMethod]
    public async Task StartReturnsOpeningText()
    {
        var (_, response) = await StartAsync();
        Assert.AreEqual(1, response["id"]!.GetValue<int>());
        Assert.IsTrue(response["ok"]!.GetValue<bool>());
        StringAssert.Contains(response["text"]!.GetValue<string>(), "Welcome.");
        Assert.IsFalse(response["finished"]!.GetValue<bool>());
    }

    [TestMethod]
    public async Task SendLeavesOutTheCommandEcho()
    {
        var (protocol, _) = await StartAsync();
        var response = await protocol.HandleAsync("""{"op":"send","input":"testwait"}""");
        Assert.AreEqual("before wait\nafter wait\n", response["text"]!.GetValue<string>());
        Assert.IsNull(response["id"]);
    }

    [TestMethod]
    public async Task MenuIsDescribedAndAnswered()
    {
        var (protocol, _) = await StartAsync();
        var response = await protocol.HandleAsync("""{"op":"send","input":"testmenu"}""");
        var menu = response["menu"]!.AsObject();
        Assert.AreEqual("Pick one", menu["caption"]!.GetValue<string>());
        Assert.AreEqual("g", menu["options"]![1]!["key"]!.GetValue<string>());
        Assert.AreEqual("Green", menu["options"]![1]!["text"]!.GetValue<string>());
        Assert.IsTrue(menu["allowCancel"]!.GetValue<bool>());

        response = await protocol.HandleAsync("""{"op":"send","input":"2"}""");
        Assert.IsNull(response["menu"]);
        StringAssert.Contains(response["text"]!.GetValue<string>(), "chose g");
    }

    [TestMethod]
    public async Task QuestionIsDescribed()
    {
        var (protocol, _) = await StartAsync();
        var response = await protocol.HandleAsync("""{"op":"send","input":"testask"}""");
        Assert.AreEqual("Sure?", response["question"]!["caption"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task ErrorsAreListed()
    {
        var (protocol, _) = await StartAsync();
        var response = await protocol.HandleAsync("""{"op":"send","input":"testerror"}""");
        Assert.HasCount(1, response["errors"]!.AsArray());
    }

    [TestMethod]
    public async Task TickRunsTimers()
    {
        var (protocol, _) = await StartAsync();
        await protocol.HandleAsync("""{"op":"send","input":"testclock"}""");
        var response = await protocol.HandleAsync("""{"op":"tick","seconds":10}""");
        StringAssert.Contains(response["text"]!.GetValue<string>(), "tock");
    }

    [TestMethod]
    public async Task EvaluateAndAssert()
    {
        var (protocol, _) = await StartAsync();
        var response = await protocol.HandleAsync("""{"op":"evaluate","expression":"player.parent.name"}""");
        Assert.AreEqual("room", response["value"]!.GetValue<string>());

        response = await protocol.HandleAsync("""{"op":"evaluate","expression":"2 + 3"}""");
        Assert.AreEqual(5, response["value"]!.GetValue<int>());

        response = await protocol.HandleAsync("""{"op":"assert","expression":"player.parent = room"}""");
        Assert.IsTrue(response["result"]!.GetValue<bool>());
    }

    [TestMethod]
    public async Task EvaluateReturnsScriptErrors()
    {
        var (protocol, _) = await StartAsync();
        var response = await protocol.HandleAsync("""{"op":"evaluate","expression":"Broken()"}""");
        Assert.IsNull(response["value"]);
        StringAssert.StartsWith(response["text"]!.GetValue<string>(), "about to fail\n");
        StringAssert.Contains(response["errors"]![0]!.GetValue<string>(), "nosuchobject");
    }

    [TestMethod]
    public async Task FinishedGameRejectsFurtherSteps()
    {
        var (protocol, _) = await StartAsync();
        var response = await protocol.HandleAsync("""{"op":"send","input":"testend"}""");
        Assert.IsTrue(response["finished"]!.GetValue<bool>());

        response = await protocol.HandleAsync("""{"id":"x","op":"send","input":"look"}""");
        Assert.AreEqual("x", response["id"]!.GetValue<string>());
        Assert.IsFalse(response["ok"]!.GetValue<bool>());
    }

    [TestMethod]
    [DataRow("not json", "invalid JSON")]
    [DataRow("[1]", "must be a JSON object")]
    [DataRow("""{"op":"fly"}""", "Unknown op 'fly'")]
    [DataRow("""{"op":"send","input":"look"}""", "No game has been started")]
    [DataRow("""{"op":"send"}""", "'input' must be a string")]
    [DataRow("""{"op":"start","game":"nosuch.quest"}""", "File not found")]
    [DataRow("""{"op":"start","game":"headless.aslx","seed":"x"}""", "'seed' must be an integer")]
    public async Task BadRequestsGetAnError(string request, string message)
    {
        var response = await new ServeProtocol().HandleAsync(request);
        Assert.IsFalse(response["ok"]!.GetValue<bool>());
        StringAssert.Contains(response["error"]!.GetValue<string>(), message);
    }

    [TestMethod]
    public async Task PaneListsAreIncluded()
    {
        var (protocol, start) = await StartAsync();
        Assert.AreEqual("south", start["exits"]![0]!.GetValue<string>());
        Assert.IsTrue(start["compassVisible"]!.GetValue<bool>());
        Assert.AreEqual("Score: 0", start["status"]![0]!.GetValue<string>());

        var response = await protocol.HandleAsync("""{"op":"send","input":"take lamp"}""");
        Assert.AreEqual("lamp", response["inventory"]![0]!.GetValue<string>());
        Assert.IsTrue(response["panesVisible"]!.GetValue<bool>());
        Assert.IsEmpty(response["objects"]!.AsArray());
    }
}
