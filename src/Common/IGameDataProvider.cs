using System.IO;
using System.Threading.Tasks;

namespace QuestViva.Common;

public class GameData(Stream data, string gameId, string filename, IGameDataProvider provider)
{
    public Stream Data => data;
    public string GameId => gameId;
    public string Filename => filename;
    public Stream? GetAdjacentFile(string file) => provider.GetAdjacentFile(file);
    public bool IsCompiled { get; init; }
    public string? ResourceRoot { get; init; }
}

public interface IGameDataProvider
{
    Task<GameData?> GetData();
    
    public Stream? GetAdjacentFile(string _) => null;
}