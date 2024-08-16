using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace TextAdventures.Quest
{
    internal class PackageReader
    {
        public class ReadResult(ZipArchive zip)
        {
            public Stream GameFile;
            
            public Stream GetFile(string filename)
            {
                var entry = zip.GetEntry(filename);
                return entry?.Open();
            }
        }

        public Task<ReadResult> LoadPackage(IGameData gameData)
        {
            var packageStream = gameData.Data;
            var zip = new ZipArchive(packageStream, ZipArchiveMode.Read);
            
            var gameFile = zip.GetEntry("game.aslx");
            
            if (gameFile == null)
            {
                throw new InvalidDataException("Invalid game file");
            }
            
            return Task.FromResult(new ReadResult(zip)
            {
                GameFile = gameFile.Open()
            });
        }
    }
}
