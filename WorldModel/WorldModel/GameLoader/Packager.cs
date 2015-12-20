using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using System.IO;

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

                using (ZipFile zip = new ZipFile(Encoding.UTF8))
                {
                    zip.AddEntry("game.aslx", data, Encoding.UTF8);

                    if (includeFiles == null)
                    {
                        var fileTypesToInclude = m_worldModel.Game.Fields[FieldDefinitions.PublishFileExtensions] ??
                                             "*.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html;*.svg";

                        foreach (var file in m_worldModel.GetAvailableExternalFiles(fileTypesToInclude))
                        {
                            zip.AddFile(Path.Combine(baseFolder, file), "");
                        }
                    }
                    else
                    {
                        foreach (var file in includeFiles)
                        {
                            zip.AddEntry(file.Filename, file.Content);
                        }
                    }
                    
                    AddLibraryResources(zip, baseFolder, ElementType.Javascript);
                    AddLibraryResources(zip, baseFolder, ElementType.Resource);

                    if (filename != null)
                    {
                        zip.Save(filename);
                    }
                    else if (outputStream != null)
                    {
                        zip.Save(outputStream);
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

        private void AddLibraryResources(ZipFile zip, string baseFolder, ElementType elementType)
        {
            foreach (Element e in m_worldModel.Elements.GetElements(elementType))
            {
                if (e.MetaFields[MetaFieldDefinitions.Library])
                {
                    string libFolder = Path.GetDirectoryName(e.MetaFields[MetaFieldDefinitions.Filename]);
                    libFolder = TextAdventures.Utility.Utility.RemoveFileColonPrefix(libFolder);
                    if (libFolder != baseFolder)
                    {
                        zip.AddFile(Path.Combine(libFolder, e.Fields[FieldDefinitions.Src]), "");
                    }
                }
            }
        }
    }
}
