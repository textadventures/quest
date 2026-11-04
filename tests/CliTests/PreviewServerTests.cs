using System.Text.Json;
using QuestViva.Cli;

namespace QuestViva.CliTests;

[TestClass]
public class PreviewServerTests
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    private DirectoryInfo _root = null!;
    private DirectoryInfo _player = null!;
    private DirectoryInfo _gameFolder = null!;
    private PreviewServer _server = null!;

    private string Base => $"http://127.0.0.1:{_server.Port}/";

    [TestInitialize]
    public void Start()
    {
        _root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "qv-preview-" + Guid.NewGuid()));
        _player = _root.CreateSubdirectory("player");
        File.WriteAllText(Path.Combine(_player.FullName, "index.html"), "<html>player</html>");
        _player.CreateSubdirectory("_framework");
        File.WriteAllText(Path.Combine(_player.FullName, "_framework", "dotnet.js"), "// runtime");
        File.WriteAllText(Path.Combine(_root.FullName, "outside.txt"), "secret");

        _gameFolder = _root.CreateSubdirectory("game");
        Write("game.aslx", "<asl version=\"580\" />");
        Write("mylib.aslx", "<library />");
        Write("other.xml", "<library />");
        Write("map.png", "png");
        Write(".secret", "secret");

        _server = PreviewServer.Start(PlayerBundle.FromFolder(_player),
            new FileInfo(Path.Combine(_gameFolder.FullName, "game.aslx")), seed: 7);
    }

    [TestCleanup]
    public async Task Stop()
    {
        await _server.DisposeAsync();
        _root.Delete(recursive: true);
    }

    private void Write(string name, string content) =>
        File.WriteAllText(Path.Combine(_gameFolder.FullName, name), content);

    private async Task<(int Status, string? Type, string Body)> GetAsync(string path)
    {
        using var response = await Http.GetAsync(Base + path);
        return ((int)response.StatusCode, response.Content.Headers.ContentType?.MediaType,
            await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public void TheUrlBootsTheQvSourceWithTheSeed()
    {
        Assert.AreEqual($"http://127.0.0.1:{_server.Port}/?source=qv&seed=7", _server.Url);
    }

    [TestMethod]
    public async Task ServesThePlayer()
    {
        Assert.AreEqual((200, "text/html", "<html>player</html>"), await GetAsync(""));
        Assert.AreEqual((200, "text/javascript", "// runtime"), await GetAsync("_framework/dotnet.js"));
        Assert.AreEqual(404, (await GetAsync("nosuch.js")).Status);
        Assert.AreEqual(404, (await GetAsync("..%2Foutside.txt")).Status);
    }

    [TestMethod]
    public async Task SaysWhichGameAndLibrariesToLoad()
    {
        var (status, type, body) = await GetAsync("qv/game");
        Assert.AreEqual(200, status);
        Assert.AreEqual("application/json", type);
        using var json = JsonDocument.Parse(body);
        Assert.AreEqual("game.aslx", json.RootElement.GetProperty("file").GetString());
        CollectionAssert.AreEqual(new[] { "mylib.aslx", "other.xml" },
            json.RootElement.GetProperty("libraries").EnumerateArray().Select(e => e.GetString()).ToList());
    }

    [TestMethod]
    public async Task ServesTheGamesFolderButNothingElse()
    {
        Assert.AreEqual((200, "image/png", "png"), await GetAsync("game/map.png"));
        Assert.AreEqual((200, "application/xml", "<asl version=\"580\" />"), await GetAsync("game/game.aslx"));
        Assert.AreEqual(404, (await GetAsync("game/.secret")).Status);
        Assert.AreEqual(404, (await GetAsync("game/..%2Foutside.txt")).Status);
        Assert.AreEqual(404, (await GetAsync("game/nosuch.png")).Status);
    }

    // Reads the event stream until a reload arrives, or returns false after the timeout.
    private async Task<bool> WaitForReloadAsync(Func<Task> action, TimeSpan timeout)
    {
        using var cancel = new CancellationTokenSource(timeout);
        using var response = await Http.GetAsync(Base + "qv/events", HttpCompletionOption.ResponseHeadersRead);
        Assert.AreEqual("text/event-stream", response.Content.Headers.ContentType?.MediaType);
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(": connected", await reader.ReadLineAsync());

        await action();
        try
        {
            while (await reader.ReadLineAsync(cancel.Token) is { } line)
            {
                if (line == "event: reload")
                {
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }

        return false;
    }

    [TestMethod]
    public async Task EditingAFileReloads()
    {
        Assert.IsTrue(await WaitForReloadAsync(() =>
        {
            Write("game.aslx", "<asl version=\"580\"><!-- edited --></asl>");
            return Task.CompletedTask;
        }, TimeSpan.FromSeconds(10)));
    }

    [TestMethod]
    public async Task HiddenFilesAndPublishedPackagesDontReload()
    {
        Assert.IsFalse(await WaitForReloadAsync(() =>
        {
            Write(".notes", "x");
            Write("game.quest", "x");
            Write("game.quest.tmp", "x");
            return Task.CompletedTask;
        }, TimeSpan.FromSeconds(2)));
    }

    [TestMethod]
    public async Task SwitchingGameReloads()
    {
        Write("second.aslx", "<asl version=\"580\" />");
        Assert.IsTrue(await WaitForReloadAsync(() =>
        {
            _server.SetGame(new FileInfo(Path.Combine(_gameFolder.FullName, "second.aslx")));
            return Task.CompletedTask;
        }, TimeSpan.FromSeconds(10)));
        StringAssert.Contains((await GetAsync("qv/game")).Body, "\"file\":\"second.aslx\"");
    }
}
