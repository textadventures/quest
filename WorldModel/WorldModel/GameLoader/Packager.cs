using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace AxeSoftware.Quest
{
    internal class Packager
    {
        private WorldModel m_worldModel;

        public Packager(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public void CreatePackage(string filename)
        {
            string data = m_worldModel.Save(SaveMode.Package);

            using (ZipFile zip = new ZipFile(filename))
            {
                zip.AddEntry("game.aslx", data);
                zip.Save();
            }
        }
    }
}
