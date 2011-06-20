using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zip;

namespace AxeSoftware.Quest
{
    internal class PackageReader
    {
        public string LoadPackage(string filename)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "Quest", Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(filename));
            Directory.CreateDirectory(tempDir);
            ZipFile zip = ZipFile.Read(filename);
            zip.ExtractAll(tempDir);
            string result = Path.Combine(tempDir, "game.aslx");
            if (!File.Exists(result))
            {
                throw new InvalidDataException("Invalid game file");
            }
            return result;
        }
    }
}
