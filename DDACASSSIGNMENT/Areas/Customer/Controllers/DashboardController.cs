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

namespace DDACASSSIGNMENT.Areas.Customer.Controllers
{
    [Area("Customer")]
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

        // GET: Customer/Dashboard
        public async Task<IActionResult> Index()
        {
            CloudTable table = GetCloudTable();
            if (!await table.ExistsAsync()) { await table.CreateIfNotExistsAsync(); }
            await table.CreateIfNotExistsAsync();
            string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var obj = _context.Orders.Include(o => o.ApplicationUser).Include(o => o.Service).ThenInclude(s => s.Period)
   .Include(o => o.Service).ThenInclude(s => s.Category)
   .Include(o => o.Service).ThenInclude(s => s.Size).Where(o => o.UserId.CompareTo(UserId) == 0);
            List<Order> order = await obj.ToListAsync();
            TableQuery<Operation> query = new TableQuery<Operation>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, UserId));
            List<Operation> operationList = new List<Operation>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<Operation> operations = table.ExecuteQuerySegmentedAsync(query, token).Result;
                token = operations.ContinuationToken;
                foreach (Operation operation in operations.Results)
                {
                    operation.Order = order.Where(o => operation.RowKey.CompareTo(o.Id.ToString()) == 0).First();
                    operationList.Add(operation);
                }
            } while (token != null);

            return View(operationList);
        }

    }
}
