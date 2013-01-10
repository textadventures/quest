using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("scriptexpander")]
    public partial class ScriptExpander : UserControl, IElementEditorControl
    {
        private IEditableScripts m_script;
        private IEditorData m_data;
        private bool m_scriptPopulated = false;

        public event Action Delete;

        public ScriptExpander()
        {
            InitializeComponent();
            ShowDeleteButton = false;
            ctlScript.Initialise += ctlScript_Initialise;
        }

        private void ctlExpander_Expanded(object sender, RoutedEventArgs e)
        {
            PopulateScript();
        }

        private void PopulateScript()
        {
            if (m_scriptPopulated) return;
            if (m_script != null)
            {
                ctlScript.Populate(m_script);
                m_scriptPopulated = true;
            }
            else if (m_data != null)
            {
                ctlScript.Populate(m_data);
                m_scriptPopulated = true;
            }
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            ctlScript.Helper.DoInitialise(controller, definition);
        }

        public void DoUninitialise()
        {
            ctlScript.Helper.DoUninitialise();
        }

        void ctlScript_Initialise()
        {
            if (ctlScript.Definition != null)
            {
                string caption = ctlScript.Definition.Caption;
                if (caption != null)
                {
                    header.Text = caption;
                }
            }
        }
        
        // Note there are two "Populate" methods, so EITHER m_script OR m_data will be set.

        public void Populate(IEditorData data, IEditableScripts script)
        {
            m_script = script;
            if (script == null)
            {
                ctlScript.Populate(script);
            }
            else if (ctlExpander.IsExpanded)
            {
                PopulateScript();
            }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            if (data == null)
            {
                ctlScript.Populate(data);
            }
            else if (ctlExpander.IsExpanded)
            {
                PopulateScript();
            }
        }

        public bool ReadOnly
        {
            get { return ctlScript.ReadOnly; }
            set
            {
                ctlScript.ReadOnly = value;
                if (ctlScript.ReadOnly)
                {
                    cmdDelete.Visibility = Visibility.Collapsed;
                }
            }
        }

        public IControlDataHelper Helper
        {
            get { return ctlScript.Helper; }
        }

        public void Save()
        {
            ctlScript.Save();
        }

        private void cmdDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        public bool ShowDeleteButton
        {
            get { return cmdDelete.Visibility == Visibility.Visible; }
            set { cmdDelete.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string Header
        {
            get { return header.Text; }
            set { header.Text = value; }
        }

        public Control FocusableControl
        {
            get { return null; }
        }
    }
}
