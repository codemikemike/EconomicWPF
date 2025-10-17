using System;

namespace EconomicWPF.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal VATRate { get; set; }
        public string Unit { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public int AccountId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}