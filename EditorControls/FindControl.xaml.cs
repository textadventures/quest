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
    public partial class FindControl : UserControl
    {
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(FindControlMode), typeof(FindControl), new PropertyMetadata(FindControlMode.Find, new PropertyChangedCallback(ModeChanged)));

        public event Action<string, bool> Find;
        public event Action<string, bool, string> Replace;
        public event Action<string, bool, string> ReplaceAll;
        public event Action Close;

        public FindControlMode Mode
        {
            get
            {
                var value = GetValue(ModeProperty);

                return value != null && value is FindControlMode ? (FindControlMode)value : FindControlMode.Invalid;
            }
            set
            {
                SetValue(ModeProperty, value);
            }
        }

        public FindControl()
        {
            InitializeComponent();
        }

        private void cmdFind_Click(object sender, RoutedEventArgs e)
        {
            DoFind();
        }

        private void cmdReplace_Click(object sender, RoutedEventArgs e)
        {
            DoReplace();
        }

        private void cmdReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            DoReplaceAll();
        }

        private void cmdClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void txtReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoReplace();
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void txtFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoFind();
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void DoFind()
        {
            if (Find != null)
                Find(txtFind.Text, chkBoxRegex.IsChecked.Value);
        }

        private void DoReplace()
        {
            if (Replace != null)
                Replace(txtFind.Text, chkBoxRegex.IsChecked.Value, txtReplace.Text);
        }

        private void DoReplaceAll()
        {
            if (ReplaceAll != null)
                ReplaceAll(txtFind.Text, chkBoxRegex.IsChecked.Value, txtReplace.Text);
        }

        private static void ModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var me = obj as FindControl;

            Visibility vis;
            GridLength len;
            if (me.Mode.HasFlag(FindControlMode.Replace))
            {
                vis = Visibility.Visible;
                len = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                vis = Visibility.Collapsed;
                len = new GridLength(0, GridUnitType.Pixel);
            }

            me.lblReplace.Visibility = vis;
            me.txtReplace.Visibility = vis;
            me.cmdReplace.Visibility = vis;
            me.cmdReplaceAll.Visibility = vis;
            me.InputGrid.ColumnDefinitions[3].Width = len;
        }
    }

    [Flags]
    public enum FindControlMode
    {
        Find = 1,
        Replace = 2,
        Invalid = 0
    }
}
