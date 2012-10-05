using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebEditor.Models
{
    public class Script
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public string Attribute { get; set; }
        public EditorController Controller { get; set; }
        public IEditableScripts Scripts { get; set; }
        public List<IEditableScript> SelectedScripts { get; set; }
    }
}