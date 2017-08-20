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
    public partial class CompassControl : UserControl
    {
        private CompassDirectionControl[] m_directionControl = new CompassDirectionControl[12];
        private int? m_selectedDirection = null;

        public event Action<string> HyperlinkClicked;
        public event Action<int> SelectionChanged;

        public CompassControl()
        {
            InitializeComponent();
            m_directionControl[0] = dirNW;
            m_directionControl[1] = dirN;
            m_directionControl[2] = dirNE;
            m_directionControl[3] = dirW;
            m_directionControl[4] = dirE;
            m_directionControl[5] = dirSW;
            m_directionControl[6] = dirS;
            m_directionControl[7] = dirSE;
            m_directionControl[8] = dirUp;
            m_directionControl[9] = dirDown;
            m_directionControl[10] = dirIn;
            m_directionControl[11] = dirOut;

            foreach (CompassDirectionControl ctl in m_directionControl)
            {
                ctl.HyperlinkClicked += ctl_HyperlinkClicked;
                ctl.MouseDown += ctl_MouseDown;
            }
        }

        void ctl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CompassDirectionControl ctl = (CompassDirectionControl)sender;
            int dirIndex = ctl.Direction;
            SelectedDirection = dirIndex;
            if (SelectionChanged != null) SelectionChanged(dirIndex);
        }

        void ctl_HyperlinkClicked(string destination)
        {
            if (HyperlinkClicked != null) HyperlinkClicked(destination);
        }

        public int? SelectedDirection
        {
            get { return m_selectedDirection; }
            set
            {
                if (m_selectedDirection.HasValue)
                {
                    m_directionControl[m_selectedDirection.Value].IsSelected = false;
                }
                m_selectedDirection = value;
                if (m_selectedDirection.HasValue)
                {
                    m_directionControl[m_selectedDirection.Value].IsSelected = true;
                }
            }
        }

        public void Clear()
        {
            foreach (CompassDirectionControl ctl in m_directionControl)
            {
                ctl.NoLinkDestination = "(none)";
            }
            SelectedDirection = null;
        }

        public void Populate(int direction, string to, bool lookonly)
        {
            if (!lookonly)
            {
                m_directionControl[direction].HyperlinkDestination = to;
            }
            else
            {
                m_directionControl[direction].NoLinkDestination = "look";
            }
        }
    }
}
