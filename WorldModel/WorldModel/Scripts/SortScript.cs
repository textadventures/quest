/*
 * This might be useful at some point, but the Undo interface would need to be updated to support
 * undoing a sort on a list which was an object field
 * 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class SortScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public SortScriptConstructor()
        {
        }

        public override string Keyword
        {
            get { return "sort"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new SortScript(new ExpressionGeneric(parameters[0]), parameters[1]);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
        #endregion
    }

    public class SortScript : ScriptBase
    {
        private IFunctionGeneric m_list;
        private string m_sortFunction;

        public SortScript(IFunctionGeneric list, string sortFunction)
        {
            m_list = list;
            m_sortFunction = sortFunction;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            object result = m_list.Execute(c);

            if (result is QuestList<Object>)
            {
                ((QuestList<Object>)result).Sort(new Comparer<Object>());
            }
            else if (result is QuestList<Exit>)
            {
                ((QuestList<Exit>)result).Sort(new Comparer<Exit>());
            }
            else if (result is QuestList<Command>)
            {
                ((QuestList<Command>)result).Sort(new Comparer<Command>());
            }
            else if (result is QuestList<string>)
            {
                ((QuestList<string>)result).Sort(new Comparer<string>());
            }
            else
            {
                throw new Exception("Unrecognised list type");
            }
        }

        #endregion

        private class Comparer<T> : IComparer<T>
        {

            #region IComparer<T> Members

            public int Compare(T x, T y)
            {
                if (x.Equals(y)) return 0;
                return 1;
            }

            #endregion
        }
    }
}
*/