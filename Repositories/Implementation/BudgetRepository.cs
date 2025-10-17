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
    /// Repository implementation for Budget using Entity Framework
    /// </summary>
    public class BudgetRepository : IBudgetRepository
    {
        private readonly EconomicDbContext _context;

        public BudgetRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Budget>> GetAllAsync()
        {
            return await _context.Budgets
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();
        }

        public async Task<Budget> GetByIdAsync(int id)
        {
            return await _context.Budgets
                .FirstOrDefaultAsync(b => b.BudgetId == id);
        }

        public async Task<int> AddAsync(Budget budget)
        {
            budget.CreatedDate = DateTime.Now;

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return budget.BudgetId;
        }

        public async Task<bool> UpdateAsync(Budget budget)
        {
            _context.Budgets.Update(budget);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
                return false;

            _context.Budgets.Remove(budget);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Budgets.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Budgets.AnyAsync(b => b.BudgetId == id);
        }

        public async Task<IEnumerable<Budget>> GetByYearAsync(int year)
        {
            return await _context.Budgets
                .Where(b => b.Year == year)
                .OrderBy(b => b.Month)
                .ToListAsync();
        }

        public async Task<IEnumerable<Budget>> GetByYearAndMonthAsync(int year, int month)
        {
            return await _context.Budgets
                .Where(b => b.Year == year && b.Month == month)
                .OrderBy(b => b.BudgetName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Budget>> GetByAccountIdAsync(int accountId)
        {
            return await _context.Budgets
                .Where(b => b.AccountId == accountId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();
        }

        public async Task<Budget> GetByAccountAndPeriodAsync(int accountId, int year, int month)
        {
            return await _context.Budgets
                .FirstOrDefaultAsync(b => b.AccountId == accountId && b.Year == year && b.Month == month);
        }
    }
}