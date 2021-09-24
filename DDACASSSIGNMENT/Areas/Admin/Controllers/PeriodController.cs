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
    public class PeriodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PeriodController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id) //used for either create or edit // if got id then is edit otherwise create
        {
            Period period = new Period();
            if (id == null)
            {
                return View(period);
            }
            period = _context.Periods.Find(id.GetValueOrDefault());

            if (period == null)
            {
                return NotFound();
            }
            return View(period);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Period period)
        {
            if (ModelState.IsValid)
            {
                if (period.Id == 0)
                {
                    _context.Add(period);
                }
                else
                {
                    _context.Update(period);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(period);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _context.Periods.ToList();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _context.Periods.Find(id);
            if (objFromDb != null)
            {
                _context.Periods.Remove(objFromDb);
                _context.SaveChanges();
                return Json(new { success = true, message = "Delete Successfully" });
            }
            return Json(new { success = false, message = "Failed to delete period" });
        }

        #endregion
    }
}
