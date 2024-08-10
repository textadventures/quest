using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest;
using Ionic.Zip;
using System.IO;
using TextAdventures.Quest.LegacyASL;

namespace TextAdventures.Quest
{
    public static class GameLauncher
    {
        public static IASL GetGame(IGameDataProvider gameDataProvider, string libraryFolder)
        {
            string tempDir;
            switch (System.IO.Path.GetExtension(gameDataProvider.Filename).ToLower())
            {
                case ".aslx":
                case ".quest":
                case ".quest-save":
                    return new WorldModel(gameDataProvider, libraryFolder);
                case ".asl":
                case ".cas":
                case ".qsg":
                    LegacyASL.LegacyGame game = new LegacyGame(gameDataProvider);
                    game.SetUnzipFunction(UnzipAndGetGameFile);
                    return game;
                case ".zip":
                    // TODO
                    throw new NotImplementedException();
                    // return GetGameFromZip(filename, libraryFolder, out tempDir);
                default:
                    return null;
            }
        }

        // private static IASL GetGameFromZip(string filename, string libraryFolder, out string tempDir)
        // {
        //     string gameFile = UnzipAndGetGameFile(filename, out tempDir);
        //     if (gameFile == null) return null;
        //     IASL result = GetGame(gameFile, libraryFolder, filename);
        //     if (result != null)
        //     {
        //         result.TempFolder = tempDir;
        //     }
        //     return result;
        // }

        private static string UnzipAndGetGameFile(string zipFile, out string tempDir)
        {
            // Unzips a file to a temp directory and returns the path of the ASL/CAS/ASLX file contained within it.

            tempDir = Path.Combine(Path.GetTempPath(), "Quest", Guid.NewGuid().ToString());
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
