using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Models
{
    public class Period
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Period Name")]
        [MaxLength(10)]
        public string Name { get; set; }
    }
}
