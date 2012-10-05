using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebEditor.Models
{
    public class ScriptDictionary
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public string Attribute { get; set; }
        public string KeyPrompt { get; set; }
        public string Source { get; set; }
        public IEditableDictionary<IEditableScripts> Value { get; set; }
    }
}