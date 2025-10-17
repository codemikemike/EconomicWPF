using EconomicWPF.Enums;
using System;

namespace EconomicWPF.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal BudgetAmount { get; set; }
        public ProjectStatus Status { get; set; } // ✅ Ændret fra string til enum
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Navigation
        public Customer Customer { get; set; }

        public Project()
        {
            Status = ProjectStatus.Active; // ✅ Opdateret til enum
            IsActive = true;
            StartDate = DateTime.Today;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}