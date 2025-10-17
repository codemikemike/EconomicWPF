using System;

namespace EconomicWPF.Models
{
    public class VATCode
    {
        public int VATCodeId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public VATCode()
        {
            IsActive = true;
            CreatedDate = DateTime.Now;
        }
    }
}