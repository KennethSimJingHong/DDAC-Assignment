using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using DDACASSSIGNMENT.Models;
using System.Security.Claims;
using DDACASSSIGNMENT.Data;

namespace DDACASSSIGNMENT.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly ApplicationDbContext _context;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
        }

        public CloudTable GetCloudTable()
        {
            // step 1: read appsettings:json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();

            //step 2: to get key access from the appsettings.json and put inside the code
            CloudStorageAccount accountdetails = CloudStorageAccount.Parse(configure["ConnectionStrings:blobstorageconnection"]);

            //step 3: how to refer to an existing / new container in the blob storage account.
            CloudTableClient clientagent = accountdetails.CreateCloudTableClient();
            CloudTable table = clientagent.GetTableReference("logs");

            return table;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var data = _context.ApplicationUsers.Find(UserId);

                //link to the correct storage account first
                CloudTable table = GetCloudTable();
                //create table
                if (!await table.ExistsAsync()) { await table.CreateIfNotExistsAsync(); }
                DateTime now = DateTime.Now;
                string dt = now.ToString("MMddyyyyHHmmss");
                Log log = new Log(data.Email, dt);
                log.Status = "LOG OUT";

                try
                {
                    TableOperation insert = TableOperation.Insert(log);
                    TableResult insertResult = table.ExecuteAsync(insert).Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return LocalRedirect(returnUrl);
            }
            else
            {

                return RedirectToPage();
            }
        }
    }
}
