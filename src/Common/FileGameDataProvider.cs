using System.IO;
using System.Threading.Tasks;

namespace QuestViva.Common;

public class FileGameDataProvider(string filename, string resourcesId): IGameDataProvider
{
    public Task<IGameData?> GetData()
    {
        var stream = File.OpenRead(filename);
        return Task.FromResult<IGameData?>(new GameData(stream, filename));
    }

    public string ResourcesId => resourcesId;

    public string Filename => filename;
}