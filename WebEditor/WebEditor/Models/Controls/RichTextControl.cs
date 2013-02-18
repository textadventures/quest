using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebEditor.Models.Controls
{
    public class RichTextControl
    {
        public IEditorControl Control { get; set; }
        public string Value { get; set; }
    }
}