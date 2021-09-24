using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class SizeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SizeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id) //used for either create or edit // if got id then is edit otherwise create
        {
            Size size = new Size();
            if (id == null)
            {
                return View(size);
            }
            size = _context.Sizes.Find(id.GetValueOrDefault());

            if (size == null)
            {
                return NotFound();
            }
            return View(size);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Size size)
        {
            if (ModelState.IsValid)
            {
                if (size.Id == 0)
                {
                    _context.Add(size);
                }
                else
                {
                    _context.Update(size);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(size);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _context.Sizes.ToList();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _context.Sizes.Find(id);
            if (objFromDb != null)
            {
                _context.Sizes.Remove(objFromDb);
                _context.SaveChanges();
                return Json(new { success = true, message = "Delete Successfully" });
            }
            return Json(new { success = false, message = "Failed to delete period" });
        }

        #endregion
    }
}
