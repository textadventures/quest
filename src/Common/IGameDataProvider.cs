using System.IO;
using System.Threading.Tasks;

namespace QuestViva.Common;

public interface IGameData
{
    Stream Data { get; }
    string Filename { get; }
}

public class GameData(Stream data, string filename) : IGameData
{
    public Stream Data => data;
    public string Filename => filename;
}

public interface IGameDataProvider
{
    Task<IGameData?> GetData();
    
    /// <summary>
    /// A unique id to be used when fetching resources for this game. Different instances of the same game can
    /// share a ResourcesId.
    /// </summary>
    string ResourcesId { get; }
}