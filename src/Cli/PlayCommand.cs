using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// `qv play`: an interactive terminal player.
internal static class PlayCommand
{
    public static async Task<int> RunAsync(FileInfo game, int? seed)
    {
        if (!game.Exists)
        {
            await Console.Error.WriteLineAsync($"File not found: {game.FullName}");
            return 1;
        }

        HeadlessSession session;
        try
        {
            session = await HeadlessSession.StartAsync(GameFiles.Load(game), seed ?? Random.Shared.Next());
        }
        catch (NotSupportedException)
        {
            await Console.Error.WriteLineAsync("qv only plays Quest 5 and later games.");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return 1;
        }

        Write(session.Opening, null);

        while (!session.Finished)
        {
            WritePrompt(session);
            Console.Write("> ");
            var input = Console.ReadLine();
            if (input == null)
            {
                break;
            }

            if (input.Trim().Length == 0)
            {
                continue;
            }

            var step = await session.SendAsync(input);
            if (step.PendingMenu != null && step.Output.Count == 0)
            {
                Console.WriteLine("Choose an option by its number.");
                Console.WriteLine();
                continue;
            }

            Write(step, input);
        }

        if (session.Finished)
        {
            Console.WriteLine("The game has ended.");
        }

        return 0;
    }

    // v600+ games draw menus and questions in their own text. Older ones expect a dialog, so
    // stand in for it.
    private static void WritePrompt(HeadlessSession session)
    {
        if (session.PendingMenu is { Inline: false } menu)
        {
            Console.WriteLine(menu.Caption);
            var n = 1;
            foreach (var option in menu.Options.Values)
            {
                Console.WriteLine($"{n++}: {option}");
            }
        }
        else if (session.PendingQuestion is { } question && !session.PendingQuestionInline)
        {
            Console.WriteLine($"{question} (yes/no)");
        }
    }

    private static void Write(HeadlessStep step, string? input)
    {
        var text = step.Output.Where(o => o.Kind == HeadlessOutputKind.Text).Select(o => o.Text).ToList();

        // The game echoes the command, which the terminal already shows.
        var first = text.FindIndex(t => t.Trim().Length > 0);
        if (input != null && first >= 0 && text[first].Trim() == "> " + input.Trim())
        {
            text.RemoveRange(0, first + 1);
        }

        var joined = string.Concat(text.Select(t => t.EndsWith('\n') ? t : t + "\n"));
        Console.Write(HeadlessSession.Normalise(joined));
        Console.WriteLine();

        // Errors in a game's scripts are already printed in its text; anything else goes to stderr.
        foreach (var error in step.Errors.Where(e => !joined.Contains("Error running script: " + e)))
        {
            Console.Error.WriteLine($"[error] {error}");
        }
    }
}
