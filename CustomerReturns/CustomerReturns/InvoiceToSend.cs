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
        public DateTime InvoiceDate { get; set; }
        public int Branch { get; set; }

        public override string ToString()
        {
            return $"Invoice no. [{InvoiceNumber}] from [{InvoiceDate.ToShortDateString()}] for customer [{CustomerNumber}] in branch [{Branch}]";
        }
    }
}
