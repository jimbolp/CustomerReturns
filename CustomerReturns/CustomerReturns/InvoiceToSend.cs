using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerReturns
{
    public class InvoiceToSend
    {
        public int CustomerNumber { get; set; }
        public long InvoiceNumber { get; set; }
        public DateTime InvoiceData { get; set; }
        public int Branch { get; set; }
    }
}
