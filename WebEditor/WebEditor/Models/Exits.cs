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
            public string ElementId { get; set; }
            public string Name { get; set; }
            public string InverseName { get; set; }
            public string To { get; set; }
            public string DirectionType { get; set; }
            public string InverseDirectionType { get; set; }
        }

        public string Id { get; set; }
        public List<CompassDirection> Directions { get; set; }
        public List<string> Objects { get; set; }
        public bool CreateInverse { get; set; }
    }
}