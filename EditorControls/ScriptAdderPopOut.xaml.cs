using System.Windows;
using TextAdventures.Utility.Language;

namespace TextAdventures.Quest.EditorControls
{
    /// <summary>
    /// Interaction logic for ScriptAdderPopOut.xaml
    /// </summary>
    public partial class ScriptAdderPopOut : Window
    {
        public ScriptAdderPopOut()
        {
            if (System.Windows.Forms.Application.OpenForms.Count > 0)
                this.FontSize = System.Windows.Forms.Application.OpenForms[0].Font.SizeInPoints * 96.0 / 72.0;

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

        private void ScriptAdderWindow_Initialized(object sender, System.EventArgs e)
        {
            ScriptAdderWindow.Title = L.T("EditorScriptAdderWindowTitle");
        }

        private void ctlScriptAdder_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
