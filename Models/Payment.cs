using EconomicWPF.Enums;
using System;

namespace EconomicWPF.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } // ✅ Ændret fra string til enum
        public string Reference { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation
        public Invoice Invoice { get; set; }

        public Payment()
        {
            PaymentDate = DateTime.Today;
            PaymentMethod = PaymentMethod.BankTransfer; // ✅ Opdateret til enum
            CreatedDate = DateTime.Now;
        }
    }
}