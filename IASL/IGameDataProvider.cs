using System.IO;
using System.Threading.Tasks;

namespace TextAdventures.Quest;

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
}