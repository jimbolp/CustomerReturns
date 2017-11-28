using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomerReturns
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public LibraSofiaEntities dbSofia { get; set; }
        public LibraBurgasEntities dbBurgas { get; set; }
        public bool ConnectionEstablished { get; set; } = false;
        public List<InvoiceToSend> invoices = new List<InvoiceToSend>();
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                dbSofia = new LibraSofiaEntities();
                dbBurgas = new LibraBurgasEntities();
                ConnectionEstablished = true;
            }
            catch (DbException)
            {
                ConnectionEstablished = false;
            }
        }

        private void FreeTextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<long> invoiceNumbers = Helper.SplitText(FreeTextTextBox.Text);

            if (invoiceNumbers == null)
                return;
            if (ConnectionEstablished)
            {
                foreach(long invoice in invoiceNumbers)
                {
                    switch (Helper.GetBranch(invoice.ToString()))
                    {
                        case 22:
                            {
                                var sale = Helper.SearchTheDatabase(dbSofia, invoice);
                                if(sale == null)
                                    break;
                                else
                                {
                                    invoices.Add(new InvoiceToSend() { CustomerNumber = Helper.GetControlDigit(sale.CustomerID ?? 0) });
                                }
                                break;
                            }
                        case 23:
                            Helper.SearchTheDatabase(dbBurgas, invoice);
                            break;
                        default:
                            break;
                    }
                }
                //Helper.SearchTheDatabase(dbBurgas);
            }
            //Helper.SplitText(FreeTextTextBox.Text);
        }
    }
}
