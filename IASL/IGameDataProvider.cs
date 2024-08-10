using System.IO;
using System.Threading.Tasks;

namespace TextAdventures.Quest;

public interface IGameDataProvider
{
    Stream GetData();
    string Filename { get; }
}