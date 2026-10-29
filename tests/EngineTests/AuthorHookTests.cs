using System.Collections;
using System.Text;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// The author hook functions (SetPaneVisible, AddPane, SetAutoScroll, ... in CoreOutput) each call
// their QuestUI member and keep what they set on game, so InitInterface can apply it again when a
// saved game is restored into a UI that has started afresh.
[TestClass]
public class AuthorHookTests
{
    private static string Arg(object? value) => value switch
    {
        null => "",
        string s => s,
        bool b => b ? "true" : "false",
        IEnumerable list => string.Join(";", list.Cast<object>()),
        _ => value.ToString() ?? ""
    };

    // Each QuestUI call made since the mock was last cleared, as "member(arg, arg)"
    private static List<string> Calls(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && ((string)i.Arguments[0]).StartsWith("QuestUI."))
            .Select(i => $"{((string)i.Arguments[0])["QuestUI.".Length..]}" +
                         $"({string.Join(", ", ((object[])i.Arguments[1]).Select(Arg))})")
            .ToList();

    private static readonly string[] HookCalls =
    [
        "addPane(clues, Clues, )",
        "setPaneContent(clues, No clues yet.)",
        "setPaneContent(customstatus, Score: 1)",
        "setCommands(Look;Wait:z)",
        "setVisible(commands, true)",
        "setVisible(compass, false)",
        "setVisible(status, false)",
        "setAutoScroll(false)",
        "setContinueOnClick(true)",
        "fadePreviousOutput(true)",
        "setCommandPlaceholder(What now?)",
        "setMapInteractive(false)"
    ];

    private static async Task<GameDriver> RestoreAsync(GameDriver driver)
    {
        var saveData = driver.Model.Save(SaveMode.SavedGame);
        return await GameDriver.LoadAsync(new GameData(File.ReadAllBytes("authorhookstest.aslx"), "authorhookstest.aslx"),
            saveData: new MemoryStream(Encoding.UTF8.GetBytes(saveData)));
    }

    [TestMethod]
    public async Task ANewGame_SendsNoHooks()
    {
        var driver = await GameDriver.LoadAsync("authorhookstest.aslx");

        var calls = Calls(driver);
        calls.ShouldNotContain(c => c.StartsWith("addPane") || c.StartsWith("setAutoScroll")
                                    || c.StartsWith("setContinueOnClick") || c.StartsWith("fadePreviousOutput")
                                    || c.StartsWith("setCommandPlaceholder") || c.StartsWith("setMapInteractive")
                                    || c.StartsWith("setCommands") || c.StartsWith("setPaneContent"));
    }

    [TestMethod]
    public async Task EachFunction_CallsItsQuestUIMember()
    {
        var driver = await GameDriver.LoadAsync("authorhookstest.aslx");
        driver.PlayerMock.Invocations.Clear();

        await driver.SendCommandAsync("setup");

        var calls = Calls(driver);
        foreach (var call in HookCalls)
        {
            calls.ShouldContain(call);
        }
    }

    [TestMethod]
    public async Task RestoringASave_AppliesThemAgain()
    {
        var driver = await GameDriver.LoadAsync("authorhookstest.aslx");
        await driver.SendCommandAsync("setup");

        var restored = await RestoreAsync(driver);

        var calls = Calls(restored);
        foreach (var call in HookCalls)
        {
            calls.ShouldContain(call);
        }
        // The panes are added before anything is put in them
        calls.IndexOf("addPane(clues, Clues, )").ShouldBeLessThan(calls.IndexOf("setPaneContent(clues, No clues yet.)"));
    }

    [TestMethod]
    public async Task PartsWithAnInterfaceOption_KeepItInStep()
    {
        var driver = await GameDriver.LoadAsync("authorhookstest.aslx");
        await driver.SendCommandAsync("setup");

        var game = driver.Model.Game;
        game.Fields.GetAsType<bool>("turnoffcompass").ShouldBeTrue();
        game.Fields.GetAsType<bool>("commandpane").ShouldBeTrue();
        game.Fields.GetAsType<QuestList<string>>("hiddenparts").ShouldBe(["status"]);
    }

    [TestMethod]
    public async Task TurningThemBackOn_IsKeptToo()
    {
        var driver = await GameDriver.LoadAsync("authorhookstest.aslx");
        await driver.SendCommandAsync("setup");
        await driver.SendCommandAsync("undo hooks");

        var restored = await RestoreAsync(driver);

        var calls = Calls(restored);
        calls.ShouldNotContain("setVisible(compass, false)");
        calls.ShouldNotContain("setVisible(status, false)");
        calls.ShouldNotContain("setAutoScroll(false)");
        calls.ShouldContain("setContinueOnClick(true)");
    }
}
