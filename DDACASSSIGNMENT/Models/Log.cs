using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace DDACASSSIGNMENT.Models
{
    public class Log : TableEntity
    {
        public Log() { }
        public Log(String Email, String Now) {
            this.PartitionKey = Email;
            this.RowKey = Now;
        }

        [Required]
        public string Status { get; set; }

    }
}
