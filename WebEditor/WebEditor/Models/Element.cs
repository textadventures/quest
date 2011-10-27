using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;

namespace WebEditor.Models
{
    public class Element
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public IEditorDefinition EditorDefinition { get; set; }
        public IEditorData EditorData { get; set; }
    }
}