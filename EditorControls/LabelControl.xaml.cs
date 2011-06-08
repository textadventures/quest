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
    [ControlType("label")]
    public partial class LabelControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;

        public LabelControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
            m_helper.Options.DisplaysOwnCaption = true;
            m_helper.Initialise += m_helper_Initialise;
        }

        void m_helper_Initialise()
        {
            textblock.Text = m_helper.ControlDefinition.Caption;
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
        }

        public void Save()
        {
        }
    }
}
