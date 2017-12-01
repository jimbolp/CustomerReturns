using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public bool FormatReady { get; set; } = false;
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
            
        }
        public IEnumerable<TextRange> GetAllWordRanges(FlowDocument document, string invoice)
        {
            string pattern = invoice;
            TextPointer pointer = document.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, pattern);
                    foreach (Match match in matches)
                    {
                        int startIndex = match.Index;
                        int length = match.Length;
                        TextPointer start = pointer.GetPositionAtOffset(startIndex);
                        TextPointer end = start.GetPositionAtOffset(length);
                        yield return  new TextRange(start, end);
                    }
                }
                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }
        private void FormatBeforeSend()
        {
            if (invoices.Count != 0)
            {
                string multi = invoices.Count != 1 ? "s" : "";
                Subject = $"Customer Returns - load old invoice{multi} into invoice history";

                Body = $"Please load the following invoice{multi}:" + Environment.NewLine;
                foreach (InvoiceToSend invoice in invoices)
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
            }
            InfoLabel.Dispatcher.Invoke(() =>
            {
                InfoLabel.Text = "Subject: "
                  + Environment.NewLine
                  + Subject
                  + Environment.NewLine
                  + Environment.NewLine
                  + "Body: "
                  + Environment.NewLine
                  + Body;

                if (NotFound.Count != 0)
                {
                    InfoLabel.Text += Environment.NewLine + "Следните фактури не бяха намерени в базата: " + Environment.NewLine;
                    foreach (long invoice in NotFound)
                    {
                        InfoLabel.Text += invoice + ", ";
                    }
                    if(InfoLabel.Text.LastIndexOf(',') >= 0)
                        InfoLabel.Text.Remove(InfoLabel.Text.LastIndexOf(','), 1);
                }
            });
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
            FreeTextTextBox.Document = new FlowDocument(); 
        }

        private void ResetFields()
        {
            invoices = new List<InvoiceToSend>();
            NotFound = new List<long>();
            InfoLabel.Text = "";
            //FreeTextTextBox.Document = new FlowDocument();
            InvoiceNumberTextBox.Text = "";
            CustomerTextBox.Text = "";
            DateTextBox.Text = "";
            BranchTextBox.Text = "";
            FormatReady = false;
        }
        private void GenerateMail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FormatReady)
                {
                    SendEmail.Send(Subject, Body);
                }
                else
                {
                    MessageBox.Show("Не е генериран имейл за изпращане.", "Внимание!");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GenerateProform_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
            Thread t = new Thread(() =>
            {
                List<long> invoiceNumbers = Helper.SplitText(FreeTextTextBox.Dispatcher.Invoke(() => new TextRange(FreeTextTextBox.Document.ContentStart, FreeTextTextBox.Document.ContentEnd).Text));

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
                            if(!NotFound.Any(i => i == invoice))
                                NotFound.Add(invoice);
                        }
                        else
                        {
                            if (invoices.Count != 0)
                            {
                                if (invoices.Any(i => i.InvoiceNumber == sale.DocNo))
                                    continue;
                            }
                            invoices.Add(new InvoiceToSend()
                            {
                                CustomerNumber = Helper.GetControlDigit(sale.CustomerID ?? 0),
                                InvoiceNumber = sale.DocNo,
                                InvoiceDate = sale.DocDate,
                                Branch = (int)Helper.GetBranch(sale.DocNo.ToString())
                            });
                            FreeTextTextBox.Dispatcher.Invoke(() =>
                            {
                                IEnumerable<TextRange> wordRanges = GetAllWordRanges(FreeTextTextBox.Document, sale.DocNo.ToString());
                                foreach (TextRange wordRange in wordRanges)
                                {
                                    if (wordRange.Text == sale.DocNo.ToString())
                                    {
                                        wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightSeaGreen);
                                    }
                                }
                            });
                        }
                    }
                }
                FormatBeforeSend();
                FormatReady = true;
            });
            t.Start();
        }
    }
}
