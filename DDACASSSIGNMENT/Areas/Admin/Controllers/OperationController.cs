using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using DDACASSSIGNMENT.Utility;

namespace DDACASSSIGNMENT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Worker)]
    public class OperationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OperationController(ApplicationDbContext context)
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

        public async Task<IActionResult> Edit(string id)
        {
            CloudTable table = GetCloudTable();
            if (!await table.ExistsAsync())
            {
                await table.CreateIfNotExistsAsync();
            }
            var obj = _context.Orders.Include(o => o.ApplicationUser).Include(o => o.Service).ThenInclude(s => s.Period)
           .Include(o => o.Service).ThenInclude(s => s.Category)
           .Include(o => o.Service).ThenInclude(s => s.Size);
            List<Order> order = await obj.ToListAsync();
            TableQuery<Operation> query = new TableQuery<Operation>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id.Split(" ")[0]))
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id.Split(" ")[1]));
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

            OperationViewModel operationViewModel = new OperationViewModel()
            {
                Operation = new Operation(),
                GroupList = _context.Groups.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                OperationList = operationList
            };

            return View(operationViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OperationViewModel operationViewModel)
        {
                CloudTable table = GetCloudTable();
                TableQuery<Operation> query = new TableQuery<Operation>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, operationViewModel.Operation.Order.ApplicationUser.Id))
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, operationViewModel.Operation.Order.Id.ToString()));
                TableContinuationToken token = null;
                TableQuerySegment<Operation> operations = table.ExecuteQuerySegmentedAsync(query, token).Result;
                token = operations.ContinuationToken;
                Operation op = operations.Results.First();
                
                op.Group = operationViewModel.Operation.Group;
                op.ETag = "*"; 

                try
                {
                    TableOperation updateOperation = TableOperation.Replace(op);
                   
                    await table.ExecuteAsync(updateOperation);
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }

            
            return RedirectToAction("Index");
        }


            #region API CALLS
            [HttpGet]
        public async Task<IActionResult> GetAll(String status)
        {
            CloudTable table = GetCloudTable();
            if (!await table.ExistsAsync())
            {
                await table.CreateIfNotExistsAsync();
            }
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
                    operation.Order = order.Where(o => operation.RowKey.CompareTo(o.Id.ToString()) == 0).First();
                    
                    //operation.Group = group.Where(g => operation.Id.CompareTo(g.Id.ToString()) == 0).First();
                    operationList.Add(operation);
                }
            } while (token != null);

            switch (status)
            {
                case "all":
                    break;
                case "pending":
                    operationList = operationList.Where(o => o.Group == "No Group").ToList();
                    break;
                case "arranged":
                    operationList = operationList.Where(o => o.Group != "No Group").ToList();
                    break;
                default:
                    break;
            }

            return Json(new { data = operationList });
        }
        
        #endregion
    }
}
