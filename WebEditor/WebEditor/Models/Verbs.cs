using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;

namespace WebEditor.Models
{
    public class Verbs
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public IEditorControl EditorControl { get; set; }
        public EditorController Controller { get; set; }
    }
}