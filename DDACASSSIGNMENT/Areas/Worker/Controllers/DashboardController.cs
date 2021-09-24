using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using System.Security.Claims;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using System.IO;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;

namespace DDACASSSIGNMENT.Areas.Worker.Controllers
{
    [Area("Worker")]
    [Authorize(Roles = SD.Role_Worker)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private CloudTable GetCloudTable()
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
            CloudTable table = clientagent.GetTableReference("operations");

            return table;
        }

 
        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            CloudTable table = GetCloudTable();
            if (!await table.ExistsAsync())
            {
                await table.CreateIfNotExistsAsync();
            }
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var groups = _context.Groupings.Include(u => u.ApplicationUser).Include(g => g.Group).ToList();
            var obj = _context.Orders.Include(o => o.ApplicationUser).Include(o => o.Service).ThenInclude(s => s.Period)
           .Include(o => o.Service).ThenInclude(s => s.Category)
           .Include(o => o.Service).ThenInclude(s => s.Size);
            List<Order> order = await obj.ToListAsync();
            TableQuery<Operation> query = new TableQuery<Operation>();
            var operationList = new List<Operation>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<Operation> operations = table.ExecuteQuerySegmentedAsync(query, token).Result;
                token = operations.ContinuationToken;
                foreach (Operation operation in operations.Results)
                {
                    
                    foreach (Grouping group in groups)
                    {
                        if(operation.Group == group.Group.Name && group.ApplicationUser.Id == userId)
                        {
                            operation.Order = order.Where(o => operation.RowKey.CompareTo(o.Id.ToString()) == 0).First();
                            operationList.Add(operation);
                        }
                    }
                    
                }
            } while (token != null);


            return Json(new { data = operationList });

        }
        #endregion

    }
}
