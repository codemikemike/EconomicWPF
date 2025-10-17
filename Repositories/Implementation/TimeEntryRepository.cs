using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EconomicWPF.Database;
using EconomicWPF.Models;
using EconomicWPF.Repositories.Interfaces;

namespace EconomicWPF.Repositories.Implementation
{
    /// <summary>
    /// Repository implementation for TimeEntry using Entity Framework
    /// </summary>
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly EconomicDbContext _context;

        public TimeEntryRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<TimeEntry>> GetAllAsync()
        {
            return await _context.TimeEntries
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<TimeEntry> GetByIdAsync(int id)
        {
            return await _context.TimeEntries
                .FirstOrDefaultAsync(t => t.TimeEntryId == id);
        }

        public async Task<int> AddAsync(TimeEntry timeEntry)
        {
            timeEntry.CreatedDate = DateTime.Now;

            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();

            return timeEntry.TimeEntryId;
        }

        public async Task<bool> UpdateAsync(TimeEntry timeEntry)
        {
            _context.TimeEntries.Update(timeEntry);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(id);
            if (timeEntry == null)
                return false;

            _context.TimeEntries.Remove(timeEntry);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.TimeEntries.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TimeEntries.AnyAsync(t => t.TimeEntryId == id);
        }

        public async Task<IEnumerable<TimeEntry>> GetByProjectIdAsync(int projectId)
        {
            return await _context.TimeEntries
                .Where(t => t.ProjectId == projectId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeEntry>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.TimeEntries
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.TimeEntries
                .Where(t => t.Date >= fromDate && t.Date <= toDate)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeEntry>> GetBillableEntriesAsync()
        {
            return await _context.TimeEntries
                .Where(t => t.IsBillable)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeEntry>> GetUnbilledEntriesAsync()
        {
            return await _context.TimeEntries
                .Where(t => t.IsBillable && !t.IsInvoiced)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalHoursByProjectIdAsync(int projectId)
        {
            var totalHours = await _context.TimeEntries
                .Where(t => t.ProjectId == projectId)
                .SumAsync(t => t.Hours);

            return totalHours;
        }
    }
}