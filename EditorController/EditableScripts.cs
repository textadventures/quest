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
        private IScript m_underlyingScript;
        private bool m_isMulti = false;
        private Element m_parent;
        private string m_attribute;

        internal EditableScripts(EditorController controller, Element parent, string attribute)
        {
            m_controller = controller;
            m_parent = parent;
            m_scripts = new List<IEditableScript>();
            m_attribute = attribute;
        }

        internal EditableScripts(EditorController controller, IScript script, Element parent, string attribute)
            : this(controller, parent, attribute)
        {
            m_underlyingScript = script;
            MultiScript multiScript = script as MultiScript;
            if (multiScript != null)
            {
                m_isMulti = true;
                multiScript.ScriptUpdated += multiScript_ScriptUpdated;
                foreach (IScript scriptItem in multiScript.Scripts)
                {
                    m_scripts.Add(m_controller.ScriptFactory.CreateScript(scriptItem, parent));
                }
            }
            else
            {
                m_scripts.Add(m_controller.ScriptFactory.CreateScript(script, parent));
            }

            foreach (IEditableScript editableScript in m_scripts)
            {
                editableScript.Updated += script_Updated;
            }
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
            bool setField = false;

            script.Updated += script_Updated;
            if (index.HasValue)
            {
                m_scripts.Insert(index.Value, script);
            }
            else
            {
                m_scripts.Add(script);
            }

            if (m_scripts.Count == 1)
            {
                m_underlyingScript = script.Script;
                UpdateField();
            }
            else
            {
                if (!m_isMulti)
                {
                    // move existing script to multiscript first, then add
                    m_underlyingScript = new MultiScript(m_underlyingScript);
                    m_underlyingScript.ScriptUpdated += multiScript_ScriptUpdated;
                    m_isMulti = true;
                    setField = true;
                }

                MultiScript multiScript = (MultiScript)m_underlyingScript;

                if (!fromUpdate)
                {
                    // Add underlying script to multiscript.
                    // We don't always want to do this - we might be responding
                    // to a multiscript update in the first place so no point adding the same
                    // script again!

                    multiScript.Add(script.Script);
                }

                if (setField) UpdateField();
            }
        }

        private void UpdateField()
        {
            if (m_parent != null) m_parent.Fields.Set(m_attribute, m_underlyingScript);
        }

        public void AddNew(string keyword, string elementName)
        {
            m_controller.WorldModel.UndoLogger.StartTransaction(string.Format("Add '{0}' script to '{1}'", keyword, elementName));
            AddNewInternal(keyword, elementName);
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        internal void AddNewInternal(string keyword, string elementName)
        {
            Add(m_controller.ScriptFactory.CreateScript(keyword, elementName), false);
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
            if (m_isMulti)
            {
                MultiScript multiScript = (MultiScript)m_underlyingScript;
                multiScript.Remove(index);
            }
            else
            {
                // only one script so set field to null
                m_underlyingScript = null;
                UpdateField();
            }
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        #endregion

        #region IDataWrapper Members

        public object GetUnderlyingValue()
        {
            return m_underlyingScript;
        }

        #endregion

        private void multiScript_ScriptUpdated(object sender, ScriptUpdatedEventArgs e)
        {
            // Has the update to the MultiScript removed one of the scripts? If so we need
            // to remove it from this wrapper too.
            if (e.RemovedScript != null)
            {
                foreach (IEditableScript es in m_scripts.ToArray())
                {
                    EditableScript s = (EditableScript)es;
                    if (s.Script == e.RemovedScript) m_scripts.Remove(es);
                }
            }
            if (e.AddedScript != null)
            {
                Add(m_controller.ScriptFactory.CreateScript(e.AddedScript, m_parent), true);
            }
            if (e.InsertedScript != null)
            {
                Add(m_controller.ScriptFactory.CreateScript(e.InsertedScript, m_parent), e.Index, true);
            }
            if (Updated != null) Updated(this, new EditableScriptsUpdatedEventArgs());
        }

        private void script_Updated(object sender, EditableScriptUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                Updated(this, new EditableScriptsUpdatedEventArgs((IEditableScript)sender, e));
            }
        }
    }
}
