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
    [ControlType("number")]
    public partial class NumberControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<int> m_helper;

        public NumberControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<int>(this);
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();
            ctlNumber.Value = m_helper.Populate(data);
            ctlNumber.IsEnabled = m_helper.CanEdit(data) && !data.ReadOnly;
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (!m_helper.IsDirty) return;
            int saveValue = ctlNumber.Value.Value;
            m_helper.Save(saveValue);
        }

        private void ctlNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!ctlNumber.Value.HasValue)
            {
                ctlNumber.Value = 0;
                return;
            }
            m_helper.SetDirty(ctlNumber.Value.Value);
            Save();
        }
    }
}
