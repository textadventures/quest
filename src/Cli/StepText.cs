using QuestViva.PlayerCore;

namespace QuestViva.Cli;

internal static class StepText
{
    // A step's text as one normalised string, without the game's echo of the command: whoever sent
    // the command already has it.
    public static string Format(HeadlessStep step, string? input)
    {
        var text = step.Output.Where(o => o.Kind == HeadlessOutputKind.Text).Select(o => o.Text).ToList();

        var first = text.FindIndex(t => t.Trim().Length > 0);
        if (input != null && first >= 0 && text[first].Trim() == "> " + input.Trim())
        {
            text.RemoveRange(0, first + 1);
        }

        var normalised = HeadlessSession.Normalise(string.Concat(text.Select(t => t.EndsWith('\n') ? t : t + "\n")));
        return normalised.Trim().Length == 0 ? "" : normalised;
    }

    // Errors in a game's scripts are already printed in its text as "Error running script: ...".
    public static IEnumerable<string> UnprintedErrors(HeadlessStep step, string text) =>
        step.Errors.Where(e => !text.Contains("Error running script: " + e));
}
