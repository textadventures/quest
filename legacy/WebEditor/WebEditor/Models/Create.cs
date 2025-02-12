using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebEditor.Models
{
    public class Create
    {
        [Required]
        [StringLength(60)]
        [Display(Name="Game name")]
        public string GameName { get; set; }

        [Required]
        [Display(Name="Game type")]
        public string SelectedType { get; set; }

        [Required]
        [Display(Name="Language")]
        public string SelectedTemplate { get; set; }

        public IEnumerable<string> AllTypes { get; set; }
        public IEnumerable<string> AllTemplates { get; set; }
    }

    public class CreateSuccess
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}