using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public class EditableScripts : IEditableScripts, IDataWrapper
    {
        private List<IEditableScript> m_scripts;
        private EditorController m_controller;
        private MultiScript m_underlyingScript;
        private Element m_parent;

        private static Dictionary<IScript, EditableScripts> s_instances = new Dictionary<IScript, EditableScripts>();

        public static EditableScripts GetInstance(EditorController controller, IScript script, Element parent)
        {
            EditableScripts instance;
            if (s_instances.TryGetValue(script, out instance))
            {
                System.Diagnostics.Debug.Assert(instance.m_parent == parent, "Wrapped value has been moved - cached IEditableScript will be invalid");
                return instance;
            }

            instance = new EditableScripts(controller, script, parent);
            s_instances.Add(script, instance);
            return instance;
        }

        private EditableScripts(EditorController controller, Element parent)
        {
            m_controller = controller;
            m_parent = parent;
            m_scripts = new List<IEditableScript>();
        }

        private EditableScripts(EditorController controller, IScript script, Element parent)
            : this(controller, parent)
        {
            InitialiseMultiScript((MultiScript)script);
            foreach (IScript scriptItem in m_underlyingScript.Scripts)
            {
                m_scripts.Add(m_controller.ScriptFactory.CreateEditableScript(scriptItem, parent));
            }

            foreach (IEditableScript editableScript in m_scripts)
            {
                editableScript.Updated += script_Updated;
            }

            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);
        }

        private void InitialiseMultiScript(MultiScript script)
        {
            System.Diagnostics.Debug.Assert(m_underlyingScript == null);
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
                InitialiseMultiScript(new MultiScript());
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

            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);
        }

        // TO DO: This is a temporary hacky flag to prevent re-entrant updates. What we should be doing instead is
        // never adding to our own wrapped m_scripts collection unless we receive an update from the underlying
        // MultiScript.
        private bool m_adding = false;

        public void AddNew(string keyword, string elementName)
        {
            m_controller.WorldModel.UndoLogger.StartTransaction(string.Format("Add '{0}' script to '{1}'", keyword, elementName));
            AddNewInternal(keyword, elementName);
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        internal void AddNewInternal(string keyword, string elementName)
        {
            Add(m_controller.ScriptFactory.CreateEditableScript(keyword, elementName), false);
        }

        public IEditableScript this[int index]
        {
            get { return m_scripts[index]; }
        }

        public event EventHandler<EditableScriptsUpdatedEventArgs> Updated;

        public void Remove(int index)
        {
            m_controller.WorldModel.UndoLogger.StartTransaction(string.Format("Remove '{0}' script", m_scripts[index].DisplayString()));
            m_scripts.Remove(m_scripts[index]);
            m_underlyingScript.Remove(index);
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
                Add(m_controller.ScriptFactory.CreateEditableScript(e.AddedScript, m_parent), true);
            }
            if (e.InsertedScript != null)
            {
                Add(m_controller.ScriptFactory.CreateEditableScript(e.InsertedScript, m_parent), e.Index, true);
            }
            if (Updated != null) Updated(this, new EditableScriptsUpdatedEventArgs());

            System.Diagnostics.Debug.Assert(m_underlyingScript.Scripts.Count() == m_scripts.Count);
        }

        private void script_Updated(object sender, EditableScriptUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                Updated(this, new EditableScriptsUpdatedEventArgs((IEditableScript)sender, e));
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
    }
}
