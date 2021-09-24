using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DDACASSSIGNMENT.Data;
using Microsoft.AspNetCore.Mvc;

namespace DDACASSSIGNMENT.ViewComponents
{
    public class UserInfoViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public UserInfoViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userFromDb = _context.ApplicationUsers.Find(claims.Value);

            return View(userFromDb);
        }
    }
}
