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
            m_defaultBackground = new SolidColorBrush(Colors.White);

            m_mouseOverBackground = new LinearGradientBrush(
                new GradientStopCollection {
                    new GradientStop(Color.FromRgb(0xF0, 0xFA, 0xFE), 0.0),
                    new GradientStop(Color.FromRgb(0xD6, 0xF0, 0xFF), 1.0)},
                new Point(0.0, 0.0), new Point(0.0, 1.0));

            m_selectedBackground = new LinearGradientBrush(
                new GradientStopCollection {
                                new GradientStop(Color.FromRgb(0xE3, 0xF4, 0xFC), 0.0),
                                new GradientStop(Color.FromRgb(0xD8, 0xEF, 0xFC), 0.38),
                                new GradientStop(Color.FromRgb(0xBE, 0xE6, 0xFD), 0.38),
                                new GradientStop(Color.FromRgb(0xA6, 0xD9, 0xF4), 1.0)},
                new Point(0.0, 0.0), new Point(0.0, 1.0));

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
                }
                else
                {
                    this.Background = IsMouseOver ? m_mouseOverBackground : m_defaultBackground;
                }
            }
        }

        private static void OnDirectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // 0 = NW, 1 = N, 2 = NE
            // 3 = W , 4 = O, 5 = E     9 = U, 10 = D
            // 6 = SW, 7 = S, 8 = SE
            // Directions 0-3, 5-8 in Wingdings ãáäß àåâæ
            // Direction 4 = "out"
            // Directions 9-10 in Marlett

            CompassDirectionControl ctl = (CompassDirectionControl)sender;

            int dir = (int)e.NewValue;
            if ((dir >= 0 && dir <= 3) || (dir >= 5 && dir <= 8))
            {
                ctl.direction.FontFamily = new FontFamily("Wingdings");
                ctl.direction.Text = "ãáäß àåâæ".Substring(dir, 1);
            }
            else if (dir == 4)
            {
                ctl.direction.Text = "out";
            }
            else if (dir >= 9 && dir <= 10)
            {
                ctl.direction.FontFamily = new FontFamily("Marlett");
                ctl.direction.Text = "tu".Substring(dir - 9, 1);
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
