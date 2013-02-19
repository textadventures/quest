using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebEditor.Models.Controls
{
    public class RichTextControl
    {
        public class TextProcessorCommand
        {
            public string Command { get; set; }
            public string InsertBefore { get; set; }
            public string InsertAfter { get; set; }
            public string Info { get; set; }
            public string Source { get; set; }
            public string Extensions { get; set; }
        }

        public IEditorControl Control { get; set; }
        public string Value { get; set; }
        public IEnumerable<TextProcessorCommand> TextProcessorCommands { get; set; }
    }
}