using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebEditor.Models
{
    public class ElementsListItem
    {
        public string Text { get; set; }
        public string CanDelete { get; set; }
        public string Previous { get; set; }
        public string Next { get; set; }
    }

    public class ElementsList
    {
        public string Key { get; set; }
        public IDictionary<string, ElementsListItem> Items { get; set; }
        public string ElementType { get; set; }
        public string ObjectType { get; set; }
        public string Filter { get; set; }
        public bool FillScreen { get; set; }
    }
}