using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebEditor.Models
{
    public class Exits
    {
        public class CompassDirection
        {
            public string Name { get; set; }
            public string To { get; set; }
        }

        public List<CompassDirection> Directions { get; set; }
    }
}