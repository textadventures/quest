namespace QuestViva.Common;

public class ByteArrayGameDataProvider(byte[] data, string filename) : IGameDataProvider
{
    public Task<GameData?> GetData()
    {
        var stream = new MemoryStream(data);
        var gameId = Path.GetFileName(filename);
        return Task.FromResult<GameData?>(new GameData(stream, gameId, filename, this));
    }
}