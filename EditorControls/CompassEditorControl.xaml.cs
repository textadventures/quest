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
    public partial class CompassEditorControl : UserControl
    {
        public enum CompassEditorMode
        {
            NoSelection,
            NotACompassExit,
            NewCompassExit,
            ExistingCompassExit
        }

        public class CreateExitEventArgs : EventArgs
        {
            public string To { get; set; }
            public string Direction { get; set; }
            public bool LookOnly { get; set; }
            public bool CreateInverse { get; set; }
        }

        private CompassEditorMode m_mode;
        private bool m_allowCreateInverse;
        private bool m_correspondingExitExists;
        private bool m_simpleMode;

        public event EventHandler<CreateExitEventArgs> CreateExit;
        public event Action<string> EditExit;
        public event Action CreateInverseExit;
        public event EventHandler<CreateExitEventArgs> CheckInverseExit;

        public CompassEditorControl()
        {
            InitializeComponent();
        }

        public CompassEditorMode Mode
        {
            get { return m_mode; }
            set
            {
                m_mode = value;
                UpdateControlVisibility();
            }
        }

        public bool SimpleMode
        {
            get { return m_simpleMode; }
            set
            {
                m_simpleMode = value;
                UpdateControlVisibility();
            }
        }

        private void UpdateControlVisibility()
        {
            if (m_mode == CompassEditorMode.NoSelection || m_mode == CompassEditorMode.NotACompassExit)
            {
                createExitTitle.Visibility = Visibility.Collapsed;
                toLabel.Visibility = Visibility.Collapsed;
                to.Visibility = Visibility.Collapsed;
                toName.Visibility = Visibility.Collapsed;
                rectangle.Visibility = Visibility.Collapsed;
                createLookOnlyTitle.Visibility = Visibility.Collapsed;
                createLookOnlyDesc.Visibility = Visibility.Collapsed;
                createLookOnly.Visibility = Visibility.Collapsed;
                create.Visibility = Visibility.Collapsed;
                edit.Visibility = Visibility.Collapsed;
                chkCorresponding.Visibility = Visibility.Collapsed;
                corresponding.Visibility = Visibility.Collapsed;
                createCorresponding.Visibility = Visibility.Collapsed;

                direction.Text = m_mode == CompassEditorMode.NoSelection ? "No exit selected" : "Selected exit is not a compass direction";
            }
            else if (m_mode == CompassEditorMode.NewCompassExit)
            {
                createExitTitle.Visibility = Visibility.Visible;
                toLabel.Visibility = Visibility.Visible;
                to.Visibility = Visibility.Visible;
                toName.Visibility = Visibility.Collapsed;
                rectangle.Visibility = m_simpleMode ? Visibility.Collapsed : Visibility.Visible;
                createLookOnlyTitle.Visibility = m_simpleMode ? Visibility.Collapsed : Visibility.Visible;
                createLookOnlyDesc.Visibility = m_simpleMode ? Visibility.Collapsed : Visibility.Visible;
                createLookOnly.Visibility = m_simpleMode ? Visibility.Collapsed : Visibility.Visible;
                create.Visibility = Visibility.Visible;
                edit.Visibility = Visibility.Collapsed;
                chkCorresponding.Visibility = Visibility.Visible;
                corresponding.Visibility = Visibility.Collapsed;
                createCorresponding.Visibility = Visibility.Collapsed;
            }
            else if (m_mode == CompassEditorMode.ExistingCompassExit)
            {
                createExitTitle.Visibility = Visibility.Collapsed;
                toLabel.Visibility = Visibility.Visible;
                to.Visibility = Visibility.Collapsed;
                toName.Visibility = Visibility.Visible;
                rectangle.Visibility = Visibility.Collapsed;
                createLookOnlyTitle.Visibility = Visibility.Collapsed;
                createLookOnlyDesc.Visibility = Visibility.Collapsed;
                createLookOnly.Visibility = Visibility.Collapsed;
                create.Visibility = Visibility.Collapsed;
                edit.Visibility = Visibility.Visible;
                chkCorresponding.Visibility = Visibility.Collapsed;

                // depends on where the exit points
                corresponding.Visibility = Visibility.Visible;
                createCorresponding.Visibility = Visibility.Visible;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public string DirectionName { get; set; }
        public string ExitID { get; set; }
        public string Destination { get; set; }

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            EditExit(ExitID);
        }

        private void to_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            create.IsEnabled = true;
            CheckInverseExit(this, new CreateExitEventArgs
            {
                To = (string)to.SelectedItem,
                Direction = DirectionName
            });
        }

        private void create_Click(object sender, RoutedEventArgs e)
        {
            CreateExit(this, new CreateExitEventArgs {
                To = (string)to.SelectedItem,
                Direction = DirectionName,
                LookOnly = false,
                CreateInverse = chkCorresponding.IsEnabled && chkCorresponding.IsChecked.Value
            });
        }

        private void createLookOnly_Click(object sender, RoutedEventArgs e)
        {
            CreateExit(this, new CreateExitEventArgs
            {
                Direction = DirectionName,
                LookOnly = true,
            });
        }

        public bool AllowCreateInverseExit
        {
            get { return m_allowCreateInverse; }
            set
            {
                m_allowCreateInverse = value;
                chkCorresponding.IsEnabled = value;
                createCorresponding.IsEnabled = value;
            } 
        }

        public bool CorrespondingExitExists
        {
            get { return m_correspondingExitExists; }
            set
            {
                m_correspondingExitExists = value;
                corresponding.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                createCorresponding.Visibility = (value || m_mode == CompassEditorMode.NewCompassExit) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void createCorresponding_Click(object sender, RoutedEventArgs e)
        {
            CreateInverseExit();
        }
    }
}
