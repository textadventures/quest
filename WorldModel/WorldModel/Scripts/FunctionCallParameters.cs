using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    // We store the parameters internally as a QuestList<string>, so we can edit them in the Editor
    // using the standard string list editor control.

    // Note that we haven't implemented any functionality to keep the two lists in sync, because
    // when playing or editing a game we only actually care about one of the lists, and when playing
    // a game there is no mechanism for modifying a script command.
    internal class FunctionCallParameters
    {
        private IList<IFunction<object>> m_parameters;
        private QuestList<string> m_parameterStrings = new QuestList<string>();

        public FunctionCallParameters(WorldModel worldModel, IList<IFunction<object>> parameters)
        {
            m_parameterStrings.UndoLog = worldModel.UndoLogger;
            m_parameters = parameters;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    string paramString = param.Save();
                    m_parameterStrings.Add(paramString);
                }
            }
        }

        public IList<IFunction<object>> Parameters
        {
            get { return m_parameters; }
        }

        public QuestList<string> ParametersAsQuestList
        {
            get { return m_parameterStrings; }
        }
    }
}
