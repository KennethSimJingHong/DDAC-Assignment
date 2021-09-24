using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Models.ViewModel
{
    public class PaymentViewModel
    {
        [Required(ErrorMessage = "Please fill in the blank")]
        public Operation Operation { get; set; }


        public IEnumerable<Card> CardList { get; set; }


    }
}
