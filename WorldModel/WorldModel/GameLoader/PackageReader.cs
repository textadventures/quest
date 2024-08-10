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
            public Stream GameFile;
            
            // TODO: Replace this with a function for extracting resources
            public string Folder;
        }

        public ReadResult LoadPackage(IGameDataProvider gameDataProvider)
        {
            var packageStream = gameDataProvider.GetData();
            var zip = new ZipArchive(packageStream, ZipArchiveMode.Read);
            
            var gameFile = zip.GetEntry("game.aslx");
            
            if (gameFile == null)
            {
                throw new InvalidDataException("Invalid game file");
            }
            
            return new ReadResult
            {
                GameFile = gameFile.Open()
            };
        }
    }
}
