using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Models.ViewModel
{
    public class OperationViewModel
    {
        [Required(ErrorMessage = "Please select an option")]
        public Operation Operation { get; set; }

        public List<Operation> OperationList { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public IEnumerable<SelectListItem> GroupList { get; set; }
    }

}
