using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuestViva.Common;

namespace QuestViva.Cli;

// `qv mcp`: an MCP server over stdio whose tools play a game, for AI agents.
internal static class McpCommand
{
    public static async Task<int> RunAsync(FileInfo? game, int? seed, bool textOnly, bool debugTools)
    {
        if (game is { Exists: false })
        {
            await Console.Error.WriteLineAsync($"File not found: {game.FullName}");
            return 1;
        }

        var builder = Host.CreateApplicationBuilder();
        // stdout carries the protocol, so logs go to stderr.
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.Services.AddSingleton(new McpGame(game, seed, textOnly));
        var mcp = builder.Services
            .AddMcpServer(o => o.ServerInfo = new() { Name = "qv", Version = VersionInfo.Version })
            .WithStdioServerTransport()
            .WithTools<PlayTools>();
        if (game != null)
        {
            mcp.WithTools<FixedGameTools>();
        }
        else
        {
            mcp.WithTools<LoadGameTools>();
        }

        if (debugTools)
        {
            mcp.WithTools<DebugTools>();
        }

        await builder.Build().RunAsync();
        return 0;
    }
}
