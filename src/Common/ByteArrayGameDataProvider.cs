namespace QuestViva.Common;

// adjacentFiles lets callers with no filesystem access (e.g. WasmEditorBridge, which runs
// in-browser) supply the bytes of files an Included Library's <include ref="..."/> needs to
// resolve at load time (see WorldModel.GetLibraryStream) — those files live as separate
// "assets" in the host's own storage, not inside gameFileBytes itself, so they must be handed
// over explicitly rather than discovered via a real filesystem lookup.
public class ByteArrayGameDataProvider(
    byte[] data,
    string filename,
    IReadOnlyDictionary<string, byte[]>? adjacentFiles = null) : IGameDataProvider
{
    public Task<GameData?> GetData()
    {
        var stream = new MemoryStream(data);
        var gameId = Path.GetFileName(filename);
        return Task.FromResult<GameData?>(new GameData(stream, gameId, filename, this));
    }

    public Stream? GetAdjacentFile(string file)
    {
        return adjacentFiles != null && adjacentFiles.TryGetValue(file, out var bytes)
            ? new MemoryStream(bytes)
            : null;
    }
}