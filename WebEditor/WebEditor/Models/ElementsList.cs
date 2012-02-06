using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebEditor.Models
{
    public class ElementsList
    {
        public string Key { get; set; }
        public IDictionary<string, string> Items { get; set; }
        public string ElementType { get; set; }
        public string ObjectType { get; set; }
        public string Filter { get; set; }
    }
}