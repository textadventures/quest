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
    [ControlType("number")]
    public partial class NumberControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<int> m_helper;
        private IEditorData m_data;

        public NumberControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<int>(this);
            m_helper.Initialise += m_helper_Initialise;
        }

        void m_helper_Initialise()
        {
            int? minimum = m_helper.ControlDefinition.GetInt("minimum");
            if (minimum.HasValue)
            {
                ctlNumber.Minimum = minimum;
            }

            int? maximum = m_helper.ControlDefinition.GetInt("maximum");
            if (maximum.HasValue)
            {
                ctlNumber.Maximum = maximum;
            }
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            if (data == null) return;
            m_helper.StartPopulating();
            ctlNumber.Value = m_helper.Populate(data);
            ctlNumber.IsEnabled = m_helper.CanEdit(data) && !data.ReadOnly;
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            // work around a bug where if user types a number but does not press Enter, the
            // number never gets written. So here we force the number control to lose focus
            // so that it saves the currently entered value.
            ctlNumber.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            DoSave();
        }

        private void DoSave()
        {
            if (m_data == null) return;
            if (!m_helper.IsDirty) return;
            int saveValue = ctlNumber.Value.Value;
            m_helper.Save(saveValue);
        }

        private void ctlNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!ctlNumber.Value.HasValue)
            {
                ctlNumber.Value = 0;
            }
            m_helper.SetDirty(ctlNumber.Value.Value);
            DoSave();
        }

        public string StringValue
        {
            get { return ctlNumber.Value.Value.ToString(); }
            set { ctlNumber.Value = int.Parse(value); }
        }

        public Control FocusableControl
        {
            get { return ctlNumber; }
        }
    }
}
