using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace TextAdventures.Quest
{
    internal class Packager
    {
        private WorldModel m_worldModel;

        public Packager(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public bool CreatePackage(string filename, bool includeWalkthrough, out string error, IEnumerable<WorldModel.PackageIncludeFile> includeFiles, Stream outputStream)
        {
            error = string.Empty;

            try
            {
                string data = m_worldModel.Save(SaveMode.Package, includeWalkthrough);
                string baseFolder = Path.GetDirectoryName(m_worldModel.Filename);

                Stream zipStream = filename != null
                    ? new FileStream(filename, FileMode.Create, FileAccess.Write)
                    : outputStream;

                using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: filename == null))
                {
                    AddStringEntry(zip, "game.aslx", data, Encoding.UTF8);

                    if (includeFiles == null)
                    {
                        var fileTypesToInclude = m_worldModel.Game.Fields[FieldDefinitions.PublishFileExtensions] ??
                                             "*.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg";

                        foreach (var file in m_worldModel.GetAvailableExternalFiles(fileTypesToInclude))
                        {
                            zip.CreateEntryFromFile(Path.Combine(baseFolder, file), Path.GetFileName(file));
                        }
                    }
                    else
                    {
                        foreach (var file in includeFiles)
                        {
                            AddStreamEntry(zip, file.Filename, file.Content);
                        }
                    }

                    AddLibraryResources(zip, baseFolder, ElementType.Javascript);
                    AddLibraryResources(zip, baseFolder, ElementType.Resource);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            return true;
        }

        private static void AddStringEntry(ZipArchive zip, string name, string content, Encoding encoding)
        {
            using (var writer = new StreamWriter(zip.CreateEntry(name).Open(), encoding))
            {
                writer.Write(content);
            }
        }

        private static void AddStreamEntry(ZipArchive zip, string name, Stream content)
        {
            using (var entryStream = zip.CreateEntry(name).Open())
            {
                content.CopyTo(entryStream);
            }
        }

        private void AddLibraryResources(ZipArchive zip, string baseFolder, ElementType elementType)
        {
            foreach (Element e in m_worldModel.Elements.GetElements(elementType))
            {
                if (e.MetaFields[MetaFieldDefinitions.Library])
                {
                    string libFolder = Path.GetDirectoryName(e.MetaFields[MetaFieldDefinitions.Filename]);
                    libFolder = TextAdventures.Utility.Utility.RemoveFileColonPrefix(libFolder);
                    if (libFolder != baseFolder)
                    {
                        string src = e.Fields[FieldDefinitions.Src];
                        zip.CreateEntryFromFile(Path.Combine(libFolder, src), Path.GetFileName(src));
                    }
                }
            }
        }
    }
}
