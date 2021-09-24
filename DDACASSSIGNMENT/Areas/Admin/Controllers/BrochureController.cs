using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Customer)]
    public class BrochureController : Controller
    {

        private readonly ApplicationDbContext _context;

        public BrochureController(ApplicationDbContext context)
        {
            _context = context;
        }

        //1. create a function should connect to the correct storage account and container
        private CloudBlobContainer GetCloudBlobContainer()
        {
            //step 1: read appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();

            //step 2: to get key access from the appsettings.json and put inside the code
            CloudStorageAccount accountdetails = CloudStorageAccount.Parse(configure["ConnectionStrings:blobstorageconnection"]);

            //step 3: how to refer to an existing /new container in the blob storage account.
            CloudBlobClient clientagent = accountdetails.CreateCloudBlobClient();
            CloudBlobContainer container = clientagent.GetContainerReference("brochures");
            return container;
        }

        public IActionResult Index()
        {
            var allObj = _context.Brochures.ToList();
            return View(allObj);
        }

        public IActionResult Insert() 
        {
            Brochure brochure = new Brochure();

            return View(brochure);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Insert(Brochure brochure)
        {
            //link to the correct storage account first
            CloudBlobContainer container = GetCloudBlobContainer();
            //create container
            if (!await container.ExistsAsync()) { await container.CreateIfNotExistsAsync(); }
            CloudBlockBlob blobitem = null;
            var files = HttpContext.Request.Form.Files;

            try
            {
                blobitem = container.GetBlockBlobReference(brochure.Name + ".jpg");
                var stream = files[0].OpenReadStream();
                blobitem.UploadFromStreamAsync(stream).Wait();
                brochure.ImageUrl = blobitem.Uri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

             _context.Add(brochure);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            Console.Write(id);
            Brochure brochure = _context.Brochures.Find(id);

            CloudBlobContainer container = GetCloudBlobContainer();
            CloudBlockBlob blob = container.GetBlockBlobReference(brochure.Name + ".jpg");

            try
            {
                string name = blob.Name;
                blob.DeleteIfExistsAsync().Wait();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            _context.Brochures.Remove(brochure);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(string name)
        {
            CloudBlobContainer container = GetCloudBlobContainer();
            CloudBlockBlob blob;
            try
            {
                await using(MemoryStream memoryStream = new MemoryStream())
                {
                    blob = container.GetBlockBlobReference(name + ".jpg");
                    await blob.DownloadToStreamAsync(memoryStream);
                }
                Stream blobStream = blob.OpenReadAsync().Result;
                return File(blobStream, blob.Properties.ContentType, blob.Name);
            }catch(Exception ex)
            {
                Console.Write(ex);
            }
            return RedirectToAction(nameof(Index));
        }
        
    }
}
