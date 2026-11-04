using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// The JSON-lines protocol behind `qv serve`: one request object in, one response object out. A
// process plays one game at a time; "start" begins it, or begins it again. The protocol is
// documented in docs/qv-serve.md.
internal sealed class ServeProtocol
{
    private HeadlessSession? _session;

    public async Task<JsonObject> HandleAsync(string line)
    {
        JsonNode? id = null;
        try
        {
            var request = JsonNode.Parse(line) as JsonObject
                          ?? throw new ProtocolException("A request must be a JSON object.");
            id = request["id"]?.DeepClone();
            var response = await HandleAsync(request);
            return WithId(response, id);
        }
        catch (Exception ex) when (ex is ProtocolException or JsonException or InvalidOperationException
                                       or FileNotFoundException or NotSupportedException)
        {
            return WithId(new JsonObject { ["ok"] = false, ["error"] = ex.Message }, id);
        }
    }

    private async Task<JsonObject> HandleAsync(JsonObject request)
    {
        var op = GetString(request, "op");
        switch (op)
        {
            case "start":
            {
                var game = new FileInfo(GetString(request, "game"));
                var seed = request["seed"] == null ? HeadlessSession.DefaultSeed : GetInt(request, "seed");
                _session = await HeadlessSession.StartAsync(GameFiles.Load(game), seed);
                return StepResponse(_session.Opening, null);
            }
            case "send":
            {
                var input = GetString(request, "input");
                return StepResponse(await Session.SendAsync(input), input);
            }
            case "tick":
                return StepResponse(await Session.TickAsync(GetInt(request, "seconds")), null);
            case "event":
                return StepResponse(
                    await Session.SendEventAsync(GetString(request, "name"), request["param"]?.GetValue<string>() ?? ""),
                    null);
            case "evaluate":
            {
                var evaluation = await Session.EvaluateWithOutputAsync(GetString(request, "expression"));
                return new JsonObject
                {
                    ["ok"] = true,
                    ["value"] = ToJson(evaluation.Value),
                    ["text"] = evaluation.Text,
                    ["errors"] = Strings(evaluation.Errors)
                };
            }
            case "assert":
                return new JsonObject
                {
                    ["ok"] = true,
                    ["result"] = await Session.AssertAsync(GetString(request, "expression"))
                };
            default:
                throw new ProtocolException($"Unknown op '{op}'.");
        }
    }

    private HeadlessSession Session => _session ?? throw new ProtocolException("No game has been started.");

    private static JsonObject StepResponse(HeadlessStep step, string? input)
    {
        var text = StepText.Format(step, input);
        var response = new JsonObject
        {
            ["ok"] = true,
            ["text"] = text,
            ["errors"] = Strings(step.Errors),
            ["menu"] = null,
            ["question"] = null,
            ["finished"] = step.Finished,
            ["exits"] = Strings(step.Exits),
            ["objects"] = Strings(step.Objects),
            ["inventory"] = Strings(step.Inventory),
            ["status"] = Strings(step.Status),
            ["panesVisible"] = step.PanesVisible,
            ["compassVisible"] = step.CompassVisible
        };

        if (step.PendingMenu is { } menu)
        {
            response["menu"] = new JsonObject
            {
                ["caption"] = menu.Caption,
                ["options"] = new JsonArray(menu.Options
                    .Select(o => (JsonNode?) new JsonObject { ["key"] = o.Key, ["text"] = o.Value })
                    .ToArray()),
                ["allowCancel"] = menu.AllowCancel,
                ["inline"] = menu.Inline
            };
        }

        if (step.PendingQuestion is { } question)
        {
            response["question"] = new JsonObject
            {
                ["caption"] = question,
                ["inline"] = step.PendingQuestionInline
            };
        }

        return response;
    }

    private static JsonArray Strings(IEnumerable<string> values) => [.. values.Select(v => (JsonNode?) v)];

    private static JsonNode? ToJson(object? value) => value switch
    {
        null => null,
        string s => s,
        bool b => b,
        int i => i,
        double d => d,
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)
    };

    private static JsonObject WithId(JsonObject response, JsonNode? id)
    {
        if (id == null)
        {
            return response;
        }

        // Put the id first, so it's easy to spot when reading the stream.
        var withId = new JsonObject { ["id"] = id };
        foreach (var (key, value) in response.ToList())
        {
            response.Remove(key);
            withId[key] = value;
        }

        return withId;
    }

    private static string GetString(JsonObject request, string name) =>
        request[name] is JsonValue value && value.TryGetValue<string>(out var s)
            ? s
            : throw new ProtocolException($"'{name}' must be a string.");

    private static int GetInt(JsonObject request, string name) =>
        request[name] is JsonValue value && value.TryGetValue<int>(out var i)
            ? i
            : throw new ProtocolException($"'{name}' must be an integer.");

    private sealed class ProtocolException(string message) : Exception(message);
}
