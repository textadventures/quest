using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebEditor.Models
{
    public class StringDictionary
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public string Attribute { get; set; }
        public string KeyPrompt { get; set; }
        public string Source { get; set; }
        public string SourceExclude { get; set; }
        public IEditableDictionary<string> Value { get; set; }
        public bool GameBook { get; set; }
    }
}