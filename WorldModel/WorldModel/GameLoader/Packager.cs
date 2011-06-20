using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            System.IO.File.WriteAllText(filename, data);
        }
    }
}
