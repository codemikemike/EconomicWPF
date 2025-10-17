using System;

namespace EconomicWPF.Models
{
    public class InvoiceLine
    {
        public int InvoiceLineId { get; set; }
        public int InvoiceId { get; set; }
        public int? ProductId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VATRate { get; set; }
        public decimal LineTotal { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedDate { get; set; }

        // ✅ Navigation properties
        public Invoice? Invoice { get; set; }
        public Product? Product { get; set; }
        public Account? Account { get; set; }
    }
}