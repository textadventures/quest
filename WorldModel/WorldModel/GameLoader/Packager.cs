using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace TextAdventures.Quest
{
    internal class Packager
    {
        private WorldModel m_worldModel;

        public Packager(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public bool CreatePackage(string filename, bool includeWalkthrough, out string error)
        {
            error = string.Empty;

            try
            {
                string data = m_worldModel.Save(SaveMode.Package, includeWalkthrough);
                string baseFolder = System.IO.Path.GetDirectoryName(m_worldModel.Filename);

                using (ZipFile zip = new ZipFile(filename))
                {
                    zip.AddEntry("game.aslx", data, Encoding.UTF8);
                    foreach (string file in m_worldModel.GetAvailableExternalFiles("*.jpg;*.jpeg;*.png;*.gif;*.js;*.wav;*.mp3;*.htm;*.html"))
                    {
                        zip.AddFile(System.IO.Path.Combine(baseFolder, file), "");
                    }
                    AddLibraryResources(zip, baseFolder, ElementType.Javascript);
                    AddLibraryResources(zip, baseFolder, ElementType.Resource);
                    zip.Save();
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
                    string libFolder = System.IO.Path.GetDirectoryName(e.MetaFields[MetaFieldDefinitions.Filename]);
                    libFolder = TextAdventures.Utility.Utility.RemoveFileColonPrefix(libFolder);
                    if (libFolder != baseFolder)
                    {
                        zip.AddFile(System.IO.Path.Combine(libFolder, e.Fields[FieldDefinitions.Src]), "");
                    }
                }
            }
        }
    }
}
