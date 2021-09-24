using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Models.ViewModel
{
    public class GroupingViewModel
    {
        public string GroupName { get; set; }

        public Grouping Grouping { get; set; }

        public List<Grouping> GroupingList { get; set; }


        [Required(ErrorMessage = "Please select an option")]
        public IEnumerable<SelectListItem> UserList { get; set; }
    }

}
