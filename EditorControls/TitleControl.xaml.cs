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
    [ControlType("title")]
    public partial class TitleControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;

        public TitleControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += m_helper_Initialise;
            m_helper.Options.DisplaysOwnCaption = true;

            gs1.Color = SystemColors.ControlLightLightColor;
            gs2.Color = SystemColors.ControlDarkColor;
            gs3.Color = gs1.Color;
        }

        void m_helper_Initialise()
        {
            label.Content = m_helper.ControlDefinition.Caption;
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
