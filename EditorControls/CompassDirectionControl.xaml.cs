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
