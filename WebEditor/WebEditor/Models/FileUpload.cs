using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebEditor.Models
{
    public class FileUpload
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public string Attribute { get; set; }
        [Required]
        public HttpPostedFileBase File { get; set; }

        public string AllFiles { get; set; }
    }
}