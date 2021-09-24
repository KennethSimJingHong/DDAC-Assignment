using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace DDACASSSIGNMENT.Models
{
    public class Operation:TableEntity
    {
        public Operation() { }

        public Operation(string UserId, string OrderId)
        {
            this.PartitionKey = UserId;
            this.RowKey = OrderId;
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public string Group { get; set; }

        [Required]
        public DateTime OperationDate { get; set; }

        [Required]
        public double TotalPrice { get; set; }

        public int Duration { get; set; }
    }
}
