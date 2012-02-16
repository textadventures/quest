using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebEditor.Models
{
    public class Create
    {
        public IEnumerable<string> AllTypes { get; set; }
        public string SelectedType { get; set; }
        public IEnumerable<string> AllTemplates { get; set; }
        public string SelectedTemplate { get; set; }
        [Required]
        [StringLength(60)]
        [Display(Name="Game name")]
        public string GameName { get; set; }
    }
}