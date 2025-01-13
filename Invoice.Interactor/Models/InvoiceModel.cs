using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.Interactor.Models
{
    public class InvoiceModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid_amount { get; set; }
        public DateOnly Duedate { get; set; }
        public string Status { get; set; } = "pending";
    }


}
