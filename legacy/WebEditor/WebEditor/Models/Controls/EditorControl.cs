using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebEditor.Models.Controls
{
    public class EditorControl
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public IEditorData EditorData { get; set; }
        public IEditorControl Control { get; set; }
        public EditorController Controller { get; set; }
        public bool IsFirst { get; set; }
        public string ControlType { get; set; }
        public string Caption { get; set; }
    }
}