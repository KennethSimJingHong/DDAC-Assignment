using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDACASSSIGNMENT.Utility
{
    public class EmailModel
    {
        public string To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        //public IFormFile Attachment { get; set; }

        public string FromEmail { get; set; }

        public string FromPassword { get; set; }
    }
}
