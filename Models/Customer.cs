using System;
using System.Collections.Generic;

namespace EconomicWPF.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerNumber { get; set; }
        public string Name { get; set; }
        public string CVR { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public decimal CreditLimit { get; set; }
        public int PaymentTermDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // ✅ Navigation property til relaterede Invoices
        public ICollection<Invoice> Invoices { get; set; }
    }
}