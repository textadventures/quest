namespace QuestViva.Cli;

// `qv preview`: serves a game in WasmPlayer on loopback until Ctrl+C, reloading open pages when a
// file in the game's folder changes.
internal static class PreviewCommand
{
    public static async Task<int> RunAsync(FileInfo game, int port, int? seed, DirectoryInfo? playerFolder)
    {
        PlayerBundle player;
        PreviewServer server;
        try
        {
            player = FindPlayer(playerFolder);
            server = PreviewServer.Start(player, game, port, seed);
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException or IOException)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return 2;
        }

        await using (server)
        {
            Console.WriteLine($"Previewing {game.FullName}");
            Console.WriteLine($"Open {server.Url}");
            Console.WriteLine("The page reloads when a file in the game's folder changes. Press Ctrl+C to stop.");

            var stopped = new TaskCompletionSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                stopped.TrySetResult();
            };
            await stopped.Task;
        }

        return 0;
    }

    // The player to serve: the one given with --player, the one built into qv, or, for a qv built
    // from a checkout without one, that checkout's WasmPlayer build.
    public static PlayerBundle FindPlayer(DirectoryInfo? folder)
    {
        if (folder != null)
        {
            if (!File.Exists(Path.Combine(folder.FullName, "index.html")))
            {
                throw new InvalidOperationException(
                    $"{folder.FullName} isn't a WasmPlayer bundle: it has no index.html.");
            }

            return PlayerBundle.FromFolder(folder);
        }

        return PlayerBundle.Embedded() ?? PlayerBundle.FromCheckout() ?? throw new InvalidOperationException(
            "This build of qv has no player built in. Build src/WasmPlayer (dotnet build " +
            "src/WasmPlayer/WasmPlayer.csproj) or pass --player with the folder of a WasmPlayer bundle.");
    }
}
