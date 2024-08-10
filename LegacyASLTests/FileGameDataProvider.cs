using TextAdventures.Quest;

namespace LegacyASLTests;

public class FileGameDataProvider(string filename): IGameDataProvider
{
    public Stream GetData()
    {
        return File.OpenRead(Filename);
    }

    public string Filename { get; } = filename;
}