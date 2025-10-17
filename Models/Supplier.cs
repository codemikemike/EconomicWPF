using System;

namespace EconomicWPF.Models
{
    public class Supplier
    {
        public int SupplierId { get; set; }
        public string SupplierNumber { get; set; }
        public string Name { get; set; }
        public string CVR { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public int PaymentTermDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}