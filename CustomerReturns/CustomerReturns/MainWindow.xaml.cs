using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
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
        public LibraPlovdivEntities dbPlovdiv { get; set; }
        public LibraVelikoTyrnovoEntities dbVelikoTyrnovo { get; set; }
        public LibraVarnaEntities dbVarna { get; set; }
        public bool ConnectionEstablished { get; set; } = false;
        public List<InvoiceToSend> invoices = new List<InvoiceToSend>();
        public List<long> NotFound { get; set; } = new List<long>();
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                dbSofia = new LibraSofiaEntities();
                dbBurgas = new LibraBurgasEntities();
                dbPlovdiv = new LibraPlovdivEntities();
                dbVelikoTyrnovo = new LibraVelikoTyrnovoEntities();
                dbVarna = new LibraVarnaEntities();
                ConnectionEstablished = true;
            }
            catch (DbException)
            {
                ConnectionEstablished = false;
            }
        }

        private void FreeTextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Thread t = new Thread(() =>
            {
                List<long> invoiceNumbers = Helper.SplitText(FreeTextTextBox.Dispatcher.Invoke(() => FreeTextTextBox.Text));

                if (invoiceNumbers == null)
                    return;
                if (ConnectionEstablished)
                {
                    foreach (long invoice in invoiceNumbers)
                    {
                        Sale sale = null;
                        switch (Helper.GetBranch(invoice.ToString()))
                        {
                            case Branch.Sofia:
                                sale = Helper.SearchTheDatabase(dbSofia, invoice);
                                break;
                            case Branch.Burgas:
                                sale = Helper.SearchTheDatabase(dbBurgas, invoice);
                                break;
                            case Branch.Plovdiv:
                                sale = Helper.SearchTheDatabase(dbPlovdiv, invoice);
                                break;
                            case Branch.VelikoTyrnovo:
                                {
                                    sale = Helper.SearchTheDatabase(dbVelikoTyrnovo, invoice);
                                    if (sale == null)
                                    {
                                        sale = Helper.SearchTheDatabase(dbVarna, invoice);
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                        if (sale == null)
                        {
                            NotFound.Add(invoice);
                        }
                        else
                        {
                            invoices.Add(new InvoiceToSend()
                            {
                                CustomerNumber = Helper.GetControlDigit(sale.CustomerID ?? 0),
                                InvoiceNumber = sale.DocNo,
                                InvoiceDate = sale.DocDate,
                                Branch = (int)Helper.GetBranch(sale.DocNo.ToString())
                            });
                        }
                    }
                }
                if (invoices.Count != 0)
                    CreateEmail();
            });
            t.Start();
        }

        private void CreateEmail()
        {
            string multi = invoices.Count != 1 ? "s" : "";
            Subject = $"Customer Returns - load old invoice{multi} into invoice history";

            Body = $"Please load the following invoice{multi}:" + Environment.NewLine;
            foreach(InvoiceToSend invoice in invoices)
            {
                Body += Environment.NewLine 
                    + invoice;
            }
            if (invoices.Count != 1)
            {
                Body += Environment.NewLine 
                    + Environment.NewLine 
                    + $"The invoices are not available in the invoice history (DKHIS) and have to be reloaded!";
            }
            else
            {
                Body += Environment.NewLine 
                    + Environment.NewLine 
                    + $"The invoice is not available in the invoice history (DKHIS) and has to be reloaded!";
            }

            InfoLabel.Dispatcher.Invoke(() => InfoLabel.Text = "Subject: "
                + Environment.NewLine
                + Subject
                + Environment.NewLine
                + Environment.NewLine
                + "Body: "
                + Environment.NewLine
                + Body);
        }
    }
}
