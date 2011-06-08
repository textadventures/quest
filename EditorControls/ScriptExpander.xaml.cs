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

namespace AxeSoftware.Quest.EditorControls
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
            if (m_scriptPopulated) return;
            if (m_script != null)
            {
                ctlScript.Populate(m_script);
            }
            else
            {
                ctlScript.Populate(m_data);
            }

            m_scriptPopulated = true;
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            ctlScript.Helper.DoInitialise(controller, definition);
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
            if (data != null)
            {
                // TO DO:
                //ctlScript.IsReadOnly = data.ReadOnly;
            }
            m_script = script;
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
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
    }
}
