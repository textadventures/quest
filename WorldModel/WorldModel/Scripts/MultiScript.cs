using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest.Scripts
{
    public class MultiScript : ScriptBase
    {
        private List<IScript> m_scripts;

        public MultiScript(params IScript[] scripts)
        {
            m_scripts = new List<IScript>(scripts);
        }

        public void Add(params IScript[] scripts)
        {
            m_scripts.AddRange(scripts);
            if (base.UndoLog != null)
            {
                foreach (IScript script in scripts)
                {
                    base.UndoLog.AddUndoAction(new UndoMultiScriptAddRemove(this, script, true, null));
                }
            }

            foreach (IScript script in scripts)
            {
                NotifyUpdate(script, null);
            }
        }

        public void Remove(int index)
        {
            if (base.UndoLog != null)
            {
                base.UndoLog.AddUndoAction(new UndoMultiScriptAddRemove(this, m_scripts[index], false, index));
            }
            RemoveSilent(m_scripts[index]);
        }

        private void RemoveSilent(IScript script)
        {
            m_scripts.Remove(script);
            NotifyUpdate(null, script);
        }

        private void AddSilent(IScript script)
        {
            m_scripts.Add(script);
            NotifyUpdate(script, null);
        }

        public void Insert(int index, IScript script)
        {
            if (base.UndoLog != null)
            {
                base.UndoLog.AddUndoAction(new UndoMultiScriptAddRemove(this, script, true, index));
            }
            InsertSilent(index, script);
        }

        private void InsertSilent(int index, IScript script)
        {
            m_scripts.Insert(index, script);
            NotifyUpdate(script, index);
        }

        public void Swap(int index1, int index2)
        {
            if (index1 == index2) return;
            if (index1 > index2)
            {
                int temp = index1;
                index1 = index2;
                index2 = temp;
            }

            IScript script;

            // This swap assumes index1 < index2
            script = m_scripts[index1];
            Remove(index1);
            Insert(index2, script);

            // If the elements are consecutive then there's no need to
            // move the second script, as it's already in the correct place
            if (index1 != index2 - 1)
            {
                script = m_scripts[index2 - 1];
                Remove(index2 - 1);
                Insert(index1, script);
            }
        }

        public IEnumerable<IScript> Scripts
        {
            get
            {
                return m_scripts.AsReadOnly();
            }
        }

        public override void Execute(Context c)
        {
            foreach (IScript script in m_scripts)
            {
                script.Execute(c);
            }
        }

        public override string Line
        {
            get
            {
                string result = string.Empty;
                foreach (IScript script in m_scripts)
                {
                    result += script.Line + Environment.NewLine;
                }
                return result;
            }
            set
            {
                throw new Exception("Cannot set Line in MultiScript");
            }
        }

        public override string Save()
        {
            string result = string.Empty;

            foreach (IScript script in m_scripts)
            {
                if (result.Length > 0) result += Environment.NewLine;
                result += script.Save();
            }

            return result;
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new NotImplementedException();
        }

        public override object GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        private class UndoMultiScriptAddRemove : AxeSoftware.Quest.UndoLogger.IUndoAction
        {
            private MultiScript m_appliesTo;
            private IScript m_script;
            private bool m_isAdd;
            private int? m_index;

            public UndoMultiScriptAddRemove(MultiScript appliesTo, IScript script, bool isAdd, int? index)
            {
                m_appliesTo = appliesTo;
                m_script = script;
                m_isAdd = isAdd;
                m_index = index;
            }

            #region IUndoAction Members

            public void DoUndo(WorldModel worldModel)
            {
                if (m_isAdd)
                {
                    DoRemove();
                }
                else
                {
                    DoAdd();
                }
            }

            public void DoRedo(WorldModel worldModel)
            {
                if (m_isAdd)
                {
                    DoAdd();
                }
                else
                {
                    DoRemove();
                }
            }

            #endregion

            private void DoAdd()
            {
                if (m_index.HasValue)
                {
                    m_appliesTo.InsertSilent(m_index.Value, m_script);
                }
                else
                {
                    m_appliesTo.AddSilent(m_script);
                }
            }

            private void DoRemove()
            {
                m_appliesTo.RemoveSilent(m_script);
            }
        }

    }
}
