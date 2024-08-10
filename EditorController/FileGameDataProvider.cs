using System.IO;
using System.Threading.Tasks;

namespace TextAdventures.Quest;

public class FileGameDataProvider(string filename): IGameDataProvider
{
    public async Task<byte[]> GetDataAsync()
    {
        return await File.ReadAllBytesAsync(Filename);
    }

    public string Filename { get; } = filename;
}