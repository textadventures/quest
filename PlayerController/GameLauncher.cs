using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;
using Ionic.Zip;
using System.IO;

namespace AxeSoftware.Quest
{
    public static class GameLauncher
    {
        public static IASL GetGame(string filename, string libraryFolder)
        {
            return GetGame(filename, libraryFolder, null);
        }

        private static IASL GetGame(string filename, string libraryFolder, string originalFilename)
        {
            switch (System.IO.Path.GetExtension(filename).ToLower())
            {
                case ".aslx":
                    return new WorldModel(filename, libraryFolder, originalFilename);
                case ".asl":
                case ".cas":
                case ".qsg":
                    LegacyASL.LegacyGame game = new AxeSoftware.Quest.LegacyASL.LegacyGame(filename, originalFilename);
                    game.SetUnzipFunction(UnzipAndGetGameFile);
                    return game;
                case ".zip":
                    return GetGameFromZip(filename, libraryFolder);
                default:
                    return null;
            }
        }

        private static IASL GetGameFromZip(string filename, string libraryFolder)
        {
            string gameFile = UnzipAndGetGameFile(filename);
            if (gameFile == null) return null;
            return GetGame(gameFile, libraryFolder, filename);
        }

        private static string UnzipAndGetGameFile(string zipFile)
        {
            // Unzips a file to a temp directory and returns the path of the ASL/CAS/ASLX file contained within it.

            string tempDir = Path.Combine(Path.GetTempPath(), "Quest", Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(zipFile));
            Directory.CreateDirectory(tempDir);
            ZipFile zip = ZipFile.Read(zipFile);
            zip.ExtractAll(tempDir);

            return SearchForGameFile(tempDir, "aslx", "asl", "cas");
        }

        private static string SearchForGameFile(string dir, params string[] exts)
        {
            foreach (string ext in exts)
            {
                string[] result = Directory.GetFiles(dir, "*." + ext, SearchOption.AllDirectories);
                if (result.Any())
                {
                    return result[0];
                }
            }
            return null;
        }
    }
}
