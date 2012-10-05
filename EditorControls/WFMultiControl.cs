using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextAdventures.Quest.EditorControls
{
    public partial class WFMultiControl : UserControl
    {
        public event EventHandler<DataModifiedEventArgs> Dirty;
        public event Action RequestParentElementEditorSave;

        public WFMultiControl()
        {
            InitializeComponent();

            wpfMultiControl.Dirty += wpfMultiControl_Dirty;
            wpfMultiControl.RequestParentElementEditorSave += wpfMultiControl_RequestParentElementEditorSave;
        }

        void wpfMultiControl_Dirty(object sender, DataModifiedEventArgs e)
        {
            Dirty(sender, e);
        }

        void wpfMultiControl_RequestParentElementEditorSave()
        {
            RequestParentElementEditorSave();
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            wpfMultiControl.DoInitialise(controller, definition);
        }

        public void Populate(IEditorData data)
        {
            wpfMultiControl.Populate(data);
        }

        public void Save()
        {
            wpfMultiControl.Save();
        }
    }
}
