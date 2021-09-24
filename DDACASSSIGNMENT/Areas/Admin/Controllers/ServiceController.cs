using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Models.ViewModel;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id) //used for either create or edit // if got id then is edit otherwise create
        {
            ServiceViewModel serviceVM = new ServiceViewModel()
            {
                Service = new Service(),
                CategoryList = _context.Categories.ToList().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                PeriodList = _context.Periods.ToList().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                SizeList = _context.Sizes.ToList().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if (id == null)
            {
                return View(serviceVM);
            }
            serviceVM.Service = _context.Services.Find(id.GetValueOrDefault());

            if (serviceVM.Service == null)
            {
                return NotFound();
            }
            return View(serviceVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ServiceViewModel serviceViewModel)
        {
            if (ModelState.IsValid)
            {

                if (serviceViewModel.Service.Id == 0)
                {
                    _context.Add(serviceViewModel.Service);
                }
                else
                {
                    _context.Update(serviceViewModel.Service);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            
            
                ServiceViewModel serviceVM = new ServiceViewModel()
                {
                    Service = new Service(),
                    CategoryList = _context.Categories.ToList().Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    PeriodList = _context.Periods.ToList().Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    SizeList = _context.Sizes.ToList().Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    })
                };
                return View(serviceVM);
            
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _context.Services.Include(c => c.Category).Include(p => p.Period).Include(s => s.Size).ToList();
            //var allObj = _context.Services.Where(x => x.CategoryId == id)

            return Json(new { data = allObj });
            
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _context.Services.Find(id);
            if (objFromDb != null)
            {
                _context.Services.Remove(objFromDb);
                _context.SaveChanges();
                return Json(new { success = true, message = "Delete Successfully" });
            }
            return Json(new { success = false, message = "Failed to delete service" });
        }

        #endregion
    }
}
