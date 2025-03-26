#nullable enable
using System;
using System.IO;
using System.Linq;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Legacy;

namespace QuestViva.PlayerCore;

public class GameLauncher(WorldModelFactory worldModelFactory)
{
    public IGame? GetGame(GameData gameData, Stream? saveData)
    {
        switch (Path.GetExtension(gameData.Filename).ToLower())
        {
            case ".aslx":
            case ".quest":
            case ".quest-save":
                return worldModelFactory.Create(gameData, saveData);
            case ".asl":
            case ".cas":
            case ".qsg":
                V4Game game = new V4Game(gameData);
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

    // For Zip files we previously used DotNetZip - should be able to use System.IO.Compression now:
    // https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-compress-and-extract-files
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.zipfile?view=net-9.0
    // We already use this in WorldModel/WorldModel/GameLoader/PackageReader.cs
        
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
        throw new NotImplementedException();

        // // Unzips a file to a temp directory and returns the path of the ASL/CAS/ASLX file contained within it.
        //
        // tempDir = Path.Combine(Path.GetTempPath(), "Quest", Guid.NewGuid().ToString());
        // Directory.CreateDirectory(tempDir);
        // ZipFile zip = ZipFile.Read(zipFile);
        // zip.ExtractAll(tempDir);
        //
        // return SearchForGameFile(tempDir, "aslx", "asl", "cas");
    }

    private static string? SearchForGameFile(string dir, params string[] exts)
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