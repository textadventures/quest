using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    internal class API
    {
        private WorldModel m_worldModel;
        private ConducttrApi m_conducttr;

        public API(WorldModel worldModel)
        {
            // non-prototype version will need to register all external assemblies
            // that implement some interface

            m_worldModel = worldModel;
            m_conducttr = new ConducttrApi();
        }

        // We will want to be able to call methods and functions via reflection

        public void Execute(string api, string method, IList<string> parameters, IScript callback)
        {
            if (api != "conducttr")
            {
                throw new ArgumentOutOfRangeException();
            }

            string result = m_conducttr.Execute(method, parameters);
            m_worldModel.RunScript(callback, new Parameters("result", result));
        }
    }
}
