using System.IO.Compression;
using QuestViva.Common;

namespace QuestViva.Engine.GameLoader;

internal class PackageReader
{
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

    public class ReadResult
    {
        private readonly Dictionary<string, byte[]> _files = new(StringComparer.OrdinalIgnoreCase);

        public readonly Stream GameFile;

        public ReadResult(ZipArchive zip, Stream gameFile)
        {
            foreach (var entry in zip.Entries)
            {
                var entryName = entry.FullName;
                var memoryStream = new MemoryStream();
                entry.Open().CopyTo(memoryStream);
                _files.TryAdd(entryName, memoryStream.ToArray());
            }

            GameFile = gameFile;
        }

        public Stream? GetFile(string filename)
        {
            return _files.TryGetValue(filename, out var bytes) ? new MemoryStream(bytes) : null;
        }

        public IEnumerable<string> GetFileNames()
        {
            return _files.Keys;
        }
    }
}