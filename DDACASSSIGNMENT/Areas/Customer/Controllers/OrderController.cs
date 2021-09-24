using DDACASSSIGNMENT.Data;
using DDACASSSIGNMENT.Models;
using DDACASSSIGNMENT.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> PaymentAsync(int id)
        {
            Operation operation = new Operation()
            {
                Order = new Order()
                {
                    Service = new Service()
                },
            };

            operation.Order.Service = _context.Services.Include(c => c.Category).Include(p => p.Period).Include(s => s.Size)
                .FirstOrDefault(x => x.Id == id);

            if (operation.Order.Service == null)
            {
                return NotFound();
            }

            CloudTable table = GetCardCloudTable();
            if (!await table.ExistsAsync())
            {
                await table.CreateIfNotExistsAsync();
            }
            string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            TableQuery<Card> query = new TableQuery<Card>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, UserId));
            List<Card> cardList = new List<Card>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<Card> cards = table.ExecuteQuerySegmentedAsync(query, token).Result;
                token = cards.ContinuationToken;
                foreach (Card card in cards.Results)
                {
                    cardList.Add(card);
                }
            } while (token != null);

            PaymentViewModel paymentViewModel = new PaymentViewModel()
            {
                Operation = operation,
                CardList = cardList
            };

            return View(paymentViewModel);
        }

        private CloudTable GetCardCloudTable()
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
            CloudTable table = clientagent.GetTableReference("card");

            return table;
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

        [HttpPost]
        public async Task<IActionResult> Purchase(PaymentViewModel paymentViewModel)
        {
            string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Order order = new Order();
            order.ServiceId = paymentViewModel.Operation.Order.Service.Id;
            order.UserId = UserId;
            _context.Add(order);
            _context.SaveChanges();

            //link to the correct storage account first
            CloudTable table = GetCloudTable();
            //create table
            if (!await table.ExistsAsync()) { await table.CreateIfNotExistsAsync(); }
            Operation operation = new Operation(UserId, order.Id.ToString());
            operation.Id = order.Id;
            operation.Group = "No Group";

            if (paymentViewModel.Operation.Order.Service.Type.CompareTo("Extra") != 0)
            {
                if (paymentViewModel.Operation.Duration == 0)
                {
                    paymentViewModel.Operation.Duration = 1;
                }
                operation.Duration = paymentViewModel.Operation.Duration;
                operation.TotalPrice = paymentViewModel.Operation.Order.Service.Price * paymentViewModel.Operation.Duration;
            }
            else {
                operation.TotalPrice = paymentViewModel.Operation.Order.Service.Price;
            }
            
            operation.OperationDate = DateTime.Now;

            try
            {
                TableOperation insert = TableOperation.Insert(operation);
                TableResult insertResult = table.ExecuteAsync(insert).Result;
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.ToString();
            }


            return RedirectToAction(nameof(Index));

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _context.Services.Include(c => c.Category).Include(p => p.Period).Include(s => s.Size).ToList();

            return Json(new { data = allObj });

        }

        #endregion
    }
}
