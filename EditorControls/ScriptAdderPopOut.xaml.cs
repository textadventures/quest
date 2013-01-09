using System.Windows;

namespace TextAdventures.Quest.EditorControls
{
    /// <summary>
    /// Interaction logic for ScriptAdderPopOut.xaml
    /// </summary>
    public partial class ScriptAdderPopOut : Window
    {
        public ScriptAdderPopOut()
        {
            InitializeComponent();
            this.Loaded += ScriptAdderPopOut_Loaded;
        }

        void ScriptAdderPopOut_Loaded(object sender, RoutedEventArgs e)
        {
            WindowHelpers.DisableMinimize(this);
        }
    }
}
