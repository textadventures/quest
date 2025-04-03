using System.IO;
using System.Threading.Tasks;

namespace QuestViva.Common;

public class FileGameDataProvider(string filename, IResourceProvider resourceProvider): IGameDataProvider
{
    public Task<GameData?> GetData()
    {
        var stream = File.OpenRead(filename);
        var gameId = Path.GetFileName(filename);
        return Task.FromResult<GameData?>(new GameData(stream, gameId, filename, this));
    }

    public IResourceProvider ResourceProvider => resourceProvider;

    public virtual Stream? GetAdjacentFile(string adjacentFilename) => null;
}

public class FileDirectoryGameDataProvider(string filename, IResourceProvider resourceProvider): FileGameDataProvider(filename, resourceProvider)
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