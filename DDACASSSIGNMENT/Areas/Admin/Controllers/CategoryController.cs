using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace DDACASSSIGNMENT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        //1. create a function where this function should connect to the correct storage account and container
        private CloudBlobContainer GetCloudBlobContainer()
        {
            // step 1: read appsettings:json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();

            //step 2: to get key access from the appsettings.json and put inside the code
            CloudStorageAccount accountdetails = CloudStorageAccount.Parse(configure["ConnectionStrings:blobstorageconnection"]);

            //step 3: how to refer to an existing / new container in the blob storage account.
            CloudBlobClient clientagent = accountdetails.CreateCloudBlobClient();
            CloudBlobContainer container = clientagent.GetContainerReference("categories");

            return container;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id) //used for either create or edit // if got id then is edit otherwise create
        {
            Category category = new Category();
            if (id == null)
            {
                return View(category);
            }
            category = _context.Categories.Find(id.GetValueOrDefault());

            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Category category)
        {
            //link to the correct storage account first
            CloudBlobContainer container = GetCloudBlobContainer();
            //create container
            if (!await container.ExistsAsync()) { await container.CreateIfNotExistsAsync(); }
            CloudBlockBlob blobitem = null;
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                try
                {
                    blobitem = container.GetBlockBlobReference(category.Name + ".jpg");
                    var stream = files[0].OpenReadStream();
                    blobitem.UploadFromStreamAsync(stream).Wait();
                    category.ImageUrl = blobitem.Uri.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                if (category.Id != 0)
                {
                    category.ImageUrl = _context.Categories.AsNoTracking().First(x => x.Id == category.Id).ImageUrl;
                }
            }

            if (category.Id == 0)
            {
                _context.Add(category);
            }
            else
            {
                _context.Update(category);
            }

            _context.SaveChanges();


            return RedirectToAction(nameof(Index));

        }

        

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _context.Categories.ToList();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {

            var objFromDb = _context.Categories.Find(id);
            if (objFromDb != null)
            {
                CloudBlobContainer container = GetCloudBlobContainer();
                CloudBlockBlob blob = container.GetBlockBlobReference(objFromDb.Name + ".jpg");

                try
                {
                    blob.DeleteIfExistsAsync().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                _context.Categories.Remove(objFromDb);
                _context.SaveChanges();
                return Json(new { success = true, message = "Delete Successfully" });
            }

            return Json(new { success = false, message = "Failed to delete category" });
        }

        #endregion
    }
}
