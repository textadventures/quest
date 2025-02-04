using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebEditor.Models.Controls
{
    public class MultiControl
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public IEditorData EditorData { get; set; }
        public IEditorControl Control { get; set; }
        public EditorController Controller { get; set; }
        public object Value { get; set; }
    }
}