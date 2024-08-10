using TextAdventures.Quest;

namespace WorldModelTests;

public class FileGameDataProvider(string filename): IGameDataProvider
{
    public Stream GetData()
    {
        return File.OpenRead(Filename);
    }

    public string Filename { get; } = filename;
}