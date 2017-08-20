using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;

namespace TextAdventures.Quest.EditorControls
{
    public class ControlDataOptions
    {
        public bool DisplaysOwnCaption { get; set; }
        public bool Resizable { get; set; }
        public bool MultipleAttributes { get; set; }
        public bool Scrollable { get; set; }
    }

    public interface IControlDataHelper
    {
        event EventHandler<DataModifiedEventArgs> Dirty;
        event Action RequestParentElementEditorSave;

        Type ExpectedType { get; }
        string Attribute { get; }
        ControlDataOptions Options { get; }

        void DoInitialise(EditorController controller, IEditorControl definition);
        void DoUninitialise();
    }

    public class ControlDataHelper<T> : IControlDataHelper
    {
        public event EventHandler<DataModifiedEventArgs> Dirty;
        public event Action RequestParentElementEditorSave;
        internal event Action Initialise;
        internal event Action Uninitialise;

        public EditorController Controller { get; private set; }
        public IEditorControl ControlDefinition { get; private set; }
        public Type ExpectedType { get; private set; }
        private IElementEditorControl m_parent;
        private IEditorData m_data;
        private bool m_populating;
        private bool m_saving;
        private bool m_dirty;
        private ControlDataOptions m_options = new ControlDataOptions();
        private T m_oldValue;

        internal ControlDataHelper(IElementEditorControl parent)
        {
            m_parent = parent;
            ExpectedType = typeof(T);
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            Controller = controller;
            ControlDefinition = definition;
            if (controller != null && Initialise != null)
            {
                Initialise();
            }
        }

        public void DoUninitialise()
        {
            Controller = null;
            ControlDefinition = null;
            // TO DO: Can't unset m_parent here, as this is passed in the constructor and
            // it is valid for a ControlDataHelper to be reinitialised (in the Attributes control).
            // Need to ensure that removing this line doesn't introduce more memory leaks - may be
            // necessary for more parent controls to listen to the Uninitialise event?
            //m_parent = null;
            m_oldValue = default(T);
            m_data = null;
            if (Uninitialise != null)
            {
                Uninitialise();
            }
        }

        public string Attribute
        {
            get { return ControlDefinition.Attribute; }
        }

        public ControlDataOptions Options
        {
            get { return m_options; }
        }

        internal void SetDirty(T newValue)
        {
            if (m_populating) return;
            if (newValue != null && newValue.Equals(m_oldValue)) return;
            if (newValue == null && m_oldValue == null) return;
            m_dirty = true;
            if (Dirty != null) Dirty(this, new DataModifiedEventArgs(ControlDefinition.Attribute, newValue));
        }

        internal void SaveParent()
        {
            RequestParentElementEditorSave();
        }

        internal T Populate(IEditorData data)
        {
            m_data = data;

            object value = data.GetAttribute(ControlDefinition.Attribute);

            if (value == null) value = default(T);
            if (!(value is T)) value = default(T);

            T result = (T)value;

            m_oldValue = result;
            return result;
        }

        internal bool CanEdit(IEditorData data)
        {
            object value = data.GetAttribute(ControlDefinition.Attribute);
            if (value == null) return true;
            return (value is T);
        }

        internal void Save(T newValue)
        {
            if (m_populating) return;
            if (!m_dirty) return;
            if (m_saving) return;
            m_saving = true;

            // When we call m_data.SetAttribute below, that can trigger off a whole chain of events which may
            // cause us to be Unpopulated before we finish the function. We'll still need to finish the transaction,
            // so we make copies first.
            bool directlySaveable = m_data.IsDirectlySaveable;
            EditorController controller = Controller;
            string caption = ControlDefinition.Caption;
            
            if (directlySaveable)
            {
                controller.StartTransaction(string.Format("Set {0} to '{1}'", ControlDefinition.Caption, newValue == null ? "null" : newValue.ToString()));
            }
            ValidationResult result = m_data.SetAttribute(ControlDefinition.Attribute, newValue);

            if (!result.Valid)
            {
                string errorValue = newValue as string;
                PopupEditors.DisplayValidationError(result, errorValue, string.Format("Unable to set '{0}'", caption));
            }

            if (directlySaveable)
            {
                controller.EndTransaction();
            }
            m_saving = false;

            // Repopulating ensures we see the currently set value, and that dirty=false
            m_parent.Populate(m_data);
        }

        internal void StartPopulating()
        {
            m_populating = true;
        }

        internal void FinishedPopulating()
        {
            m_populating = false;
            m_dirty = false;
        }

        internal bool IsDirty
        {
            get { return m_dirty; }
        }

        internal void RaiseDirtyEvent(object newValue)
        {
            if (m_populating) return;
            Dirty(this, new DataModifiedEventArgs(ControlDefinition == null ? null : ControlDefinition.Attribute, newValue));
        }

        internal void RaiseRequestParentElementEditorSaveEvent()
        {
            if (RequestParentElementEditorSave != null) RequestParentElementEditorSave();
        }

        internal bool IsPopulating
        {
            get { return m_populating; }
        }
    }
}
