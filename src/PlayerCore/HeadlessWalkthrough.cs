namespace QuestViva.PlayerCore;

// One walkthrough step as it ran. Response is what the game printed, and is null for a step that
// doesn't run a turn (assert:, label:, delay:, runtime:). AssertPassed is set for assert: steps.
public sealed record WalkthroughStep(int Number, string Step, HeadlessStep? Response, bool? AssertPassed);

// How a walkthrough went. When it failed, FailedStep is the 1-based number of the step it failed
// at (null if it failed before any step, or because the game ended early), and Failure says why.
public sealed record WalkthroughResult(string Name, int StepCount, int StepsRun, int? FailedStep, string? Failure)
{
    public bool Passed => Failure == null;
}

// Runs one of a game's built-in walkthroughs in a HeadlessSession, with the step syntax the
// player's WalkthroughRunner uses: a command, menu:<key> or answer:yes/no while a menu or question
// is pending, assert:<expression>, event:<name>;<param>, delay:<ms>, and label:/runtime:, which
// are ignored.
//
// Unlike the player's runner, which stops quietly at a failed assert and only shows script errors
// in the transcript, this is meant for CI and agents, so anything that means the walkthrough no
// longer does what its author wrote fails it: a failed or broken assert, a script error, a menu
// answer that isn't an option, a step that doesn't match what the game is waiting for, or the
// game ending before the last step.
//
// delay: is real time in the player, during which the game's timers keep running. Here it
// advances the game clock instead, after every step from then on as the player's delay applies,
// a whole second at a time as the player's timer does.
public static class HeadlessWalkthrough
{
    public static IReadOnlyList<string> Names(HeadlessSession session) =>
        session.Game.Walkthroughs.Walkthroughs.Keys.ToList();

    public static async Task<WalkthroughResult> RunAsync(HeadlessSession session, string name,
        Action<WalkthroughStep>? onStep = null)
    {
        if (!session.Game.Walkthroughs.Walkthroughs.TryGetValue(name, out var walkthrough))
        {
            throw new ArgumentException($"The game has no walkthrough called '{name}'.", nameof(name));
        }

        var steps = walkthrough.Steps.Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        var delay = 0;
        var elapsed = 0;

        for (var i = 0; i < steps.Count; i++)
        {
            var number = i + 1;
            var step = steps[i];
            WalkthroughResult Fail(string reason) => new(name, steps.Count, i, number, reason);

            if (session.Finished)
            {
                return new WalkthroughResult(name, steps.Count, i, null,
                    $"The game ended after step {i}, with {steps.Count - i} {(steps.Count - i == 1 ? "step" : "steps")} left.");
            }

            HeadlessStep? response = null;
            bool? assertPassed = null;

            if (session.PendingMenu is { } menu)
            {
                if (!step.StartsWith("menu:", StringComparison.Ordinal))
                {
                    return Fail($"A menu (\"{menu.Caption}\") is waiting for an answer, but the step isn't menu:<key>.");
                }

                var key = step[5..];
                if (!menu.Options.ContainsKey(key))
                {
                    return Fail($"'{key}' isn't one of the menu's options ({string.Join(", ", menu.Options.Keys)}).");
                }

                response = await session.AnswerMenuAsync(key);
            }
            else if (session.PendingQuestion is { } question)
            {
                var answer = step.StartsWith("answer:", StringComparison.Ordinal) ? step[7..] : null;
                if (answer is not ("yes" or "no"))
                {
                    return Fail($"A question (\"{question}\") is waiting for an answer, but the step isn't answer:yes or answer:no.");
                }

                response = await session.AnswerQuestionAsync(answer == "yes");
            }
            else if (step.StartsWith("menu:", StringComparison.Ordinal) || step.StartsWith("answer:", StringComparison.Ordinal))
            {
                return Fail("No menu or question is waiting for an answer.");
            }
            else if (step.StartsWith("assert:", StringComparison.Ordinal))
            {
                try
                {
                    assertPassed = await session.AssertAsync(step[7..]);
                }
                catch (Exception ex)
                {
                    onStep?.Invoke(new WalkthroughStep(number, step, null, false));
                    return Fail($"The assert couldn't be evaluated: {ex.Message}");
                }
            }
            else if (step.StartsWith("label:", StringComparison.Ordinal) || step.StartsWith("runtime:", StringComparison.Ordinal))
            {
                // Labels are notes for the reader, and runtime: prints the elapsed real time.
            }
            else if (step.StartsWith("delay:", StringComparison.Ordinal))
            {
                if (!int.TryParse(step[6..].Trim(), out delay) || delay < 0)
                {
                    return Fail("A delay must be a whole number of milliseconds.");
                }
            }
            else if (step.StartsWith("event:", StringComparison.Ordinal))
            {
                var parts = step[6..].Split(';', 2);
                response = await session.SendEventAsync(parts[0], parts.Length > 1 ? parts[1] : "");
            }
            else
            {
                response = await session.SendCommandAsync(step);
            }

            if (delay > 0 && !session.Finished)
            {
                elapsed += delay;
                if (elapsed >= 1000)
                {
                    var tick = await session.TickAsync(elapsed / 1000);
                    elapsed %= 1000;
                    response = response == null ? tick : Combine(response, tick);
                }
            }

            onStep?.Invoke(new WalkthroughStep(number, step, response, assertPassed));

            if (assertPassed == false)
            {
                return Fail($"Assert failed: {step[7..]}");
            }

            if (response?.Errors.FirstOrDefault() is { } error)
            {
                return Fail($"Script error: {error}");
            }
        }

        return new WalkthroughResult(name, steps.Count, steps.Count, null, null);
    }

    // A step's response followed by the timer output its delay produced, with what the game is
    // waiting for taken from the later one.
    private static HeadlessStep Combine(HeadlessStep first, HeadlessStep then) => new()
    {
        Output = [..first.Output, ..then.Output],
        PendingMenu = then.PendingMenu,
        PendingQuestion = then.PendingQuestion,
        PendingQuestionInline = then.PendingQuestionInline,
        Finished = then.Finished,
        Exits = then.Exits,
        Objects = then.Objects,
        Inventory = then.Inventory,
        Status = then.Status,
        PanesVisible = then.PanesVisible,
        CompassVisible = then.CompassVisible
    };
}
