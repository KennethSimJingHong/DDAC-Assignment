using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        [NotMapped]
        public string Role { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        //public static explicit operator ApplicationUser(Task<IdentityUser> v)
        //{
        //    throw new NotImplementedException();
        //}

        public string ProfilePicture { get; set; }
    }
}
