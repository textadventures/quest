using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace QuestViva.Cli;

// `qv preview`'s web server: WasmPlayer and a game's folder, on loopback only.
//
//   /             the player (PlayerBundle), opened as /?source=qv
//   /qv/game      which file to play, and the libraries beside it the game could include
//   /qv/events    server-sent events: "reload" when a file in the game's folder changes
//   /game/<path>  the game's folder, which the player uses as its resourceRoot
//
// wasm-player.js's source=qv boot branch is the other half. Hidden files and anything outside the
// game's folder aren't served.
internal sealed class PreviewServer : IAsyncDisposable
{
    private static readonly TimeSpan ReloadDelay = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(20);

    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=utf-8",
        [".htm"] = "text/html; charset=utf-8",
        [".js"] = "text/javascript; charset=utf-8",
        [".mjs"] = "text/javascript; charset=utf-8",
        [".css"] = "text/css; charset=utf-8",
        [".json"] = "application/json",
        [".wasm"] = "application/wasm",
        [".aslx"] = "application/xml",
        [".xml"] = "application/xml",
        [".txt"] = "text/plain; charset=utf-8",
        [".svg"] = "image/svg+xml",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".ico"] = "image/x-icon",
        [".mp3"] = "audio/mpeg",
        [".wav"] = "audio/wav",
        [".ogg"] = "audio/ogg",
        [".m4a"] = "audio/mp4",
        [".mp4"] = "video/mp4",
        [".webm"] = "video/webm",
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".ttf"] = "font/ttf",
        [".otf"] = "font/otf"
    };

    private readonly PlayerBundle _player;
    private readonly HttpListener _listener;
    private readonly int? _seed;
    private readonly CancellationTokenSource _stopping = new();
    private readonly List<HttpListenerResponse> _eventClients = [];
    private readonly Lock _lock = new();
    private readonly Task _acceptLoop;
    private FileInfo _game;
    private FileSystemWatcher? _watcher;
    private Timer? _reloadTimer;

    private PreviewServer(PlayerBundle player, FileInfo game, HttpListener listener, int port, int? seed)
    {
        _player = player;
        _game = game;
        _listener = listener;
        _seed = seed;
        Port = port;
        Watch();
        _acceptLoop = AcceptAsync();
        _ = KeepAliveAsync();
    }

    public int Port { get; }

    public FileInfo Game
    {
        get
        {
            lock (_lock)
            {
                return _game;
            }
        }
    }

    public string Url => $"http://127.0.0.1:{Port}/?source=qv" + (_seed is { } seed ? $"&seed={seed}" : "");

    // Starts serving game on port, or with port 0 on a free one.
    public static PreviewServer Start(PlayerBundle player, FileInfo game, int port = 0, int? seed = null)
    {
        if (!game.Exists)
        {
            throw new FileNotFoundException($"File not found: {game.FullName}");
        }

        if (port == 0)
        {
            // HttpListener can't pick a port itself, so ask the OS for a free one.
            using var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
        }

        var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        try
        {
            listener.Start();
        }
        catch (HttpListenerException ex)
        {
            throw new IOException($"Couldn't serve on port {port}: {ex.Message}", ex);
        }

        return new PreviewServer(player, game, listener, port, seed);
    }

    // Switches to another game. Open pages reload to play it.
    public void SetGame(FileInfo game)
    {
        lock (_lock)
        {
            if (string.Equals(game.FullName, _game.FullName, StringComparison.Ordinal))
            {
                return;
            }

            _game = game;
        }

        Watch();
        SendReload();
    }

    public async ValueTask DisposeAsync()
    {
        await _stopping.CancelAsync();
        _watcher?.Dispose();
        _reloadTimer?.Dispose();
        lock (_lock)
        {
            foreach (var client in _eventClients)
            {
                client.Abort();
            }

            _eventClients.Clear();
        }

        _listener.Stop();
        _listener.Close();
        try
        {
            await _acceptLoop;
        }
        catch (Exception ex) when (ex is ObjectDisposedException or HttpListenerException or OperationCanceledException)
        {
        }
    }

    private void Watch()
    {
        _watcher?.Dispose();
        var game = Game;
        var watcher = new FileSystemWatcher(game.DirectoryName!)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite |
                           NotifyFilters.Size
        };
        watcher.Changed += OnFileChanged;
        watcher.Created += OnFileChanged;
        watcher.Deleted += OnFileChanged;
        watcher.Renamed += OnFileChanged;
        watcher.EnableRaisingEvents = true;
        _watcher = watcher;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var name = Path.GetFileName(e.FullPath);
        var relative = e.Name ?? name;
        // Hidden files (.git, editor swap files), temporary files, and published .quest output -
        // qv publish writes one beside the game - don't change the game being previewed.
        if (relative.Split('/', '\\').Any(s => s.StartsWith('.')) ||
            name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith('~') ||
            (name.EndsWith(".quest", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(name, Game.Name, StringComparison.Ordinal)))
        {
            return;
        }

        // An editor saving a file raises several events; reload once they've settled.
        lock (_lock)
        {
            _reloadTimer?.Dispose();
            _reloadTimer = new Timer(_ => SendReload(), null, ReloadDelay, Timeout.InfiniteTimeSpan);
        }
    }

    private void SendReload() => Broadcast("event: reload\ndata: \n\n");

    private void Broadcast(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        List<HttpListenerResponse> clients;
        lock (_lock)
        {
            clients = [.. _eventClients];
        }

        foreach (var client in clients)
        {
            try
            {
                client.OutputStream.Write(bytes);
                client.OutputStream.Flush();
            }
            catch (Exception ex) when (ex is IOException or HttpListenerException or ObjectDisposedException
                                           or InvalidOperationException)
            {
                // The page went away.
                lock (_lock)
                {
                    _eventClients.Remove(client);
                }
            }
        }
    }

    // A comment line now and then, so a page that has gone is noticed and dropped.
    private async Task KeepAliveAsync()
    {
        try
        {
            while (!_stopping.IsCancellationRequested)
            {
                await Task.Delay(KeepAliveInterval, _stopping.Token);
                Broadcast(": keep-alive\n\n");
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task AcceptAsync()
    {
        while (!_stopping.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception ex) when (ex is ObjectDisposedException or HttpListenerException or InvalidOperationException)
            {
                return;
            }

            _ = Task.Run(() => Handle(context));
        }
    }

    private void Handle(HttpListenerContext context)
    {
        var response = context.Response;
        try
        {
            var path = context.Request.Url?.AbsolutePath ?? "/";
            if (context.Request.HttpMethod is not ("GET" or "HEAD"))
            {
                Send(response, 405, "text/plain", "Only GET is supported."u8.ToArray());
            }
            else if (path == "/qv/events")
            {
                response.StatusCode = 200;
                response.ContentType = "text/event-stream";
                response.Headers["Cache-Control"] = "no-store";
                response.SendChunked = true;
                response.OutputStream.Write(": connected\n\n"u8);
                response.OutputStream.Flush();
                lock (_lock)
                {
                    _eventClients.Add(response);
                }

                // Left open: Broadcast writes to it until the page goes away.
                return;
            }
            else if (path == "/qv/game")
            {
                Send(response, 200, "application/json", JsonSerializer.SerializeToUtf8Bytes(GameInfo()));
            }
            else if (path.StartsWith("/game/", StringComparison.Ordinal))
            {
                var bytes = ReadGameFile(Uri.UnescapeDataString(path["/game/".Length..]));
                if (bytes == null)
                {
                    Send(response, 404, "text/plain", "Not found."u8.ToArray());
                }
                else
                {
                    Send(response, 200, ContentType(path), bytes);
                }
            }
            else
            {
                var relative = path == "/" ? "index.html" : Uri.UnescapeDataString(path.TrimStart('/'));
                var bytes = _player.Read(relative);
                if (bytes == null)
                {
                    Send(response, 404, "text/plain", "Not found."u8.ToArray());
                }
                else
                {
                    Send(response, 200, ContentType(relative), bytes);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or HttpListenerException or ObjectDisposedException
                                       or InvalidOperationException)
        {
            // The page went away mid-response.
        }

        try
        {
            response.Close();
        }
        catch (Exception ex) when (ex is IOException or HttpListenerException or ObjectDisposedException
                                       or InvalidOperationException)
        {
        }
    }

    private object GameInfo()
    {
        var game = Game;
        // Every .aslx or .xml beside the game is a library it could include, as in the Editor.
        var libraries = game.Directory!.EnumerateFiles()
            .Where(f => f.Name != game.Name && !f.Name.StartsWith('.') &&
                        (f.Extension.Equals(".aslx", StringComparison.OrdinalIgnoreCase) ||
                         f.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase)))
            .Select(f => f.Name)
            .Order(StringComparer.Ordinal)
            .ToList();
        return new { file = game.Name, libraries };
    }

    private byte[]? ReadGameFile(string relative)
    {
        var folder = Path.GetFullPath(Game.DirectoryName!) + Path.DirectorySeparatorChar;
        if (relative.Split('/', '\\').Any(s => s.Length == 0 || s.StartsWith('.')))
        {
            return null;
        }

        var full = Path.GetFullPath(Path.Combine(folder, relative));
        return full.StartsWith(folder, StringComparison.Ordinal) && File.Exists(full) ? File.ReadAllBytes(full) : null;
    }

    private static string ContentType(string path) =>
        ContentTypes.GetValueOrDefault(Path.GetExtension(path), "application/octet-stream");

    private static void Send(HttpListenerResponse response, int status, string contentType, byte[] body)
    {
        response.StatusCode = status;
        response.ContentType = contentType;
        // Everything is local and may change between reloads.
        response.Headers["Cache-Control"] = "no-store";
        response.ContentLength64 = body.Length;
        response.OutputStream.Write(body);
    }
}
