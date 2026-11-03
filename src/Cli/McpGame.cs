using System.Text;
using ModelContextProtocol;
using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// The game behind `qv mcp`, shared by its tools. Tool calls can arrive concurrently and a
// HeadlessSession handles one step at a time, so calls are serialised here.
internal sealed class McpGame(FileInfo game, int seed)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private HeadlessSession? _session;

    public Task<string> StartAsync() => RunAsync(async () =>
    {
        _session = await HeadlessSession.StartAsync(GameFiles.Load(game), seed);
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
        catch (NotSupportedException)
        {
            throw new McpException("qv only plays Quest 5 and later games.");
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
    private static string Describe(HeadlessStep step, string? input)
    {
        var text = StepText.Format(step, input);
        var result = new StringBuilder(text);

        foreach (var error in StepText.UnprintedErrors(step, text))
        {
            result.Append("[error] ").Append(error).Append('\n');
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
