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
            this.Closing += ScriptAdderPopOut_Closing;
            ctlScriptAdder.AddScript += ctlScriptAdder_AddScript;
            ctlScriptAdder.CloseClicked += ctlScriptAdder_CloseClicked;
        }

        public string SelectedScript { get; set; }

        void ScriptAdderPopOut_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        void ctlScriptAdder_AddScript(string script)
        {
            SelectedScript = script;
            this.Hide();
        }
        
        void ctlScriptAdder_CloseClicked()
        {
            this.Hide();
        }

        void ScriptAdderPopOut_Loaded(object sender, RoutedEventArgs e)
        {
            WindowHelpers.DisableMinimize(this);
        }
    }
}
