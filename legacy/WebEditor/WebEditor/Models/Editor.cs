using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebEditor.Models
{
    public class Editor
    {
        public int GameId { get; set; }
        public bool SimpleMode { get; set; }
        public string ErrorRedirect { get; set; }
        public int CacheBuster { get; set; }
    }
}