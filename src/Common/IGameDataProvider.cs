using System.IO;
using System.Threading.Tasks;

namespace QuestViva.Common;

public class GameData(Stream data, string filename, IGameDataProvider provider)
{
    public Stream Data => data;
    public string Filename => filename;
    public Stream? GetAdjacentFile(string file) => provider.GetAdjacentFile(file);
}

public interface IGameDataProvider
{
    Task<GameData?> GetData();
    
    /// <summary>
    /// A unique id to be used when fetching resources for this game. Different instances of the same game can
    /// share a ResourcesId.
    /// </summary>
    string ResourcesId { get; }
    
    public Stream? GetAdjacentFile(string _) => null;
}