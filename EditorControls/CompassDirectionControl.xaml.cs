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
    public partial class CompassDirectionControl : UserControl
    {
        public static DependencyProperty DirectionProperty;

        private Brush m_defaultBackground;
        private Brush m_mouseOverBackground;
        private Brush m_selectedBackground;
        private bool m_selected;

        public event Action<string> HyperlinkClicked;

        static CompassDirectionControl()
        {
            DirectionProperty = DependencyProperty.Register("Direction", typeof(int), typeof(CompassDirectionControl),
                new FrameworkPropertyMetadata(-1, new PropertyChangedCallback(OnDirectionChanged)));
        }

        public int Direction
        {
            get { return (int)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public CompassDirectionControl()
        {
            InitializeComponent();
            border.BorderBrush = SystemColors.ControlTextBrush;
            m_defaultBackground = new SolidColorBrush(Colors.White);

            m_mouseOverBackground = new SolidColorBrush(Color.FromRgb(0xBE, 0xE6, 0xFD));
            m_selectedBackground = SystemColors.HighlightBrush;

            this.Background = m_defaultBackground;
            this.MouseEnter += MouseEnterUpdateBackground;
            this.MouseLeave += MouseLeaveUpdateBackground;
        }

        private void MouseEnterUpdateBackground(object sender, MouseEventArgs e)
        {
            if (IsSelected) return;
            this.Background = m_mouseOverBackground;
        }

        private void MouseLeaveUpdateBackground(object sender, MouseEventArgs e)
        {
            if (IsSelected) return;
            this.Background = m_defaultBackground;
        }

        public bool IsSelected
        {
            get { return m_selected; }
            set
            {
                m_selected = value;
                if (value)
                {
                    this.Background = m_selectedBackground;
                    this.Foreground = SystemColors.HighlightTextBrush;
                    border.BorderBrush = SystemColors.HighlightTextBrush;
                }
                else
                {
                    this.Background = IsMouseOver ? m_mouseOverBackground : m_defaultBackground;
                    this.Foreground = SystemColors.ControlTextBrush;
                    border.BorderBrush = SystemColors.ControlTextBrush;
                }
            }
        }

        private static void OnDirectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // 0 = NW, 1 = N, 2 = NE
            // 3 = W ,        4 = E     8 = U, 10 = In
            // 5 = SW, 6 = S, 7 = SE    9 = D, 11 = Out
            // Directions 0-7 in Wingdings ãáäßàåâæ
            // Directions 8-9 in Marlett
            // Direction 10,11 = "in","out"

            CompassDirectionControl ctl = (CompassDirectionControl)sender;

            int dir = (int)e.NewValue;
            if ((dir >= 0 && dir <= 7))
            {
                ctl.direction.FontFamily = new FontFamily("Wingdings");
                ctl.direction.Text = "ãáäßàåâæ".Substring(dir, 1);
            }
            else if (dir >= 8 && dir <= 9)
            {
                ctl.direction.FontFamily = new FontFamily("Marlett");
                ctl.direction.Text = "tu".Substring(dir - 8, 1);
            }
            else if (dir == 10)
            {
                ctl.direction.Text = "in";
            }
            else if (dir == 11)
            {
                ctl.direction.Text = "out";
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public string NoLinkDestination
        {
            get { return noLinkDestination.Text; }
            set
            {
                noLinkDestination.Text = value;
                noLinkDestination.Visibility = Visibility.Visible;
                destination.Visibility = Visibility.Collapsed;
            }
        }

        public string HyperlinkDestination
        {
            get { return ((Run)destinationLink.Inlines.FirstInline).Text; }
            set
            {
                destinationLink.Inlines.Clear();
                destinationLink.Inlines.Add(new Run(value));
                destination.Visibility = Visibility.Visible;
                noLinkDestination.Visibility = Visibility.Collapsed;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (HyperlinkClicked != null) HyperlinkClicked(HyperlinkDestination);
        }
    }
}
