using System;

namespace EconomicWPF.Models
{
    public class Budget
    {
        public int BudgetId { get; set; }
        public string BudgetName { get; set; }  // ✅ Tilføjet
        public int AccountId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }  // ✅ Tilføjet
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Navigation
        public Account Account { get; set; }
    }
}