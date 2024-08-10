using System.Threading.Tasks;

namespace TextAdventures.Quest;

public interface IGameDataProvider
{
    Task<byte[]> GetDataAsync();
    string Filename { get; }
}