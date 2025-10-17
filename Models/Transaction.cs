using System;

namespace EconomicWPF.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int AccountId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Description { get; set; }
        public string VoucherNumber { get; set; }
        public int? InvoiceId { get; set; }
        public int? ProjectId { get; set; }
        public int? DimensionId { get; set; }
        public bool IsPosted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Navigation
        public Account Account { get; set; }
        public Invoice Invoice { get; set; }
        public Project Project { get; set; }
        public Dimension Dimension { get; set; }

        public Transaction()
        {
            TransactionDate = DateTime.Today;
            IsPosted = false;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}