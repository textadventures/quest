using System.IO.Compression;
using System.Text;

namespace QuestViva.Engine.GameLoader;

internal class Packager(WorldModel worldModel)
{
    private readonly WorldModel _worldModel = worldModel;

    public bool CreatePackage(string? filename, bool includeWalkthrough, out string error,
        IEnumerable<WorldModel.PackageIncludeFile>? includeFiles, Stream? outputStream)
    {
        error = string.Empty;

        try
        {
            // Editor normally assigns gameid on open; ensure publish never ships without an IFID.
            if (string.IsNullOrWhiteSpace(_worldModel.Game.Fields.GetString("gameid")))
            {
                _worldModel.Game.Fields.Set("gameid", Guid.NewGuid().ToString());
            }

            var data = _worldModel.Save(SaveMode.Package, includeWalkthrough);
            // Babel "other file formats": literal ASCII UUID://…// somewhere in the file.
            // ZIP archive comment is written at the end of the archive as raw bytes.
            // Trailing newline to workaround https://github.com/iftechfoundation/babel-tool/issues/43
            var ifid = _worldModel.Game.Fields.GetString("gameid")!.Trim().ToUpperInvariant();
            var ifidBrand = $"UUID://{ifid}//\n";

            var includeFileList = includeFiles?.ToList() ?? [];
            var ifiction = IFictionWriter.Write(_worldModel, includeFileList);

            if (filename != null)
            {
                using var fileStream = File.Create(filename);
                WriteZip(fileStream, data, ifidBrand, ifiction, includeFileList);
            }
            else
            {
                // Caller owns outputStream — do not dispose it. Callers pass one whenever they
                // don't pass a filename.
                WriteZip(outputStream!, data, ifidBrand, ifiction, includeFileList);
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }

        return true;
    }

    private static void WriteZip(Stream stream, string data, string ifidBrand, string ifiction,
        List<WorldModel.PackageIncludeFile> includeFileList)
    {
        using var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
        zip.Comment = ifidBrand;

        var gameEntry = zip.CreateEntry("game.aslx", CompressionLevel.Optimal);
        using (var entryStream = gameEntry.Open())
        using (var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
        {
            writer.Write(data);
        }

        var ifictionEntry = zip.CreateEntry("metadata.iFiction", CompressionLevel.Optimal);
        using (var entryStream = ifictionEntry.Open())
        using (var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
        {
            writer.Write(ifiction);
        }

        foreach (var file in includeFileList)
        {
            var fileEntry = zip.CreateEntry(file.Filename, CompressionLevel.Optimal);
            using var fileEntryStream = fileEntry.Open();
            file.Content.CopyTo(fileEntryStream);
        }
    }
}
