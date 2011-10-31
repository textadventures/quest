using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebEditor.Models
{
    public class StringList
    {
        public string Attribute { get; set; }
        public string EditPrompt { get; set; }
        public IEnumerable<string> Items { get; set; }
    }
}