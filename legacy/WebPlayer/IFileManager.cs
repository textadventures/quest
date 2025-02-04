using System.Threading.Tasks;

namespace WebPlayer
{
    public class SourceFileData
    {
        public string Filename { get; set; }
        public bool? IsCompiled { get; set; }
    }

    public interface IFileManager
    {
        Task<SourceFileData> GetFileForID(string id);
    }
}