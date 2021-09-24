using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [Required]
        public int SizeId { get; set; }

        [ForeignKey("SizeId")]
        public Size Size { get; set; }

        public int? PeriodId { get; set; }

        [ForeignKey("PeriodId")]
        public Period Period { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
