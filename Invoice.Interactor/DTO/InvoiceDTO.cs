using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.Interactor.DTO
{
    public class InvoiceDTO
    {
        public decimal Amount { get; set; }

        public DateOnly Duedate { get; set; }
    }

    public class CreateInvoiceRespose
    {
        public string Id { get; set; }
    }
}
