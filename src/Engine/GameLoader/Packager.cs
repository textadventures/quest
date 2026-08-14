using System.IO.Compression;
using System.Text;

namespace QuestViva.Engine.GameLoader;

internal class Packager(WorldModel worldModel)
{
    private readonly WorldModel _worldModel = worldModel;

    public bool CreatePackage(string filename, bool includeWalkthrough, out string error,
        IEnumerable<WorldModel.PackageIncludeFile> includeFiles, Stream outputStream)
    {
        error = string.Empty;

        try
        {
            var data = _worldModel.Save(SaveMode.Package, includeWalkthrough);

            using var stream = filename != null ? File.Create(filename) : outputStream;
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var gameEntry = zip.CreateEntry("game.aslx", CompressionLevel.Optimal);
                using (var entryStream = gameEntry.Open())
                using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    writer.Write(data);
                }

                foreach (var file in includeFiles)
                {
                    var fileEntry = zip.CreateEntry(file.Filename, CompressionLevel.Optimal);
                    using var fileEntryStream = fileEntry.Open();
                    file.Content.CopyTo(fileEntryStream);
                }
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }

        return true;
    }
}
