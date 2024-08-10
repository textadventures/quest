using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;

namespace TextAdventures.Quest
{
    internal class PackageReader
    {
        public class ReadResult
        {
            public byte[] GameFile;
            public string Folder;
        }

        public async Task<ReadResult> LoadPackage(IGameDataProvider gameDataProvider)
        {
            ReadResult result = new ReadResult();
            string tempDir = Path.Combine(Path.GetTempPath(), "Quest", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            
            var bytes = await gameDataProvider.GetDataAsync();
            var memoryStream = new MemoryStream(bytes);
            
            ZipFile zip = ZipFile.Read(memoryStream);
            zip.ExtractAll(tempDir);

            result.Folder = tempDir;
            var gameFile = Path.Combine(tempDir, "game.aslx");

            if (!File.Exists(gameFile))
            {
                throw new InvalidDataException("Invalid game file");
            }

            result.GameFile = await File.ReadAllBytesAsync(gameFile);

            return result;
        }
    }
}
