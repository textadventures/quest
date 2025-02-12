using System;
using System.Windows;

namespace TextAdventures.Quest.EditorControls
{
    /// <summary>
    /// Interaction logic for ScriptEditorPopOut.xaml
    /// </summary>
    public partial class ScriptEditorPopOut : Window
    {
        public ScriptEditorPopOut()
        {
            InitializeComponent();
            ctlScriptEditor.HidePopOutButton();
            this.Closed += ScriptEditorPopOut_Closed;
            this.Loaded += ScriptEditorPopOut_Loaded;
        }

        void ScriptEditorPopOut_Loaded(object sender, RoutedEventArgs e)
        {
            WindowHelpers.DisableMinimize(this);
        }

        void ScriptEditorPopOut_Closed(object sender, EventArgs e)
        {
            ctlScriptEditor.Save();
        }

        internal ScriptEditorControl ScriptEditor
        {
            get { return ctlScriptEditor; }
        }
    }
}
