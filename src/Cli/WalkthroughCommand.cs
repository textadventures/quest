using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// `qv walkthrough`: runs a game's built-in walkthroughs, each from a fresh start, and exits non-zero
// if any fails, so authors can run them in CI.
internal static class WalkthroughCommand
{
    public static async Task<int> RunAsync(FileInfo game, string? name, int? seed, bool quiet)
    {
        if (!game.Exists)
        {
            await Console.Error.WriteLineAsync($"File not found: {game.FullName}");
            return 1;
        }

        // A fixed seed by default, so a walkthrough that passes once passes every time, and in CI.
        var seedValue = seed ?? HeadlessSession.DefaultSeed;
        HeadlessSession session;
        try
        {
            session = await HeadlessSession.StartAsync(GameFiles.Load(game), seedValue);
        }
        catch (InvalidOperationException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return 1;
        }

        var names = HeadlessWalkthrough.Names(session);
        if (name != null && !names.Contains(name))
        {
            await Console.Error.WriteLineAsync(names.Count == 0
                ? "The game has no walkthroughs."
                : $"The game has no walkthrough called '{name}'. Its walkthroughs: {string.Join(", ", names)}");
            return 1;
        }

        if (names.Count == 0)
        {
            await Console.Error.WriteLineAsync("The game has no walkthroughs.");
            return 1;
        }

        var failed = 0;
        var first = true;
        foreach (var walkthrough in name != null ? [name] : names)
        {
            if (!first)
            {
                session = await HeadlessSession.StartAsync(GameFiles.Load(game), seedValue);
            }

            if (!quiet)
            {
                if (!first)
                {
                    Console.WriteLine();
                }

                Console.WriteLine($"=== {walkthrough} ===");
                Console.WriteLine();
                Console.Write(StepText.Format(session.Opening, null));
            }

            first = false;
            var result = await HeadlessWalkthrough.RunAsync(session, walkthrough, quiet ? null : WriteStep);

            if (!quiet)
            {
                Console.WriteLine();
            }

            Console.WriteLine(Summary(result));
            if (!result.Passed)
            {
                failed++;
            }
        }

        return failed == 0 ? 0 : 1;
    }

    public static string Summary(WalkthroughResult result) => result.Passed
        ? $"PASS {result.Name} ({result.StepCount} steps)"
        : result.FailedStep is { } step
            ? $"FAIL {result.Name} at step {step} of {result.StepCount}: {result.Failure}"
            : $"FAIL {result.Name}: {result.Failure}";

    private static void WriteStep(WalkthroughStep step)
    {
        Console.WriteLine(StepLine(step));
        if (step.Response != null)
        {
            Console.Write(StepText.Format(step.Response, step.Step));
        }
    }

    // How a step is shown in a transcript: a command as the player typed it, anything else as the
    // walkthrough wrote it, in brackets.
    public static string StepLine(WalkthroughStep step) => step.AssertPassed switch
    {
        true => $"[{step.Step}: passed]",
        false => $"[{step.Step}: FAILED]",
        _ when IsCommand(step.Step) => $"> {step.Step}",
        _ => $"[{step.Step}]"
    };

    private static bool IsCommand(string step) =>
        !new[] { "menu:", "answer:", "assert:", "label:", "delay:", "event:", "runtime:" }
            .Any(prefix => step.StartsWith(prefix, StringComparison.Ordinal));
}
