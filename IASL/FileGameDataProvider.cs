using System.IO;
using System.Threading.Tasks;

namespace TextAdventures.Quest;

public class FileGameDataProvider(string filename): IGameDataProvider
{
    public Task<IGameData?> GetData()
    {
        var stream = File.OpenRead(filename);
        return Task.FromResult<IGameData?>(new GameData(stream, filename));
    }
    
    public string Filename => filename;
}