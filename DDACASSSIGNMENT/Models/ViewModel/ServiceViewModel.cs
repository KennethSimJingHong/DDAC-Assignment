using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Models.ViewModel
{
    public class ServiceViewModel
    {
        [Required(ErrorMessage = "Please fill in the blank")]
        public Service Service { get; set; }


        public IEnumerable<SelectListItem> CategoryList { get; set; }


        public IEnumerable<SelectListItem> PeriodList { get; set; }


        public IEnumerable<SelectListItem> SizeList { get; set; }
    }
}
