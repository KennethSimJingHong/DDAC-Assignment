using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Models.ViewModel;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context
            )
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.ApplicationUsers.ToList();
            var userRole = _context.UserRoles.ToList(); //mapping between users and roles
            var roles = _context.Roles.ToList();
            foreach (var user in users)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                
            }
            var userList = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if(user.Id != User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    userList.Add(user);
                }
            }
            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult ToggleLock([FromBody] string id)
        {
            var objFromDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Failed to lock/unlock" });
            }
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;

            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(100);
            }
            _context.SaveChanges();
            return Json(new { success = true, message = "Operation Successful." });
        }

        [HttpDelete]
        public IActionResult Delete(string id)
        {
            var userFromDb = _context.ApplicationUsers.Find(id);
            if (userFromDb != null)
            {
                _context.ApplicationUsers.Remove(userFromDb);

                _context.SaveChanges();
                return Json(new { success = true, message = "Delete Successfully" });
            }
            return Json(new { success = false, message = "Failed to delete Users" });
        }

        #endregion
    }
}
