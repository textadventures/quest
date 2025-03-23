using System.IO;
using System.Threading.Tasks;

namespace QuestViva.Common;

public class FileGameDataProvider(string filename, string resourcesId): IGameDataProvider
{
    public Task<GameData?> GetData()
    {
        var stream = File.OpenRead(filename);
        return Task.FromResult<GameData?>(new GameData(stream, filename, filename, this));
    }

    public string ResourcesId => resourcesId;

    public virtual Stream? GetAdjacentFile(string adjacentFilename) => null;
}

public class FileDirectoryGameDataProvider(string filename, string resourcesId): FileGameDataProvider(filename, resourcesId)
{
    private readonly string? _parentDirectory = Path.GetDirectoryName(filename);

    public override Stream? GetAdjacentFile(string adjacentFilename)
    {
        if (_parentDirectory == null)
        {
            return null;
        }
            
        var adjacentFilePath = Path.Combine(_parentDirectory, adjacentFilename);
        if (File.Exists(adjacentFilePath))
        {
            return File.OpenRead(adjacentFilePath);
        }
        return null;
    }
}