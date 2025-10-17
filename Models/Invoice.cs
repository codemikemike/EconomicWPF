using EconomicWPF.Enums;
using System;
using System.Collections.Generic;

namespace EconomicWPF.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public Currency Currency { get; set; }
        public InvoiceStatus Status { get; set; }
        public string? Notes { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // ✅ Navigation properties
        public Customer? Customer { get; set; }
        public List<InvoiceLine>? InvoiceLines { get; set; }
    }
}