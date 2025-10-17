using System;

namespace EconomicWPF.Models
{
    public class TimeEntry
    {
        public int TimeEntryId { get; set; }
        public int ProjectId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string Description { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsBillable { get; set; }
        public bool IsInvoiced { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation
        public Project Project { get; set; }

        public TimeEntry()
        {
            Date = DateTime.Today;
            IsBillable = true;
            IsInvoiced = false;
            CreatedDate = DateTime.Now;
        }
    }
}