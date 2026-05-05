using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace TextAdventures.Quest
{
    internal class PackageReader
    {
        public class ReadResult
        {
            public string GameFile;
            public string Folder;
        }

        public ReadResult LoadPackage(string filename)
        {
            ReadResult result = new ReadResult();
            string tempDir = Path.Combine(Path.GetTempPath(), "Quest", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(filename, tempDir);

            result.Folder = tempDir;
            result.GameFile = Path.Combine(tempDir, "game.aslx");

            if (!File.Exists(result.GameFile))
            {
                throw new InvalidDataException("Invalid game file");
            }

            return result;
        }
    }
}
