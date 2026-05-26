using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using QuestViva.Common;

namespace QuestViva.Engine.GameLoader;

internal class PackageReader
{
    public class ReadResult
    {
        private readonly Dictionary<string, byte[]> _files = new();
            
        public ReadResult(ZipArchive zip, Stream gameFile)
        {
            foreach (var entry in zip.Entries)
            {
                var entryName = entry.FullName;
                var memoryStream = new MemoryStream();
                entry.Open().CopyTo(memoryStream);
                _files.Add(entryName, memoryStream.ToArray());
            }

            GameFile = gameFile;
        }
            
        public readonly Stream GameFile;
            
        public Stream GetFile(string filename)
        {
            var bytes = _files[filename];
            return new MemoryStream(bytes);
        }
            
        public IEnumerable<string> GetFileNames() => _files.Keys;
    }

    public static Task<ReadResult> LoadPackage(GameData gameData)
    {
        var packageStream = gameData.Data;
        var zip = new ZipArchive(packageStream, ZipArchiveMode.Read);
            
        var gameFile = zip.GetEntry("game.aslx");
            
        if (gameFile == null)
        {
            throw new InvalidDataException("Invalid game file");
        }
            
        return Task.FromResult(new ReadResult(zip, gameFile.Open()));
    }
}