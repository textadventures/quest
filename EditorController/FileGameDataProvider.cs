using System.IO;
using System.Threading.Tasks;

namespace TextAdventures.Quest;

public class FileGameDataProvider(string filename): IGameDataProvider
{
    public Stream GetData()
    {
        return File.OpenRead(Filename);
    }

    public string Filename { get; } = filename;
}