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
    public partial class FindControl : UserControl
    {
        public event Action<string> Find;
        public event Action Close;

        public FindControl()
        {
            InitializeComponent();
        }

        private void cmdFind_Click(object sender, RoutedEventArgs e)
        {
            DoFind();
        }

        private void cmdClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DoFind()
        {
            Find(txtFind.Text);
        }

        private void txtFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoFind();
            }
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
