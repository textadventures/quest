using System.Text;
using ModelContextProtocol;
using QuestViva.Engine;
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
    private const string NoGame =
        "No game is loaded. Pass the path of a game file (.quest, .aslx, .asl or .cas) as game, or to start_game.";

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly bool _fixed = game != null;
    private FileInfo? _game = game;
    private int _seed = seed ?? Random.Shared.Next();
    private HeadlessSession? _session;
    private PreviewServer? _preview;

    // Loads the game at path, relative to the working directory, or with no path restarts the
    // current game.
    public Task<string> StartAsync(string? path = null) => RunAsync(async () =>
    {
        if (path != null)
        {
            var file = Resolve(path);
            // Load before switching, so a bad path leaves the current game as it was.
            var newSeed = seed ?? Random.Shared.Next();
            var session = await HeadlessSession.StartAsync(GameFiles.Load(file), newSeed);
            (_game, _seed, _session) = (file, newSeed, session);
        }
        else
        {
            var current = _game ?? throw new McpException(NoGame);
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

    // The value, then anything the game printed while evaluating it, and any script errors it didn't
    // print. A script error in a function the expression calls makes the call return null, so
    // without the error an agent would see only a null and not know why.
    public Task<string> EvaluateAsync(string expression) => RunAsync(async () =>
    {
        var evaluation = await Session.EvaluateWithOutputAsync(expression);
        var result = new StringBuilder(evaluation.Value switch
        {
            null => "null",
            "" => "\"\" (an empty string)",
            var value => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
        });

        if (evaluation.Text.Length > 0)
        {
            result.Append("\n[The game printed:]\n").Append(evaluation.Text.TrimEnd('\n'));
        }

        foreach (var error in StepText.UnprintedErrors(evaluation.Errors, evaluation.Text))
        {
            result.Append("\n[error] ").Append(error);
        }

        return result.ToString();
    });

    // Restarts the game, re-reading its file, and runs one of its walkthroughs. The game is left
    // where the walkthrough stopped, so the agent can carry on playing from there. Without a name,
    // lists the walkthroughs instead.
    public Task<string> RunWalkthroughAsync(string? name, bool transcript, string? path = null) => RunAsync(async () =>
    {
        var current = SelectGame(path);
        var session = await HeadlessSession.StartAsync(GameFiles.Load(current), _seed);
        var names = HeadlessWalkthrough.Names(session);

        if (name == null)
        {
            return names.Count == 0
                ? "The game has no walkthroughs."
                : "The game's walkthroughs: " + string.Join(", ", names);
        }

        if (!names.Contains(name))
        {
            throw new McpException(names.Count == 0
                ? "The game has no walkthroughs."
                : $"The game has no walkthrough called '{name}'. Its walkthroughs: {string.Join(", ", names)}");
        }

        _session = session;
        var result = new StringBuilder();
        WalkthroughStep? last = null;
        var outcome = await HeadlessWalkthrough.RunAsync(session, name, step =>
        {
            last = step;
            if (transcript)
            {
                result.Append(WalkthroughCommand.StepLine(step)).Append('\n');
                if (step.Response != null)
                {
                    result.Append(StepText.Format(step.Response, step.Step));
                }
            }
        });

        // Without the transcript, the last step's response: where the game was left, or what it
        // said at the step that failed.
        if (!transcript && last is { Response: { } response })
        {
            result.Append(WalkthroughCommand.StepLine(last)).Append('\n').Append(Describe(response, last.Step));
        }

        if (result.Length > 0)
        {
            result.Append('\n');
        }

        result.Append(WalkthroughCommand.Summary(outcome)).Append('\n');
        return result.ToString();
    });

    // Checks the game's file as it is on disk now, without restarting the game.
    public Task<string> CheckAsync(string? path = null) => RunAsync(async () =>
        CheckCommand.Format(await GameCheck.CheckAsync(GameFiles.Load(SelectGame(path)))));

    // Serves the game in WasmPlayer for a browser, as `qv preview` does, and returns the URL. The
    // server lasts as long as this process, and follows the current game: previewing another game
    // reloads open pages with it.
    public Task<string> PreviewAsync(string? path) => RunAsync(() =>
    {
        var game = SelectGame(path);
        if (_preview == null)
        {
            _preview = PreviewServer.Start(PreviewCommand.FindPlayer(null), game, seed: seed);
        }
        else
        {
            _preview.SetGame(game);
        }

        return Task.FromResult(
            $"Open {_preview.Url} to play {game.Name} in a browser. The page reloads when a file in the " +
            "game's folder changes. Unlike start_game, it runs the game's JavaScript and stylesheets, and " +
            "shows its layout, pictures and panes.\n");
    });

    // Publishes the game's .aslx as a .quest package, as `qv publish` does. Refusing because of
    // problems isn't an error: the report says what to fix.
    public Task<string> PublishAsync(string? path, string? output, bool force, string format = "quest") =>
        RunAsync(async () => PublishCommand.Format(await PublishCommand.PublishAsync(SelectGame(path), format,
            output == null ? null : Resolve(output), force, null), "set force"));

    // The game's functions, read from its file as it is now: its own and those of libraries the
    // author included, and with includeBuiltIn those of the libraries built into Quest too.
    public Task<string> ListFunctionsAsync(string? filter, bool includeBuiltIn, string? path = null) => RunAsync(async () =>
    {
        var functions = (await LoadFunctionsAsync(path))
            .Where(f => includeBuiltIn || !f.BuiltIn)
            .Where(f => filter == null || f.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var result = new StringBuilder();
        foreach (var function in functions)
        {
            result.Append(Signature(function)).Append("  [").Append(Source(function)).Append("]\n");
        }

        if (functions.Count == 0)
        {
            result.Append(filter == null ? "The game has no functions of its own." : $"No functions match '{filter}'.");
            result.Append(includeBuiltIn ? "\n" : " Pass include_built_in to search Quest's built-in libraries too.\n");
        }

        result.Append("\nFunctions built into the engine itself, such as GetBoolean and ListCount, aren't listed: " +
                      "look them up with docs. They take precedence over a game function with the same name.\n");
        return result.ToString();
    });

    // One function: where it comes from, its definition, and its entry in the docs if it has one.
    public Task<string> DescribeFunctionAsync(string name, string? path = null) => RunAsync(async () =>
    {
        var functions = await LoadFunctionsAsync(path);
        var function = functions.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        var docs = QuestDocs.Pages.SelectMany(p => p.Sections)
            .Where(s => s.Page.Id.StartsWith("reference/", StringComparison.Ordinal) &&
                        s.Heading.Equals(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (function == null)
        {
            if (docs.Count > 0)
            {
                return $"{name} is built into the engine, not defined in the game or a library. From the docs:\n\n" +
                       string.Join("\n", docs.Select(d => QuestDocs.Read($"{d.Page.Id}#{d.Anchor}")));
            }

            var similar = functions.Where(f => f.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Select(f => f.Name).Take(10).ToList();
            return $"There's no function called '{name}'." +
                   (similar.Count > 0 ? " Similar names: " + string.Join(", ", similar) + "." : "") + "\n";
        }

        var result = new StringBuilder();
        result.Append(Signature(function)).Append('\n');
        result.Append(function.Library == null
            ? "Defined in the game file."
            : function.BuiltIn
                ? $"Defined in {function.Library}, built into Quest. Overriding it means adding a function with " +
                  "the same name to the game file, which the game then keeps as it is."
                : $"Defined in {function.Library}, a library the game includes.");
        if (function.Overrides != null)
        {
            result.Append($" It replaces the function of the same name in {function.Overrides}: the game keeps its own copy, " +
                          $"so changes to {function.Overrides} don't reach it.");
        }

        if (GameChecker.IsEngineFunction(function.Name))
        {
            result.Append($" {function.Name} is also built into the engine: a statement, {function.Name} (...), runs this " +
                          $"function, but an expression such as x = {function.Name}(...) calls the engine's.");
        }

        result.Append("\n\n```quest\n").Append(function.Script.Trim()).Append("\n```\n");
        foreach (var section in docs)
        {
            result.Append("\nFrom the docs:\n").Append(QuestDocs.Read($"{section.Page.Id}#{section.Anchor}"));
        }

        return result.ToString();
    });

    private async Task<IReadOnlyList<FunctionInfo>> LoadFunctionsAsync(string? path) =>
        await GameFunctions.LoadAsync(GameFiles.Load(SelectGame(path)));

    // The game whose file a tool reads. Given a path, that game becomes the current one, so an agent
    // can check a game before starting it; start_game with no path then starts it. Switching to
    // another game ends the one being played.
    private FileInfo SelectGame(string? path)
    {
        if (path != null)
        {
            var file = Resolve(path);
            if (_game == null || !string.Equals(file.FullName, _game.FullName, StringComparison.Ordinal))
            {
                if (_fixed)
                {
                    throw new McpException($"This server only plays {_game!.FullName}. Leave out game.");
                }

                if (!file.Exists)
                {
                    throw new McpException($"There's no file at {file.FullName}.");
                }

                (_game, _seed, _session) = (file, seed ?? Random.Shared.Next(), null);
            }
        }

        return _game ?? throw new McpException(NoGame);
    }

    private static FileInfo Resolve(string path)
    {
        if (path.StartsWith("~/"))
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[2..]);
        }

        return new FileInfo(Path.GetFullPath(path));
    }

    private static string Signature(FunctionInfo function) =>
        $"{function.Name} ({string.Join(", ", function.Parameters)})" +
        (function.ReturnType != null ? $" returns {function.ReturnType}" : "");

    private static string Source(FunctionInfo function) =>
        (function.Library ?? (function.Overrides != null ? $"game, replacing {function.Overrides}'s" : "game")) +
        (GameChecker.IsEngineFunction(function.Name) ? $"; an expression calls the engine's own {function.Name} instead" : "");

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
