namespace QuestViva.Common;

public class GameData(Stream data, string gameId, string filename, IGameDataProvider provider)
{
    public Stream Data => data;
    public string GameId => gameId;
    public string Filename => filename;

    public bool IsCompiled { get; init; }
    public string? ResourceRoot { get; init; }

    public Stream? GetAdjacentFile(string file)
    {
        return provider.GetAdjacentFile(file);
    }
}

public interface IGameDataProvider
{
    Task<GameData?> GetData();

    public Stream? GetAdjacentFile(string _)
    {
        return null;
    }
}