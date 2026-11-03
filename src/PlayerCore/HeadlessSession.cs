using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using QuestViva.Common;
using QuestViva.Engine;

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

    public IEnumerable<string> Errors =>
        Output.Where(o => o.Kind == HeadlessOutputKind.Error).Select(o => o.Text);

    // The text output as one normalised string, one chunk per line, errors left out.
    public string Text => HeadlessSession.Normalise(string.Concat(
        Output.Where(o => o.Kind == HeadlessOutputKind.Text).Select(o => o.Text.EndsWith('\n') ? o.Text : o.Text + "\n")));
}

// Plays a v5+ game with no UI: a command goes in, and that turn's plain text comes back, along with
// any menu or question the game is now waiting on. Waits, pauses and synchronous sounds are
// advanced automatically, and SetTimeout timers that come due are drained after every step, so a
// step returns once the game is waiting for the player again.
//
// SendAsync, AnswerMenuAsync and AnswerQuestionAsync are what a player could do. Tick, SendEvent,
// Assert and Evaluate reach past that - an agent playing the game should not be given them.
public sealed class HeadlessSession
{
    public const int DefaultSeed = 1234;

    private static readonly string[] LegacyExtensions = [".asl", ".cas", ".qsg"];

    private readonly WorldModel _world;
    private readonly HeadlessPlayer _player;
    private List<HeadlessOutput> _output = [];
    private int _pendingTick;

    private HeadlessSession(GameData gameData, int seed)
    {
        _world = new WorldModel(gameData, null);
        _world.SetRandomSeed(seed);
        _world.LogError += ex => Write(new HeadlessOutput(HeadlessOutputKind.Error, ex.Message));
        _world.PrintText += WriteHtml; // output path for ASL < v540
        _world.RequestNextTimerTick += seconds => _pendingTick = seconds;
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
    public GameState State => _world.State;
    public bool Finished => State == GameState.Finished;

    // The underlying game, for privileged inspection such as IGameDebug.GetObjects.
    public IGameDebug Game => _world;

    public static async Task<HeadlessSession> StartAsync(GameData gameData, int seed = DefaultSeed,
        Action<HeadlessOutput>? onOutput = null)
    {
        if (LegacyExtensions.Contains(Path.GetExtension(gameData.Filename).ToLowerInvariant()))
        {
            throw new NotSupportedException("Headless sessions only support Quest 5 and later games.");
        }

        var session = new HeadlessSession(gameData, seed);
        if (onOutput != null)
        {
            session.OutputWritten += onOutput;
        }

        if (!await session._world.Initialise(session._player))
        {
            throw new InvalidOperationException(
                $"Game '{gameData.GameId}' failed to load: {string.Join("; ", session._world.Errors)}");
        }

        session.Opening = await session.RunStepAsync(() => session._world.Begin());
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
        RunStepAsync(() => _world.SendCommand(command));

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
        return RunStepAsync(() => _world.SetMenuResponse(key));
    }

    public Task<HeadlessStep> AnswerQuestionAsync(bool answer)
    {
        if (PendingQuestion == null)
        {
            throw new InvalidOperationException("No question is pending.");
        }

        _player.PendingQuestion = null;
        return RunStepAsync(() => _world.SetQuestionResponse(answer));
    }

    // Advances the game clock, firing any timers that come due - the stand-in for a browser
    // player's one-second interval, for games gated on real-time timers. SetTimeout timers are
    // already drained after every step.
    public Task<HeadlessStep> TickAsync(int seconds) =>
        RunStepAsync(() =>
        {
            _pendingTick = 0;
            return _world.Tick(seconds);
        });

    // Fires an event a game's own JavaScript would normally send with ASLEvent.
    public Task<HeadlessStep> SendEventAsync(string eventName, string param) =>
        RunStepAsync(() => _world.SendEvent(eventName, param));

    public Task<bool> AssertAsync(string expression) => _world.AssertAsync(expression);

    public Task<object?> EvaluateAsync(string expression) => _world.EvaluateAsync(expression);

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

    private HeadlessStep CurrentStep(IReadOnlyList<HeadlessOutput> output) =>
        new()
        {
            Output = output,
            PendingMenu = PendingMenu,
            PendingQuestion = PendingQuestion,
            PendingQuestionInline = PendingQuestionInline,
            Finished = Finished
        };

    private async Task AutoAdvanceAsync()
    {
        while (!Finished && (_player.IsWaiting || _player.IsPausing))
        {
            if (_player.IsWaiting)
            {
                _player.IsWaiting = false;
                await _world.FinishWait();
            }

            if (_player.IsPausing)
            {
                _player.IsPausing = false;
                await _world.FinishPause();
            }
        }
    }

    private bool HasPendingTimeout() =>
        _world.Elements.GetElements(ElementType.Timer)
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
            await _world.Tick(seconds > 0 ? seconds : 1);
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
