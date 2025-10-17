using System;

namespace EconomicWPF.Models
{
    public class Dimension
    {
        public int DimensionId { get; set; }
        public string DimensionName { get; set; }
        public string DimensionValue { get; set; }
        public int? ParentDimensionId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation
        public Dimension ParentDimension { get; set; }

        public Dimension()
        {
            IsActive = true;
            CreatedDate = DateTime.Now;
        }
    }
}