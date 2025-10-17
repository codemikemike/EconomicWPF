using System;

namespace EconomicWPF.Models
{
    public class BankTransaction
    {
        public int BankTransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public bool IsReconciled { get; set; }
        public int? TransactionId { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation
        public Transaction Transaction { get; set; }

        public BankTransaction()
        {
            TransactionDate = DateTime.Today;
            IsReconciled = false;
            CreatedDate = DateTime.Now;
        }
    }
}