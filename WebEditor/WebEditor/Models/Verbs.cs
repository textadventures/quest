using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebEditor.Models
{
    public class Verbs
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public IEditorControl EditorControl { get; set; }
        public EditorController Controller { get; set; }
        public IEditorDataExtendedAttributeInfo EditorData { get; set; }
    }
}