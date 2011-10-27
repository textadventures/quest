using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebEditor.Models
{
    public class Game
    {
        public class TreeItem
        {
            public string Key { get; set; }
            public string Text { get; set; }
            public IEnumerable<TreeItem> Children { get; set; }
        }

        public string Name { get; set; }
        public IEnumerable<TreeItem> Elements { get; set; }
    }
}