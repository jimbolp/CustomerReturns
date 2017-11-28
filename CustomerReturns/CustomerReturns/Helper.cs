using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CustomerReturns
{
    public static class Helper
    {
        /// <summary>
        /// Splits the text and tries to fill the fields manualy
        /// </summary>
        /// <param name="text"></param>
        public static List<long> SplitText(string text)
        {
            List<long> listInvoices = new List<long>();
            //string patternSplit = @"[(\s+)(\,)\-–;\\\/]|[(\s+)(\,)\-–;\\\/]+";
            //Regex regex = new Regex(patternSplit);
            //string patternCustomer = @"(?:\D|^|\s)(?<CustomerNumber>\d{7,7})(?:\D|$)";
            string patternInvoice = @"(?:\D|^|\s)(?<InvoiceNumber>\d{10,10})(?:\D|$)";
            
            MatchCollection matches = new Regex(patternInvoice).Matches(text);
            //if (match.Success)
            //{
            //    MainWindow mainWindow = Application.Current.Dispatcher.Invoke(() => Application.Current.MainWindow as MainWindow);
            //    mainWindow.CustomerTextBox.Text = match.Groups["CustomerNumber"].Value.ToString();
            //}
            if (matches != null && matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    listInvoices.Add(Int64.Parse(match.Groups["InvoiceNumber"].Value));
                }
                return listInvoices;
            }
            return null;
        }

        public static int GetBranch(string invoiceNo)
        {
            if (invoiceNo.StartsWith("1"))
            {
                return 22;
            }
            else if (invoiceNo.StartsWith("2"))
            {
                return 23;
            }
            else if (invoiceNo.StartsWith("3"))
            {
                return 26;
            }
            else if (invoiceNo.StartsWith("5"))
            {
                return 25;
            }
            return -1;
        }

        public static Sale SearchTheDatabase(object db, long invoice)
        {
            IQueryable<Sale> sale = null;
            try
            {
                sale = (db.GetType().GetProperty("Sales").GetValue(db) as DbSet<Sale>).Where(s => s.DocNo == invoice);
                return sale.FirstOrDefault();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        public static int GetControlDigit(int number)
        {
            if (number <= 0)
                return 0;
            int result = (7 * ((number / 1) % 10) + 6 * ((number / 10) % 10)
            + 5 * ((number / 100) % 10) + 4 * ((number / 1000) % 10)
            + 3 * ((number / 10000) % 10) + 2 * ((number / 100000) % 10));
            if ((result % 11) == 10)
                number = number * 10 + 0;
            else
                number = number * 10 + (result % 11);

            return result;
        }
    }
}
