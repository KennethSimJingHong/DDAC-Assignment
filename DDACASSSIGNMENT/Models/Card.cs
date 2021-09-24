using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace DDACASSSIGNMENT.Models
{
    public class Card : TableEntity
    {
        public Card() { }
        public Card(String UserId, string CardNO) {
            this.PartitionKey = UserId;
            this.RowKey = CardNO;
        }

        [Required]
        public string cardName { get; set; }

        [Required]
        public string expiryDate { get; set; }

        [Required]
        public string CVV { get; set; }
    }
}
