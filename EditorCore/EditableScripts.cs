using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    public class EditableScripts : IEditableScripts, IDataWrapper
    {
        public event EventHandler<EditableScriptsUpdatedEventArgs> Updated;
        public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated;

        private List<IEditableScript> m_scripts;
        private EditorController m_controller;
        private IMultiScript m_underlyingScript;
        private static EditableDataWrapper<IScript, EditableScripts> s_wrapper;
        private bool m_replacingScripts;

        static EditableScripts()
        {
            s_wrapper = new EditableDataWrapper<IScript, EditableScripts>(GetNewInstance);
        }

        public static EditableScripts GetInstance(EditorController controller, IScript script)
        {
            return s_wrapper.GetInstance(controller, script);
        }

        private static EditableScripts GetNewInstance(EditorController controller, IScript script)
        {
            return new EditableScripts(controller, script);
        }

        public static void Clear()
        {
            s_wrapper.Clear();
        }

        private EditableScripts(EditorController controller)
        {
            m_controller = controller;
            m_scripts = new List<IEditableScript>();
        }

        private EditableScripts(EditorController controller, IScript script)
            : this(controller)
        {
            InitialiseScript(script);
        }

        private void InitialiseScript(IScript script)
        {
            InitialiseMultiScript((IMultiScript) script);

            foreach (IScript scriptItem in m_underlyingScript.Scripts)
            {
                m_scripts.Add(m_controller.ScriptFactory.CreateEditableScript(scriptItem));
            }

            foreach (IEditableScript editableScript in m_scripts)
            {
                editableScript.Updated += script_Updated;
            }

            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);
        }

        private void InitialiseMultiScript(IMultiScript script)
        {
            if (m_underlyingScript != null)
            {
                m_underlyingScript.ScriptUpdated -= multiScript_ScriptUpdated;
            }
            m_underlyingScript = script;
            m_underlyingScript.ScriptUpdated += multiScript_ScriptUpdated;
            m_underlyingScript.UndoLog = m_controller.WorldModel.UndoLogger;
        }

        #region IEditableScripts Members

        public IEnumerable<IEditableScript> Scripts
        {
            get { return m_scripts.AsReadOnly(); }
        }

        private void Add(EditableScriptBase script, bool fromUpdate)
        {
            Add(script, null, fromUpdate);
        }

        private void Add(EditableScriptBase script, int? index, bool fromUpdate)
        {
            script.Updated += script_Updated;
            if (index.HasValue)
            {
                m_scripts.Insert(index.Value, script);
            }
            else
            {
                m_scripts.Add(script);
            }

            if (m_underlyingScript == null)
            {
                InitialiseMultiScript(new MultiScript(m_controller.WorldModel));
            }

            if (!fromUpdate)
            {
                // Add underlying script to multiscript.
                // We don't always want to do this - we might be responding
                // to a multiscript update in the first place so no point adding the same
                // script again!

                m_adding = true;
                m_underlyingScript.Add(script.Script);
                m_adding = false;
            }

            System.Diagnostics.Debug.Assert(m_replacingScripts || m_underlyingScript.Scripts.Count() == m_scripts.Count);
        }

        // TO DO: This is a temporary hacky flag to prevent re-entrant updates. What we should be doing instead is
        // never adding to our own wrapped m_scripts collection unless we receive an update from the underlying
        // MultiScript.
        private bool m_adding = false;

        public void AddNew(string keyword, string elementName)
        {
            m_controller.WorldModel.UndoLogger.StartTransaction(string.Format("Add '{0}' script to '{1}'", keyword, elementName));
            AddNewInternal(keyword);
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        internal void AddNewInternal(string keyword)
        {
            EditableScriptBase script;
            if (!string.IsNullOrEmpty(keyword))
            {
                script = m_controller.ScriptFactory.CreateEditableScript(keyword);
            }
            else
            {
                script = m_controller.ScriptFactory.CreateEditableFunctionCallScript();
            }
            Add(script, false);
        }

        public IEditableScript this[int index]
        {
            get { return m_scripts[index]; }
        }

        public void Remove(int[] indexes)
        {
            string desc = (indexes.Length == 0) ?
                string.Format("Remove '{0}' script", m_scripts[indexes[0]].DisplayString()) :
                string.Format("Remove {0} scripts", indexes.Length);

            m_controller.WorldModel.UndoLogger.StartTransaction(desc);

            var indexesDescending = from index in indexes
                                    orderby index descending
                                    select index;

            foreach (int index in indexesDescending)
            {
                m_scripts.Remove(m_scripts[index]);
                m_underlyingScript.Remove(index);
            }

            m_controller.WorldModel.UndoLogger.EndTransaction();

            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);
        }

        public int Count
        {
            get { return m_scripts.Count; }
        }

        #endregion

        #region IDataWrapper Members

        public object GetUnderlyingValue()
        {
            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);
            return m_underlyingScript;
        }

        #endregion

        private void multiScript_ScriptUpdated(object sender, ScriptUpdatedEventArgs e)
        {
            if (m_adding) return;

            // Has the update to the MultiScript removed one of the scripts? If so we need
            // to remove it from this wrapper too.
            if (e.RemovedScript != null)
            {
                foreach (IEditableScript es in m_scripts.ToArray())
                {
                    EditableScriptBase s = (EditableScriptBase)es;
                    if (s.Script == e.RemovedScript) m_scripts.Remove(es);
                }
            }
            if (e.AddedScript != null)
            {
                Add(m_controller.ScriptFactory.CreateEditableScript(e.AddedScript), true);
            }
            if (e.InsertedScript != null)
            {
                Add(m_controller.ScriptFactory.CreateEditableScript(e.InsertedScript), e.Index, true);
            }
            if (e.ScriptsReplaced)
            {
                m_replacingScripts = true;
                m_scripts.Clear();
                foreach (var script in ((MultiScript) sender).Scripts)
                {
                    Add(m_controller.ScriptFactory.CreateEditableScript(script), true);
                }
                m_replacingScripts = false;
            }
            if (Updated != null) Updated(this, new EditableScriptsUpdatedEventArgs());
            if (UnderlyingValueUpdated != null) UnderlyingValueUpdated(this, new DataWrapperUpdatedEventArgs());

            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);
        }

        private void script_Updated(object sender, EditableScriptUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                Updated(this, new EditableScriptsUpdatedEventArgs((IEditableScript)sender, e));
            }
            if (UnderlyingValueUpdated != null)
            {
                UnderlyingValueUpdated(this, new DataWrapperUpdatedEventArgs());
            }

            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);
        }

        public string DisplayString()
        {
            return DisplayString(-1, string.Empty);
        }

        public string DisplayString(int index, string newValue)
        {
            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);

            int count = 0;
            StringBuilder result = new StringBuilder();
            foreach (IEditableScript script in m_scripts)
            {
                if (result.Length > 0) result.Append(Environment.NewLine);
                if (index == count)
                {
                    result.Append(newValue);
                }
                else
                {
                    result.Append(script.DisplayString());
                }
                count++;
            }
            return result.ToString();
        }

        public void Swap(int index1, int index2)
        {
            m_underlyingScript.Swap(index1, index2);
        }

        public void Cut(int[] indexes)
        {
            Copy(indexes);
            Remove(indexes);
        }

        public void Copy(int[] indexes)
        {
            List<IScript> scripts = new List<IScript>();
            foreach (int index in indexes)
            {
                scripts.Add(m_underlyingScript.Scripts.ElementAt(index));
            }
            m_controller.SetClipboardScript(scripts);
        }

        public void Paste(int index, bool useTransaction)
        {
            if (useTransaction)
            {
                m_controller.StartTransaction("Paste script");
            }
            foreach (IScript script in m_controller.GetClipboardScript())
            {
                m_underlyingScript.Insert(index, m_controller.ScriptFactory.Clone(script));
                index++;
            }
            if (useTransaction)
            {
                m_controller.EndTransaction();
            }
        }

        public IEditableScripts Clone(string parent, string attribute)
        {
            IScript clonedScript = (IScript)m_underlyingScript.Clone();
            Element parentElement = m_controller.WorldModel.Elements.Get(parent);
            parentElement.Fields.Set(attribute, clonedScript);
            clonedScript = (IScript)parentElement.Fields.Get(attribute);
            EditableScripts result = new EditableScripts(m_controller, clonedScript);

            return result;
        }

        public string Owner
        {
            get
            {
                if (m_underlyingScript == null) return null;
                if (m_underlyingScript.Owner == null) return null;
                return m_underlyingScript.Owner.Name;
            }
        }

        public string Code
        {
            get
            {
                if (m_underlyingScript == null) return string.Empty;
                string result = Utility.IndentScript(m_underlyingScript.Save(), 0, "  ");
                if (result.StartsWith(Environment.NewLine)) result = result.Substring(Environment.NewLine.Length);
                if (result.EndsWith(Environment.NewLine)) result = result.Substring(0, result.Length - Environment.NewLine.Length);
                return result;
            }
            set
            {
                m_controller.StartTransaction("Editing script in code view");
                m_underlyingScript.LoadCode(value);
                ClearScripts();
                InitialiseScript(m_underlyingScript);
                m_controller.EndTransaction();
            }
        }

        private void ClearScripts()
        {
            foreach (var script in m_scripts)
            {
                script.Updated -= script_Updated;
            }
            m_scripts.Clear();
        }
    }
}
