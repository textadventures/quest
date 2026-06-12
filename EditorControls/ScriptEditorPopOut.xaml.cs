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
            if (System.Windows.Forms.Application.OpenForms.Count > 0)
                this.FontSize = System.Windows.Forms.Application.OpenForms[0].Font.SizeInPoints * 96.0 / 72.0;

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
