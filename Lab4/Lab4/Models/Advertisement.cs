using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lab4.Models
{
    public class Advertisement
    {
        public int Id 
        { 
            get; 
            set; 
        }

        [Required]
        [StringLength(50)]
        [Display(Name = "File Name")]
        public String FileName 
        { 
            get; 
            set; 
        }

        [Required]
        public string BlobKey { get; set; }

        [Required]
        [Url]
        public String Url 
        { 
            get; 
            set; 
        }

        public Community Community 
        { 
            get; 
            set; 
        }
    }
}
