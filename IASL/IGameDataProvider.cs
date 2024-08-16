using System.IO;
using System.Threading.Tasks;

namespace TextAdventures.Quest;

public interface IGameDataProvider
{
    Task<Stream> GetData();
    string Filename { get; }
}