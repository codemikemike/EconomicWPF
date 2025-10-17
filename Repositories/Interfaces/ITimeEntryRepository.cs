using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface ITimeEntryRepository : IRepository<TimeEntry>
    {
        Task<IEnumerable<TimeEntry>> GetByProjectIdAsync(int projectId);
        Task<IEnumerable<TimeEntry>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<TimeEntry>> GetBillableEntriesAsync();
        Task<IEnumerable<TimeEntry>> GetUnbilledEntriesAsync();
        Task<decimal> GetTotalHoursByProjectIdAsync(int projectId);
    }
}