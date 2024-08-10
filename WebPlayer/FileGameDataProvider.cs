using TextAdventures.Quest;

namespace WebPlayer;

public class FileGameDataProvider(string filename): IGameDataProvider
{
    public Stream GetData()
    {
        return File.OpenRead(Filename);
    }

    public string Filename { get; } = filename;
}