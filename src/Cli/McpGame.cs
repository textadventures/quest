using System.Text;
using ModelContextProtocol;
using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// The game behind `qv mcp`, shared by its tools. Tool calls can arrive concurrently and a
// HeadlessSession handles one step at a time, so calls are serialised here.
//
// The game is either fixed when the server starts, or loaded by start_game. Each game gets a seed when
// it is loaded - the --seed one, or else a random one - and keeps it when restarted.
//
// Unless textOnly, a step also gives what a browser player sees beside the text: the exits when the
// game shows its compass (many games leave them out of their room descriptions for that reason),
// and the status pane, such as score and health, when it shows its panes.
internal sealed class McpGame(FileInfo? game, int? seed, bool textOnly = false)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private FileInfo? _game = game;
    private int _seed = seed ?? Random.Shared.Next();
    private HeadlessSession? _session;

    // Loads the game at path, relative to the working directory, or with no path restarts the
    // current game.
    public Task<string> StartAsync(string? path = null) => RunAsync(async () =>
    {
        if (path != null)
        {
            if (path.StartsWith("~/"))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[2..]);
            }

            var file = new FileInfo(Path.GetFullPath(path));
            // Load before switching, so a bad path leaves the current game as it was.
            var newSeed = seed ?? Random.Shared.Next();
            var session = await HeadlessSession.StartAsync(GameFiles.Load(file), newSeed);
            (_game, _seed, _session) = (file, newSeed, session);
        }
        else
        {
            var current = _game ?? throw new McpException(
                "No game is loaded. Pass the path of a game file (.quest, .aslx, .asl or .cas) to start_game.");
            _session = await HeadlessSession.StartAsync(GameFiles.Load(current), _seed);
        }

        return Describe(_session.Opening, null);
    });

    public Task<string> SendAsync(string input) =>
        RunAsync(async () => Describe(await Session.SendAsync(input), input));

    public Task<string> TickAsync(int seconds) =>
        RunAsync(async () => Describe(await Session.TickAsync(seconds), null));

    public Task<string> SendEventAsync(string name, string param) =>
        RunAsync(async () => Describe(await Session.SendEventAsync(name, param), null));

    public Task<string> EvaluateAsync(string expression) =>
        RunAsync(async () => Convert.ToString(await Session.EvaluateAsync(expression),
            System.Globalization.CultureInfo.InvariantCulture) ?? "null");

    private HeadlessSession Session =>
        _session ?? throw new InvalidOperationException("No game is running. Call start_game first.");

    private async Task<string> RunAsync(Func<Task<string>> action)
    {
        await _lock.WaitAsync();
        try
        {
            return await action();
        }
        // The SDK reports other exceptions to the client only as "An error occurred", which leaves an
        // agent nothing to go on - "call start_game first", or why an expression didn't evaluate.
        catch (Exception ex) when (ex is not McpException)
        {
            throw new McpException(ex.Message, ex);
        }
        finally
        {
            _lock.Release();
        }
    }

    // A step as an agent reads it: the game's text, then anything it is waiting for that the text
    // doesn't already show.
    private string Describe(HeadlessStep step, string? input)
    {
        var text = StepText.Format(step, input);
        var result = new StringBuilder(text);

        foreach (var error in StepText.UnprintedErrors(step, text))
        {
            result.Append("[error] ").Append(error).Append('\n');
        }

        if (!textOnly && step is { CompassVisible: true, Finished: false, Exits.Count: > 0 })
        {
            result.Append("[Exits: ").AppendJoin(", ", step.Exits).Append("]\n");
        }

        if (!textOnly && step is { PanesVisible: true, Finished: false, Status.Count: > 0 })
        {
            result.Append("[Status: ").AppendJoin(" | ", step.Status).Append("]\n");
        }

        if (step.PendingMenu is { Inline: false } menu)
        {
            result.Append('\n').Append(menu.Caption).Append('\n');
            var n = 1;
            foreach (var option in menu.Options.Values)
            {
                result.Append(n++).Append(": ").Append(option).Append('\n');
            }
        }
        else if (step.PendingQuestion is { } question && !step.PendingQuestionInline)
        {
            result.Append('\n').Append(question).Append(" (yes/no)\n");
        }

        if (step.PendingMenu != null && step.Output.Count == 0)
        {
            result.Append("[Choose an option by its number.]\n");
        }

        if (step.Finished)
        {
            result.Append("[The game has ended. Call start_game to play again.]\n");
        }

        return result.Length == 0 ? "[No output.]" : result.ToString();
    }
}
