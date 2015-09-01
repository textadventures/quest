using System.Threading.Tasks;

namespace WebPlayer
{
    public interface IFileManager
    {
        Task<string> GetFileForID(string id);
    }
}