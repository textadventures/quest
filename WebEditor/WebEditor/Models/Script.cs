using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;

namespace WebEditor.Models
{
    public class Script
    {
        public string Attribute { get; set; }
        public EditorController Controller { get; set; }
        public IEditableScripts Scripts { get; set; }
    }
}