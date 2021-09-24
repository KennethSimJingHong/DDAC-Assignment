using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDACASSSIGNMENT.Models;
using System.Security.Claims;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;

namespace DDACASSSIGNMENT.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CardController : Controller
    {
        public IActionResult Index()
        {
            return View();
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
            CloudTable table = clientagent.GetTableReference("card");

            return table;
        }

        public async Task<IActionResult> Add(Card cards)
        {
            string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //link to the correct storage account first
            CloudTable table = GetCloudTable();
            //create table
            if (!await table.ExistsAsync()) { await table.CreateIfNotExistsAsync(); }
            Card card = new Card(UserId, cards.RowKey);
            card.cardName = cards.cardName;
            card.expiryDate = cards.expiryDate;
            card.CVV = cards.CVV;

            try
            {
                TableOperation insert = TableOperation.Insert(card);
                TableResult insertResult = table.ExecuteAsync(insert).Result;
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.ToString();
            }


            return RedirectToAction(nameof(CardView));

        }

        public async Task<IActionResult> CardView()
        {
            CloudTable table = GetCloudTable();
            if (!await table.ExistsAsync()) { await table.CreateIfNotExistsAsync(); }
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

            return View(cardList);
        }
    
        public async Task <ActionResult> DeleteRecord(string RKey)
        {
            string message = null;
            CloudTable table = GetCloudTable();
            if (!await table.ExistsAsync()) { await table.CreateIfNotExistsAsync(); }
            try
            {
                string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                Card card1 = new Card(UserId, RKey) { ETag = "*"};
                TableOperation delete = TableOperation.Delete(card1);
                await table.ExecuteAsync(delete);
                message = "Card deleted";
            }catch(Exception ex)
            {
                message = "Fail to delete due to" + ex.ToString();
            }
            return RedirectToAction(nameof(CardView));
        }
    }
}
