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
    [ControlType("gameid")]
    public partial class GameIdControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;

        public GameIdControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();
            textBox.Text = m_helper.Populate(data);
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (!m_helper.IsDirty) return;
            m_helper.Save(textBox.Text);
        }

        public Control FocusableControl
        {
            get { return textBox; }
        }

        private void cmdGenerate_Click(object sender, RoutedEventArgs e)
        {
            textBox.Text = EditorController.GetNewGameId();
            m_helper.SetDirty(textBox.Text);
            Save();
        }
    }
}
