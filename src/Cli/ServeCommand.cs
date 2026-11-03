using System.Text.Encodings.Web;
using System.Text.Json;

namespace QuestViva.Cli;

// `qv serve`: the JSON-lines protocol over stdin/stdout, for programs driving a game. Nothing but
// protocol responses is written to stdout.
internal static class ServeCommand
{
    // Game text is full of apostrophes and quotes; the default encoder's HTML-safe escaping of them
    // is pointless on a pipe.
    private static readonly JsonSerializerOptions JsonOptions =
        new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static async Task<int> RunAsync()
    {
        var protocol = new ServeProtocol();
        var stdout = Console.Out;
        while (await Console.In.ReadLineAsync() is { } line)
        {
            if (line.Trim().Length == 0)
            {
                continue;
            }

            var response = await protocol.HandleAsync(line);
            await stdout.WriteLineAsync(response.ToJsonString(JsonOptions));
            await stdout.FlushAsync();
        }

        return 0;
    }
}
