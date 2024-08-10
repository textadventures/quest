using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace TextAdventures.Quest
{
    internal class PackageReader
    {
        public class ReadResult
        {
            public byte[] GameFile;
            
            // TODO: Replace this with a function for extracting resources
            public string Folder;
        }

        public async Task<ReadResult> LoadPackage(IGameDataProvider gameDataProvider)
        {
            var packageBytes = await gameDataProvider.GetDataAsync();
            var packageStream = new MemoryStream(packageBytes);
            var zip = new ZipArchive(packageStream, ZipArchiveMode.Read);
            
            var gameFile = zip.GetEntry("game.aslx");
            
            if (gameFile == null)
            {
                throw new InvalidDataException("Invalid game file");
            }
            
            var gameStream = gameFile.Open();
            using var ms = new MemoryStream();
            await gameStream.CopyToAsync(ms);
            
            var result = new ReadResult
            {
                GameFile = ms.ToArray()
            };

            return result;
        }
    }
}
