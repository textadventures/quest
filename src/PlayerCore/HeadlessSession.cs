using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Legacy;

namespace QuestViva.PlayerCore;

public enum HeadlessOutputKind
{
    Text,
    Error
}

// One chunk of output, in the order the game produced it. Text is already converted to plain text;
// Error is the message of a script error the engine logged.
public sealed record HeadlessOutput(HeadlessOutputKind Kind, string Text);

// Everything one call produced, and what the game is waiting for afterwards.
public sealed class HeadlessStep
{
    public required IReadOnlyList<HeadlessOutput> Output { get; init; }
    public MenuData? PendingMenu { get; init; }
    public string? PendingQuestion { get; init; }

    // True when the game has already drawn the question in its text (v600+), as MenuData.Inline is
    // for a menu.
    public bool PendingQuestionInline { get; init; }
    public bool Finished { get; init; }

    // What a browser player shows in its compass and side panes, as display names: the exits (both
    // compass directions and named places), the visible objects, and the inventory. CompassVisible and
    // PanesVisible say whether the game shows them at all.
    public IReadOnlyList<string> Exits { get; init; } = [];
    public IReadOnlyList<string> Objects { get; init; } = [];
    public IReadOnlyList<string> Inventory { get; init; } = [];

    // The status pane's lines, such as "Score: 5", if the game has any.
    public IReadOnlyList<string> Status { get; init; } = [];
    public bool PanesVisible { get; init; }
    public bool CompassVisible { get; init; }

    public IEnumerable<string> Errors =>
        Output.Where(o => o.Kind == HeadlessOutputKind.Error).Select(o => o.Text);

    // The text output as one normalised string, one chunk per line, errors left out.
    public string Text => HeadlessSession.Normalise(string.Concat(
        Output.Where(o => o.Kind == HeadlessOutputKind.Text).Select(o => o.Text.EndsWith('\n') ? o.Text : o.Text + "\n")));
}

// Plays a game with no UI: a command goes in, and that turn's plain text comes back, along with
// any menu or question the game is now waiting on. Waits, pauses and synchronous sounds are
// advanced automatically, and SetTimeout timers that come due are drained after every step, so a
// step returns once the game is waiting for the player again.
//
// SendAsync, AnswerMenuAsync and AnswerQuestionAsync are what a player could do. Tick, SendEvent,
// Assert and Evaluate reach past that - an agent playing the game should not be given them.
public sealed class HeadlessSession
{
    public const int DefaultSeed = 1234;

    private static readonly string[] Quest4Extensions = [".asl", ".cas", ".qsg"];

    // _world is the same game as _game for Quest 5 and later, and null for a Quest 1–4 game, which
    // V4Game plays.
    private readonly IGame _game;
    private readonly WorldModel? _world;
    private readonly HeadlessPlayer _player;
    private bool _finished;
    private List<HeadlessOutput> _output = [];
    private int _pendingTick;
    private List<ListData> _exits = [];
    private List<ListData> _objects = [];
    private List<ListData> _inventory = [];

    private HeadlessSession(GameData gameData, int seed)
    {
        _game = Quest4Extensions.Contains(Path.GetExtension(gameData.Filename).ToLowerInvariant())
            ? new V4Game(gameData, null)
            : new WorldModel(gameData, null);
        _world = _game as WorldModel;
        _game.SetRandomSeed(seed);
        _game.LogError += ex => Write(new HeadlessOutput(HeadlessOutputKind.Error, ex.Message));
        _game.PrintText += WriteHtml; // output path for Quest 1–4 and ASL < v540
        _game.RequestNextTimerTick += seconds => _pendingTick = seconds;
        _game.UpdateList += OnUpdateList;
        _game.Finished += () => _finished = true;
        _player = new HeadlessPlayer(WriteHtml);
    }

    // Every chunk of output as it is produced, including any from Assert/Evaluate, which return
    // no HeadlessStep.
    public event Action<HeadlessOutput>? OutputWritten;

    // The opening text, up to the first point the game waits for the player.
    public HeadlessStep Opening { get; private set; } = null!;

    public MenuData? PendingMenu => _player.PendingMenu;
    public string? PendingQuestion => _player.PendingQuestion;
    public bool PendingQuestionInline => _player.PendingQuestionInline;
    public GameState State => _world?.State ?? (_finished ? GameState.Finished : GameState.Running);
    public bool Finished => State == GameState.Finished;

    // The underlying game, for privileged inspection such as IGameDebug.GetObjects.
    public IGameDebug Game => (IGameDebug) _game;

    public static async Task<HeadlessSession> StartAsync(GameData gameData, int seed = DefaultSeed,
        Action<HeadlessOutput>? onOutput = null)
    {
        var session = new HeadlessSession(gameData, seed);
        if (onOutput != null)
        {
            session.OutputWritten += onOutput;
        }

        if (!await session._game.Initialise(session._player))
        {
            throw new InvalidOperationException(
                $"Game '{gameData.GameId}' failed to load: {string.Join("; ", session._game.Errors)}");
        }

        session.Opening = await session.RunStepAsync(() => session._game.Begin());
        return session;
    }

    // Input as a player would type it: the answer to a pending menu or question, or else a command.
    // A menu answer may be an option's key, its text, or its 1-based number; a question answer is
    // yes if it is "yes", "y", "true" or "1". An answer that matches no menu option cancels the menu
    // if it can be cancelled, and otherwise does nothing, leaving the menu pending.
    public Task<HeadlessStep> SendAsync(string input)
    {
        if (PendingMenu != null)
        {
            return AnswerMenuAsync(input);
        }

        if (PendingQuestion != null)
        {
            return AnswerQuestionAsync(input.Trim().ToLowerInvariant() is "yes" or "y" or "true" or "1");
        }

        return SendCommandAsync(input);
    }

    public Task<HeadlessStep> SendCommandAsync(string command) =>
        RunStepAsync(() => _game.SendCommand(command));

    public Task<HeadlessStep> AnswerMenuAsync(string choice)
    {
        var menu = PendingMenu ?? throw new InvalidOperationException("No menu is pending.");
        var key = ResolveMenuKey(menu, choice);
        if (key == null && !menu.AllowCancel)
        {
            // Cancelling a menu that can't be cancelled leaves its script waiting forever. A player
            // can't do it either: the engine ignores anything but an option while such a menu is up.
            return Task.FromResult(CurrentStep([]));
        }

        _player.PendingMenu = null;
        return RunStepAsync(() => _game.SetMenuResponse(key));
    }

    public Task<HeadlessStep> AnswerQuestionAsync(bool answer)
    {
        if (PendingQuestion == null)
        {
            throw new InvalidOperationException("No question is pending.");
        }

        _player.PendingQuestion = null;
        return RunStepAsync(() => _game.SetQuestionResponse(answer));
    }

    // Advances the game clock, firing any timers that come due - the stand-in for a browser
    // player's one-second interval, for games gated on real-time timers. SetTimeout timers are
    // already drained after every step.
    public Task<HeadlessStep> TickAsync(int seconds) =>
        RunStepAsync(async () =>
        {
            _pendingTick = 0;
            if (_world != null)
            {
                await _world.Tick(seconds);
                return;
            }

            // A Quest 1–4 timer skips the first tick after it's turned on, however long that tick is,
            // so a single Tick(seconds) would never fire it. Tick a second at a time, as a browser
            // player does, stopping at anything the player has to answer.
            for (var i = 0; i < seconds && !Finished && PendingMenu == null && PendingQuestion == null; i++)
            {
                await TickGameAsync(1);
                await AutoAdvanceAsync();
            }
        });

    // Fires an event a game's own JavaScript would normally send with ASLEvent.
    public Task<HeadlessStep> SendEventAsync(string eventName, string param) =>
        RunStepAsync(() => _game.SendEvent(eventName, param));

    // For a Quest 1–4 game the expression is a condition in that syntax, such as "got <lamp>".
    public Task<bool> AssertAsync(string expression) => Game.AssertAsync(expression);

    public Task<object?> EvaluateAsync(string expression) =>
        _world?.EvaluateAsync(expression)
        ?? throw new NotSupportedException("Games from Quest 4 and earlier can't evaluate expressions. Use assert with a condition in their syntax, such as \"got <lamp>\", instead.");

    // V4Game's Tick doesn't return until a prompt one of its timers opens has been answered, which
    // only the caller can do once this returns.
    private Task TickGameAsync(int seconds) =>
        _world?.Tick(seconds) ?? Game.TickUntilSuspended(seconds);

    private async Task<HeadlessStep> RunStepAsync(Func<Task> action)
    {
        if (Finished)
        {
            throw new InvalidOperationException("The game has finished.");
        }

        await action();
        await AutoAdvanceAsync();
        await DrainTimersAsync();
        // Taken here rather than cleared before the action, so errors logged while the game loads
        // land in Opening.
        var output = _output;
        _output = [];
        return CurrentStep(output);
    }

    private HeadlessStep CurrentStep(IReadOnlyList<HeadlessOutput> output)
    {
        // The objects list includes the exits too; a browser player draws those in the compass.
        var exitKeys = _exits.Select(ListKey).ToHashSet();
        // Quest 1–4 games always show their panes and compass.
        var panesVisible = _world?.Game.Fields.GetAsType<bool>("showpanes") ?? true;
        return new HeadlessStep
        {
            Output = output,
            PendingMenu = PendingMenu,
            PendingQuestion = PendingQuestion,
            PendingQuestionInline = PendingQuestionInline,
            Finished = Finished,
            Exits = DisplayNames(_exits),
            Objects = DisplayNames(_objects.Where(o => !exitKeys.Contains(ListKey(o)))),
            Inventory = DisplayNames(_inventory),
            Status = _player.Status,
            PanesVisible = panesVisible,
            CompassVisible = panesVisible && _world?.Game.Fields.GetAsType<bool>("turnoffcompass") != true
        };
    }

    private void OnUpdateList(ListType listType, List<ListData> items)
    {
        switch (listType)
        {
            case ListType.ExitsList:
                _exits = items;
                break;
            case ListType.ObjectsList:
                _objects = items;
                break;
            case ListType.InventoryList:
                _inventory = items;
                break;
        }
    }

    // Quest 1–4 list items have no element id, only a name.
    private static string ListKey(ListData item) => item.ElementId ?? "name:" + item.ElementName;

    // A game's GetListDisplayAlias can return null, despite ListData.Text's type.
    private static List<string> DisplayNames(IEnumerable<ListData> items) =>
        items.Select(i => HtmlToText((string?) i.Text ?? i.ElementName ?? ""))
            .Where(name => name.Length > 0)
            .ToList();

    private async Task AutoAdvanceAsync()
    {
        while (!Finished && (_player.IsWaiting || _player.IsPausing))
        {
            if (_player.IsWaiting)
            {
                _player.IsWaiting = false;
                await _game.FinishWait();
            }

            if (_player.IsPausing)
            {
                _player.IsPausing = false;
                await _game.FinishPause();
            }
        }
    }

    private bool HasPendingTimeout() =>
        _world != null && _world.Elements.GetElements(ElementType.Timer)
            .Any(t => t.Name.StartsWith("timeout", StringComparison.Ordinal) && t.Fields.GetAsType<bool>("enabled"));

    // SetTimeout creates "timeout*" timers. Fire them until none are left, so a step's delayed
    // text arrives with the step. Other timers only run on TickAsync.
    private async Task DrainTimersAsync()
    {
        var guard = 0;
        while (!Finished && guard++ < 500 && HasPendingTimeout())
        {
            var seconds = _pendingTick;
            _pendingTick = 0;
            await TickGameAsync(seconds > 0 ? seconds : 1);
            await AutoAdvanceAsync();
        }
    }

    private void WriteHtml(string html)
    {
        var text = HtmlToText(html);
        if (text.Length > 0)
        {
            Write(new HeadlessOutput(HeadlessOutputKind.Text, text));
        }
    }

    private void Write(HeadlessOutput output)
    {
        _output.Add(output);
        OutputWritten?.Invoke(output);
    }

    private static string? ResolveMenuKey(MenuData menu, string choice)
    {
        if (menu.Options.ContainsKey(choice))
        {
            return choice;
        }

        foreach (var option in menu.Options)
        {
            if (string.Equals(option.Value, choice, StringComparison.OrdinalIgnoreCase))
            {
                return option.Key;
            }
        }

        if (int.TryParse(choice, out var n) && n >= 1 && n <= menu.Options.Count)
        {
            return menu.Options.Keys.ElementAt(n - 1);
        }

        return null;
    }

    // Strips the constrained HTML Quest emits down to plain text.
    public static string HtmlToText(string html)
    {
        var text = Regex.Replace(html, "<br\\s*/?>", "\n");
        text = Regex.Replace(text, "<[^>]+>", "");
        return WebUtility.HtmlDecode(text);
    }

    // Stable for diffing: strips trailing whitespace from each line, collapses runs of blank lines
    // to one, and ends with a single newline.
    public static string Normalise(string text)
    {
        var result = new StringBuilder();
        var blank = 0;
        foreach (var line in text.Replace("\r\n", "\n").Split('\n').Select(l => l.TrimEnd()))
        {
            if (line.Length == 0)
            {
                blank++;
                if (blank > 1)
                {
                    continue;
                }
            }
            else
            {
                blank = 0;
            }

            result.Append(line).Append('\n');
        }

        return result.ToString().Trim('\n') + "\n";
    }
}
