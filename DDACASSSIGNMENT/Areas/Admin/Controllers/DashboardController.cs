using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;

namespace DDACASSSIGNMENT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private CloudTable GetLogCloudTable()
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

        public async Task<IActionResult> Index()
        {
            CloudTable logTable = GetLogCloudTable();
            if (!await logTable.ExistsAsync()) { await logTable.CreateIfNotExistsAsync(); }
            TableQuery<Log> logQuery = new TableQuery<Log>();
            TableContinuationToken logToken = null;
            TableQuerySegment<Log> logs = logTable.ExecuteQuerySegmentedAsync(logQuery, logToken).Result;
            logToken = logs.ContinuationToken;
            return View(logs.Results);
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



        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = _context.ApplicationUsers.ToList();
            var userRole = _context.UserRoles.ToList(); //mapping between users and roles
            var roles = _context.Roles.ToList();
            var totalUsers = new List<int> { 0, 0, 0 };
            foreach (var user in users)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                var role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                if (role == SD.Role_Admin)
                {
                    totalUsers[0] += 1;
                }
                else if (role == SD.Role_Worker)
                {
                    totalUsers[1] += 1;
                }
                else if (role == SD.Role_Customer)
                {
                    totalUsers[2] += 1;
                }

            }

            List<string> categoryHead = new();
            List<int> categoryTail = new();
            if(_context.Services.ToList().Count != 0)
            {
                var services = _context.Services.Include(c => c.Category).ToList();
                var categories = _context.Categories.ToList();
                foreach (var category in categories)
                {
                    categoryHead.Add(category.Name);
                    categoryTail.Add(0);
                }
                for (int i = 0; i < categoryHead.Count; i++)
                {
                    foreach (var service in services)
                    {
                        if (service.Category.Name == categoryHead[i])
                        {
                            categoryTail[i] += 1;
                        }
                    }
                }
            }


            List<double> earning1 = new() { 0, 0, 0, 0, 0, 0 };
            List<double> earning2 = new() { 0, 0, 0, 0, 0, 0 };

            CloudTable table = GetCloudTable();
            if (!await table.ExistsAsync()) { await table.CreateIfNotExistsAsync(); }
            TableQuery<Operation> query = new TableQuery<Operation>();
            TableContinuationToken token = null;

            TableQuerySegment<Operation> operations = table.ExecuteQuerySegmentedAsync(query, token).Result;
            token = operations.ContinuationToken;
            var year = DateTime.Now.Year;
            foreach (Operation operation in operations.Results)
            {
                if (operation.OperationDate.Year == year)
                {
                    if (operation.OperationDate.Month == 1)
                    {
                        earning1[0] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 2)
                    {
                        earning1[1] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 3)
                    {
                        earning1[2] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 4)
                    {
                        earning1[3] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 5)
                    {
                        earning1[4] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 6)
                    {
                        earning1[5] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 7)
                    {
                        earning2[0] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 8)
                    {
                        earning2[1] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 9)
                    {
                        earning2[2] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 10)
                    {
                        earning2[3] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 11)
                    {
                        earning2[4] += operation.TotalPrice;
                    }
                    else if (operation.OperationDate.Month == 12)
                    {
                        earning2[5] += operation.TotalPrice;
                    }
                }
            }



            return Json(new { data1 = totalUsers, data2 = earning1, data4 = earning2, data31 = categoryHead, data32 = categoryTail});

        }

        #endregion
    }
}
