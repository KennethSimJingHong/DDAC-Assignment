using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Models.ViewModel;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class GroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id) //used for either create or edit // if got id then is edit otherwise create
        {
            Group group = new Group();
            if (id == null)
            {
                return View(group);
            }

            group = _context.Groups.Find(id.GetValueOrDefault());

            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Group group)
        {
            if (ModelState.IsValid)
            {
                if (group.Id == 0)
                {
                    _context.Add(group);
                }
                else
                {
                    _context.Update(group);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(group);
        }

        public IActionResult Manage(int id)
        {
            var objFromDb = _context.Groupings.Where(c => c.GroupId == id).Include(c => c.ApplicationUser).ToList();
            var userList = _context.ApplicationUsers.ToList();
            foreach (var obj in objFromDb)
            {
                var data = userList.Where(c => c.Id == obj.ApplicationUser.Id);
                if (data != null)
                {
                    userList.Remove(obj.ApplicationUser);
                }
            }
            var userRole = _context.UserRoles.ToList(); //mapping between users and roles
            var roles = _context.Roles.ToList();
            foreach (var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }

            var UserList = userList.Where(c => c.Role == SD.Role_Worker).Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            GroupingViewModel groupingViewModel = new GroupingViewModel()
            {
                GroupName = _context.Groups.Where(c => c.Id == id).FirstOrDefault().Name,
                Grouping = new Grouping() {GroupId = id },
                GroupingList = _context.Groupings.Where(c => c.GroupId == id).Include(c => c.ApplicationUser).ToList(),
                UserList = UserList
            };

            return View(groupingViewModel);
        }

        [HttpPost]
        public IActionResult Manage(GroupingViewModel groupingViewModel)
        {

            _context.Add(new Grouping{
                GroupId = groupingViewModel.Grouping.GroupId,
                UserId = groupingViewModel.Grouping.UserId
            });
            _context.SaveChanges();

            return RedirectToAction(nameof(Manage));

        }

        

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _context.Groups.ToList();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _context.Groups.Find(id);
            if (objFromDb != null)
            {
                _context.Groups.Remove(objFromDb);
                _context.SaveChanges();
                return Json(new { success = true, message = "Delete Successfully" });
            }
            return Json(new { success = false, message = "Failed to delete group" });
        }

        [HttpDelete]
        public IActionResult DeleteGrouping(int id)
        {
            var objFromDb = _context.Groupings.Find(id);
            if (objFromDb != null)
            {
                _context.Groupings.Remove(objFromDb);
                _context.SaveChanges();
                return Json(new { success = true, message = "Delete Successfully" });
            }
            return Json(new { success = false, message = "Failed to delete user from group" });
        }

        #endregion
    }
}

//var objFromDb = _context.Groupings.Where(c => c.GroupId == groupingViewModel.Grouping.GroupId).Include(c => c.ApplicationUser).ToList();
//var userList = _context.ApplicationUsers.ToList();
//foreach (var obj in objFromDb)
//{
//    var data = userList.Where(c => c.Id == obj.ApplicationUser.Id);
//    if (data != null)
//    {
//        userList.Remove(obj.ApplicationUser);
//    }
//}
//var userRole = _context.UserRoles.ToList(); //mapping between users and roles
//var roles = _context.Roles.ToList();
//foreach (var user in userList)
//{
//    var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
//    user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
//}

//var UserList = userList.Where(c => c.Role == SD.Role_Worker).Select(i => new SelectListItem
//{
//    Text = i.Name,
//    Value = i.Id.ToString()
//});

//GroupingViewModel groupingVM = new GroupingViewModel()
//{
//    GroupName = _context.Groups.Where(c => c.Id == groupingViewModel.Grouping.GroupId).FirstOrDefault().Name,
//    Grouping = new Grouping() { GroupId = groupingViewModel.Grouping.GroupId },
//    GroupingList = _context.Groupings.Where(c => c.GroupId == groupingViewModel.Grouping.GroupId).Include(c => c.ApplicationUser).ToList(),
//    UserList = UserList
//};

//return View(groupingVM);