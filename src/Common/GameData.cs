namespace QuestViva.Common;

// A game file's contents, plus whatever it needs from alongside it. Everything that loads a
// game (WasmPlayer, WasmEditor, GameQuery) already has the bytes in hand, so nothing here
// touches the filesystem. filename needn't exist on disk: its extension picks the loader, and
// it names the game.
//
// adjacentFiles supplies files the game refers to by name but which aren't inside the game
// file itself - the custom libraries an <include ref="..."/> pulls in (see
// WorldModel.GetLibraryStream), or a Quest 4 game's library and resource files. The host keeps
// those as separate assets in its own storage, so it has to hand them over explicitly.
public class GameData(byte[] data, string filename, IReadOnlyDictionary<string, byte[]>? adjacentFiles = null)
{
    public Stream Data { get; } = new MemoryStream(data);
    public string GameId => Path.GetFileName(filename);
    public string Filename => filename;

    public Stream? GetAdjacentFile(string file)
    {
        return adjacentFiles != null && adjacentFiles.TryGetValue(file, out var bytes)
            ? new MemoryStream(bytes)
            : null;
    }
}
